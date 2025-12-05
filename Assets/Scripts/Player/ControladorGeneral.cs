using UnityEngine;
using System.Collections.Concurrent;
using TMPro; // 👈 para usar TMP_Text
using System.Collections;
using System.Collections.Generic;

public class ControladorGeneral : MonoBehaviour
{
    public static ConcurrentQueue<string> colaMovimientos = new ConcurrentQueue<string>();
    public static ConcurrentQueue<int> colaVelocidades = new ConcurrentQueue<int>();

    [Header("Refs")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider col;

    [Header("Forward / Run")]
    [SerializeField] public float runSpeed = 6f;
    private bool isRunning = false;
    private int velocidadActual = 0;

    [Header("Lanes")]
    [SerializeField] private int laneCount = 3;
    [SerializeField] private float laneWidth = 5f;
    [SerializeField] private float laneChangeDuration = 0.35f;
    private float[] lanesX;
    private int laneIdx;
    private float laneStartX, laneTargetX, laneT;
    private int? pendingLane = null;
    public bool IsLaneChanging { get; private set; }

    [Header("Jump")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float cooldownSalto = 1.2f;
    private float tiempoUltimoSalto = -Mathf.Infinity;

    [Header("Checkpoint")]
    [SerializeField] private float checkpointZ = 100f;
    [SerializeField] private TextMeshProUGUI checkpointTexto; // arrastra tu TextMeshPro desde el inspector para el checkpoint

    private bool checkpointAlcanzado = false;

    [Header("Meta")]
    [SerializeField] private float metaZ = 198.69f;
    [SerializeField] private TextMeshProUGUI metaTexto; // arrastra tu TextMeshPro desde el inspector

    private bool metaAlcanzada = false;

    [Header("Estado Alterado")]
    public float penalizacionVelocidad = 0f;

    [Header("Colisión y Retroceso")]
    [SerializeField] private float distanciaRetroceso = 2f;
    [SerializeField] private float duracionRetroceso = 0.3f;
    private bool estaRetrocediendo = false;

    [Header("Control de Hielo")]
    private bool debeCongelarseAlTocarSuelo = false;
    private bool estaCongelado = false;


    [Header("Ignorar colisión con contenedores")]
    [SerializeField] private List<Transform> contenedoresIgnorar = new List<Transform>();

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!col) col = GetComponent<CapsuleCollider>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Carriles centrados (-W, 0, +W)
        lanesX = new float[laneCount];
        int mid = laneCount / 2;
        for (int i = 0; i < laneCount; i++) lanesX[i] = (i - mid) * laneWidth;
        laneIdx = mid;

        // 🔹 BLOQUEAR TODAS LAS ROTACIONES para evitar que se voltee
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationY |
                        RigidbodyConstraints.FreezeRotationZ;

        // Ocultar texto de meta al inicio
        if (metaTexto != null)
            metaTexto.gameObject.SetActive(false);

        // Ocultar texto de checkpoint al inicio
        if (checkpointTexto != null)
            checkpointTexto.gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        // Si está retrocediendo, no hacer nada más
        if (estaRetrocediendo) return;

        // 🔹 FORZAR rotación correcta cada frame (por si acaso)
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        // 🔹 NUEVO: Verificar si debe congelarse por hielo
        VerificarEstadoHielo();

        // Si está congelado, no hacer nada más
        if (estaCongelado) return;

        if (isRunning)
            rb.MovePosition(rb.position + Vector3.forward * runSpeed * Time.fixedDeltaTime);

        if (IsLaneChanging)
        {
            laneT += Time.fixedDeltaTime / laneChangeDuration;
            if (laneT >= 1f) { laneT = 1f; IsLaneChanging = false; }

            float t = Mathf.SmoothStep(0f, 1f, laneT);
            float newX = Mathf.Lerp(laneStartX, laneTargetX, t);
            Vector3 p = rb.position;
            rb.MovePosition(new Vector3(newX, p.y, p.z));

            if (!IsLaneChanging && pendingLane.HasValue)
            {
                int idx = pendingLane.Value; pendingLane = null;
                MoveToLane(idx);
            }
        }
    }

    void Update()
    {
        // Si está retrocediendo, no procesar movimientos ni velocidad
        if (estaRetrocediendo) return;

        // Si está congelado, no procesar nada
        if (estaCongelado) return;
        if (GlobalData.inicio2 == false) return;
        ProcesarMovimientos();
        ProcesarVelocidad();
        //RevisarCheckpoint();
        //RevisarMeta();
    }

    // === Procesar colas ===
    private void ProcesarMovimientos()
    {
        if (metaAlcanzada) return;
        while (colaMovimientos.TryDequeue(out string mov))
        {
            switch (mov)
            {
                case "izq": MoveLeft(); break;
                case "centro": MoveCenter(); break;
                case "der": MoveRight(); break;
                case "jumping": Jump(); break;
                case "jumping2": Jump(); break;

            }
        }
    }

    private void ProcesarVelocidad()
    {
        if (metaAlcanzada) return;

        // Si hay cambios en la cola, actualizamos la base
        while (colaVelocidades.TryDequeue(out int nuevaVel))
        {
            if (nuevaVel != velocidadActual)
            {
                velocidadActual = nuevaVel;

                if (velocidadActual == 0)
                    Stop();
                else
                {
                    SetRun(true);
                    // YA NO calculamos runSpeed directamente aquí, llamamos a la función
                    RecalcularVelocidadFinal();
                }
            }
        }

        // IMPORTANTE: Si hay penalización pero no hay mensajes en la cola, 
        // necesitamos asegurarnos de que la velocidad se mantenga actualizada.
        // Puedes llamar a RecalcularVelocidadFinal() aquí o asegurarte de llamarla
        // cuando actives el poder. Por seguridad, dejémoslo vinculado a la función de abajo.
    }

    // AÑADE ESTA NUEVA FUNCIÓN PÚBLICA
    public void RecalcularVelocidadFinal()
    {
        // Fórmula: (Velocidad Base * Multiplicador) - Penalización por Hielo
        float velocidadCalculada = (3f * velocidadActual);

        // Evitar que la velocidad sea negativa (ir marcha atrás)
        if (velocidadCalculada < 0) velocidadCalculada = 0;

        runSpeed = velocidadCalculada;

        // Opcional: Si la velocidad baja mucho, pausar animación de correr
        // if (runSpeed < 0.1f) anim.speed = 0.5f; else anim.speed = 1f;
    }

    // === API interna ===
    private void SetRun(bool on) => isRunning = on;

    private void Jump()
    {
        if (IsGrounded() && Time.time - tiempoUltimoSalto >= cooldownSalto)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            tiempoUltimoSalto = Time.time;
        }
    }

    private void MoveLeft() => MoveToLane(Mathf.Clamp(laneIdx - 1, 0, laneCount - 1));
    private void MoveRight() => MoveToLane(Mathf.Clamp(laneIdx + 1, 0, laneCount - 1));
    private void MoveCenter() => MoveToLane(laneCount / 2);

    private void Stop()
    {
        isRunning = false;
        rb.linearVelocity = Vector3.zero;
    }

    private void MoveToLane(int idx)
    {
        idx = Mathf.Clamp(idx, 0, laneCount - 1);

        if (IsLaneChanging)
        {
            pendingLane = idx;
            return;
        }

        if (idx == laneIdx && !IsLaneChanging) return;

        laneIdx = idx;
        laneStartX = rb.position.x;
        laneTargetX = lanesX[laneIdx];
        laneT = 0f;
        IsLaneChanging = true;
    }

    private bool IsGrounded()
    {
        if (col != null)
        {
            Vector3 center = col.bounds.center;
            Vector3 bottom = new Vector3(center.x, col.bounds.min.y + 0.05f, center.z);
            float radius = Mathf.Max(0.05f, col.radius * 0.9f);
            return Physics.CheckCapsule(center, bottom, radius, ~0, QueryTriggerInteraction.Ignore);
        }
        return Physics.Raycast(rb.position, Vector3.down, 1.1f);
    }

    // === Checkpoint ===
    private void RevisarCheckpoint()
    {
        if (!checkpointAlcanzado && rb.position.z >= checkpointZ)
        {
            checkpointAlcanzado = true;
            MostrarCheckpoint();
        }
    }

    private void MostrarCheckpoint()
    {
        Debug.Log("¡Pasaste el checkpoint!");

        if (checkpointTexto != null)
        {
            checkpointTexto.gameObject.SetActive(true);
            checkpointTexto.text = "¡Tiempo para Descansar!";
            StartCoroutine(ParpadearTexto(checkpointTexto));
        }
    }

    // === Meta ===
    private void RevisarMeta()
    {
        if (!metaAlcanzada && rb.position.z >= metaZ)
        {
            metaAlcanzada = true;
            MostrarMeta();
        }
    }

    private void MostrarMeta()
    {
        Debug.Log("¡Llegaste a la meta!");

        Stop();
        metaAlcanzada = true;

        // 🔹 Vaciar las colas
        while (colaMovimientos.TryDequeue(out _)) { }
        while (colaVelocidades.TryDequeue(out _)) { }

        if (metaTexto != null)
        {
            metaTexto.gameObject.SetActive(true);
            metaTexto.text = "¡Llegaste a la meta!";
            StartCoroutine(ParpadearTexto(metaTexto));
        }
    }

    // 👇 ESTA ES LA CORUTINA PARA EL PARPADEO
    private IEnumerator ParpadearTexto(TextMeshProUGUI texto)
    {
        while (true)
        {
            texto.enabled = !texto.enabled; // alterna visible/invisible
            yield return new WaitForSeconds(0.5f); // espera 0.5 segundos
        }
    }

    // === SISTEMA DE CONGELAMIENTO POR HIELO ===
    private void VerificarEstadoHielo()
    {
        // Si GlobalData.hielo se activó
        if (GlobalData.hielo && !estaCongelado)
        {
            // Si está en el suelo, congelar inmediatamente
            if (IsGrounded())
            {
                CongelarJugador();
            }
            else
            {
                // Si está en el aire, marcar para congelar cuando toque el suelo
                debeCongelarseAlTocarSuelo = true;
            }
        }

        // Si debe congelarse al tocar suelo Y ya tocó el suelo
        if (debeCongelarseAlTocarSuelo && IsGrounded())
        {
            CongelarJugador();
            debeCongelarseAlTocarSuelo = false;
        }

        // Si GlobalData.hielo se desactiva, descongelar
        if (!GlobalData.hielo && estaCongelado)
        {
            DescongelarJugador();
        }
    }

    private void CongelarJugador()
    {
        estaCongelado = true;

        // Detener completamente el movimiento
        isRunning = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Vaciar todas las colas de movimiento
        while (colaMovimientos.TryDequeue(out _)) { }
        while (colaVelocidades.TryDequeue(out _)) { }

        Debug.Log("🧊 Jugador CONGELADO por hielo");
    }

    private void DescongelarJugador()
    {
        estaCongelado = false;
        debeCongelarseAlTocarSuelo = false;

        Debug.Log("🔥 Jugador DESCONGELADO");
    }

    // === DETECCIÓN DE COLISIÓN FRONTAL ===
    private void OnCollisionEnter(Collision collision)
    {
        // 🔹 RESETEAR rotación inmediatamente al chocar

        if (collision.gameObject.CompareTag("Ground")) return;
        foreach (Transform cont in contenedoresIgnorar)
        {
            if (cont != null && collision.transform.IsChildOf(cont))
                return;
        }
        // 🟦 DEBUG COMPLETO DE LA COLISIÓN
        string nombre = collision.gameObject.name;
        string padre = collision.transform.parent != null ? collision.transform.parent.name : "SIN PADRE";
        Vector3 punto = collision.contacts[0].point;

        Debug.Log($"⚠️ COLISIÓN DETECTADA\n" +
                  $"Objeto: {nombre}\n" +
                  $"Padre: {padre}\n" +
                  $"Tag: {collision.gameObject.tag}\n" +
                  $"Punto de contacto: {punto}");

        // aquí sigue tu código..
        transform.rotation = Quaternion.identity;
        rb.angularVelocity = Vector3.zero; // Detener cualquier rotación angular

        // Verificar si la colisión es frontal (el objeto está adelante del corredor)
        Vector3 direccionColision = collision.contacts[0].point - rb.position;

        // Si el punto de contacto está adelante (z positivo respecto al corredor)
        if (direccionColision.z > 0.1f && !estaRetrocediendo)
        {
            Debug.Log($"¡Colisión frontal con {collision.gameObject.name}!");
            StartCoroutine(Retroceder());
        }
    }

    private IEnumerator Retroceder()
    {
        estaRetrocediendo = true;

        // 🔹 FORZAR rotación correcta
        transform.rotation = Quaternion.identity;
        rb.angularVelocity = Vector3.zero;

        // 🔹 Vaciar las colas para ignorar movimientos pendientes
        while (colaMovimientos.TryDequeue(out _)) { }
        while (colaVelocidades.TryDequeue(out _)) { }

        // Pausar el movimiento hacia adelante
        bool estabaCorreindo = isRunning;
        isRunning = false;

        // Calcular posición de retroceso
        Vector3 posicionInicial = rb.position;
        Vector3 posicionRetroceso = posicionInicial + Vector3.back * distanciaRetroceso;

        float tiempoTranscurrido = 0f;

        // Animar el retroceso
        while (tiempoTranscurrido < duracionRetroceso)
        {
            tiempoTranscurrido += Time.deltaTime;
            float t = tiempoTranscurrido / duracionRetroceso;

            // Interpolación suave
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            Vector3 nuevaPosicion = Vector3.Lerp(posicionInicial, posicionRetroceso, smoothT);

            rb.MovePosition(nuevaPosicion);

            // 🔹 Mantener rotación correcta durante todo el retroceso
            transform.rotation = Quaternion.identity;

            yield return null;
        }

        // Asegurar que llegue a la posición final
        rb.MovePosition(posicionRetroceso);

        // 🔹 Última verificación de rotación
        transform.rotation = Quaternion.identity;
        rb.angularVelocity = Vector3.zero;

        // Restaurar el estado de correr si estaba corriendo antes
        if (estabaCorreindo)
            isRunning = true;

        estaRetrocediendo = false;
    }
}