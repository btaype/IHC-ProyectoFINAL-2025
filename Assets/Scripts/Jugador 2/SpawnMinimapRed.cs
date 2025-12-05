using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using LiteNetLib;
using LiteNetLib.Utils;

public class SpawnMinimapRed : MonoBehaviour
{
    [Header("Cámaras")]
    public Camera camaraOrtogonal;
    public Camera camaraPrincipal;

    [Header("Minimap UI")]
    public RawImage rawImageMinimap;

    [Header("Configuración")]
    public float[] carrilesX = { 5f, 0f, -5f };
    public float alturaFija = 1f;

    [Header("Prefabs (dentro del script)")]
    public GameObject prefabTorreta1;
    public GameObject prefabTorreta2;

    [Header("Botones")]
    public Button[] botonesObjetos;
    public Button btnCancelar;

    [Header("Cooldown Visual")]
    public Image cooldown1;
    public Image cooldown2;
    public float tiempoCooldown = 5f;

    [Header("Red")]
    public Client client;

    private int numeroJugador = 0;
    private GameObject objetoSeleccionado;
    private bool puedeColocar = false;

    // 🆕 NUEVO: Para la luz de preview
    private GameObject previewObj;
    private Color previewColor;

    void Start()
    {
        Debug.Log("spawncooldown INICIADO");

        StartCoroutine(EsperarReferencias());

        // Botones originales (🆕 con colores para la luz)
        if (botonesObjetos.Length >= 2)
        {
            botonesObjetos[0].onClick.AddListener(() =>
            {
                Seleccionar(prefabTorreta1, "Torreta1", Color.red);
            });

            botonesObjetos[1].onClick.AddListener(() =>
            {
                Seleccionar(prefabTorreta2, "Torreta2", Color.cyan);
            });
        }

        if (btnCancelar) btnCancelar.onClick.AddListener(Cancelar);

        // Minimap
        if (rawImageMinimap != null)
        {
            var trigger = rawImageMinimap.gameObject.GetComponent<EventTrigger>() ??
                          rawImageMinimap.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((data) => ClickEnMinimap((PointerEventData)data));
            trigger.triggers.Clear();
            trigger.triggers.Add(entry);
            rawImageMinimap.raycastTarget = true;
            Debug.Log("EventTrigger agregado al RawImage");
        }
        else
        {
            Debug.LogError("RawImageMinimap NO ASIGNADO EN INSPECTOR");
        }

        // Inicializar cooldown visual
        if (cooldown1 != null) cooldown1.fillAmount = 0;
        if (cooldown2 != null) cooldown2.fillAmount = 0;
    }

    IEnumerator EsperarReferencias()
    {
        // Cargar número de jugador
        numeroJugador = PlayerPrefs.GetInt("NumeroJugador", 0);
        Debug.Log($"Número de jugador: {numeroJugador}");

        // Esperar al cliente
        while (client == null)
        {
            client = FindObjectOfType<Client>();
            if (client != null)
            {
                Debug.Log("✅ Cliente encontrado en spawncooldown");
                break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void Update()
    {
        RevisarPoderesEspeciales();

        if (!puedeColocar || objetoSeleccionado == null) return;

        // 🆕 ACTUALIZAR PREVIEW / LUZ GUIA
        ActualizarPreview();

        // Colocación original (solo si NO hay render texture en cámara ortogonal)
        if (camaraOrtogonal.targetTexture == null)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                ColocarConCamara(camaraOrtogonal);
            }
        }
    }

    // 🆕 NUEVO: Actualizar posición y pulso de la luz de preview
    void ActualizarPreview()
    {
        if (previewObj == null || camaraOrtogonal == null || rawImageMinimap == null) return;

        // Detectar si el mouse está sobre el minimap (no mostrar preview ahí, para clics precisos)
        RectTransform minimapRT = rawImageMinimap.rectTransform;
        bool mouseSobreMinimap = RectTransformUtility.RectangleContainsScreenPoint(minimapRT, Input.mousePosition, null);

        if (mouseSobreMinimap) return; // No actualizar preview si mouse sobre minimap

        Ray rayPreview = camaraOrtogonal.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(rayPreview, out RaycastHit hitPreview))
        {
            int carril = EncontrarCarril(hitPreview.point.x);
            Vector3 posPreview = new Vector3(carrilesX[carril], alturaFija, hitPreview.point.z);

            previewObj.transform.position = posPreview;

            // 💡 Pulso dinámico en la luz
            Light lightPreview = previewObj.GetComponent<Light>();
            if (lightPreview != null)
            {
                lightPreview.intensity = 1.5f + (Mathf.Sin(Time.time * 8f) * 0.8f);
            }
        }
    }

    void ClickEnMinimap(PointerEventData data)
    {
        Debug.Log("CLICK EN MINIMAP DETECTADO!");

        if (!puedeColocar || objetoSeleccionado == null)
        {
            Debug.Log("No hay objeto seleccionado");
            return;
        }

        if (rawImageMinimap.texture == null) return;

        RectTransform rt = rawImageMinimap.rectTransform;
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rt, data.position, data.pressEventCamera, out localPoint))
        {
            Vector2 norm = new Vector2(
                (localPoint.x - rt.rect.xMin) / rt.rect.width,
                (localPoint.y - rt.rect.yMin) / rt.rect.height
            );

            Vector2 pixel = new Vector2(
                norm.x * rawImageMinimap.texture.width,
                norm.y * rawImageMinimap.texture.height
            );

            Ray ray = camaraOrtogonal.ScreenPointToRay(pixel);
            ColocarConRayo(ray);
        }
    }

    void ColocarConRayo(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            int carril = EncontrarCarril(hit.point.x);
            Vector3 pos = new Vector3(carrilesX[carril], alturaFija, hit.point.z);

            GameObject obstaculoColocado = Instantiate(objetoSeleccionado, pos, Quaternion.identity);

            Debug.Log($"🧱 OBJETO COLOCADO → Carril {carril + 1} (X={carrilesX[carril]}, Z={hit.point.z:F1})");

            // 🔥 ENVIAR AL SERVIDOR
            if (GlobalData.forcedConstruction != -1)
            {
                GlobalData.forcedConstruction = -1;
                Debug.Log("COMBO COMPLETADO → Botones liberados");
            }
            if (client != null)
            {
                // Determinar el índice del obstáculo (1 o 2)
                int indexObstaculo = (objetoSeleccionado == prefabTorreta1) ? 1 : 2;

                EstadoPosicion estado = new EstadoPosicion
                {
                    jugador = numeroJugador,
                    pos_x = pos.x,
                    pos_y = pos.y,
                    pos_z = pos.z,
                    obstaculo = indexObstaculo
                };

                string json = JsonUtility.ToJson(estado);

                var writer = new NetDataWriter();
                writer.Put(json);

                client.Server.Send(writer, DeliveryMethod.Sequenced);

                Debug.Log($"📤 Obstáculo enviado: {json}");
            }
            else
            {
                Debug.LogWarning("⚠ No se encontró cliente para enviar el obstáculo");
            }

            // 🔥 Iniciar cooldown según el prefab seleccionado
            if (objetoSeleccionado == prefabTorreta1)
                IniciarCooldown(0);

            if (objetoSeleccionado == prefabTorreta2)
                IniciarCooldown(1);
        }
        else
        {
            Debug.LogWarning("Raycast NO pegó");
        }

        Cancelar();
    }

    void ColocarConCamara(Camera cam)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        ColocarConRayo(ray);
    }

    // 🆕 MODIFICADO: Seleccionar con color para luz
    void Seleccionar(GameObject prefab, string nombre, Color luzColor = default)
    {
        // Destruir preview anterior
        if (previewObj != null)
        {
            Destroy(previewObj);
            previewObj = null;
        }

        objetoSeleccionado = prefab;
        previewColor = luzColor == default ? Color.white : luzColor;
        puedeColocar = true;

        // 🆕 Crear la luz de preview
        CrearPreviewLuz();

        Debug.Log($"🎯 SELECCIONADO: {nombre} (luz: {previewColor})");
    }

    // 🆕 NUEVO: Crear la esfera luminosa de guía
    void CrearPreviewLuz()
    {
        previewObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        previewObj.name = "PreviewLuzGuia";
        previewObj.transform.SetParent(transform);
        previewObj.transform.localRotation = Quaternion.identity;

        // Tamaño (ajusta según base de tus torretas)
        previewObj.transform.localScale = Vector3.one * 1.2f;

        // Quitar collider (no queremos interferir)
        Collider col = previewObj.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Material emisivo semitransparente
        Renderer rend = previewObj.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(previewColor.r, previewColor.g, previewColor.b, 0.4f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", previewColor * 2.5f);
        rend.material = mat;

        // 💡 Luz puntual
        Light luz = previewObj.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = previewColor;
        luz.intensity = 1.5f;
        luz.range = 8f;
        luz.shadows = LightShadows.None; // Sin sombras para performance
    }

    int EncontrarCarril(float x)
    {
        int best = 0;
        float minDist = Mathf.Abs(carrilesX[0] - x);
        for (int i = 1; i < carrilesX.Length; i++)
        {
            float d = Mathf.Abs(carrilesX[i] - x);
            if (d < minDist) { minDist = d; best = i; }
        }
        return best;
    }

    // 🆕 MODIFICADO: Cancelar destruye la preview
    void Cancelar()
    {
        puedeColocar = false;
        objetoSeleccionado = null;

        if (previewObj != null)
        {
            Destroy(previewObj);
            previewObj = null;
        }
    }

    // ---------------------------------------------------------------------
    // ----------------------- SISTEMA DE COOLDOWN -------------------------
    // ---------------------------------------------------------------------

    void IniciarCooldown(int index)
    {
        Button btn = botonesObjetos[index];
        Image img = (index == 0) ? cooldown1 : cooldown2;

        btn.interactable = false;
        StartCoroutine(CooldownRutina(btn, img));
    }

    IEnumerator CooldownRutina(Button btn, Image overlay)
    {
        float t = tiempoCooldown;

        if (overlay != null)
            overlay.transform.localScale = Vector3.one;

        while (t > 0)
        {
            t -= Time.deltaTime;

            if (overlay != null)
            {
                float factor = t / tiempoCooldown;
                overlay.transform.localScale = new Vector3(factor, 1f, 1f);
            }

            yield return null;
        }

        if (overlay != null)
            overlay.transform.localScale = Vector3.zero;

        btn.interactable = true;
    }

    void RevisarPoderesEspeciales()
    {
        if (client == null) return;

        if (GlobalData.mancha && numeroJugador == 2)
        {
            EstadoPosicion estado = new EstadoPosicion
            {
                jugador = numeroJugador,
                pos_x = 0f,
                pos_y = 0f,
                pos_z = 0f,
                obstaculo = 5
            };
            GlobalData.forcedConstruction = 0;
            string json = JsonUtility.ToJson(estado);
            var writer = new NetDataWriter();
            writer.Put(json);
            client.Server.Send(writer, DeliveryMethod.Sequenced);

            Debug.Log($"📤 Poder Mancha enviado: {json}");
            GlobalData.mancha = false; // opcional: resetear después de enviar
        }

        if (GlobalData.hielo && numeroJugador == 2 && GlobalData.espera == true)
        {
            GlobalData.espera = false;
            EstadoPosicion estado = new EstadoPosicion
            {
                jugador = numeroJugador,
                pos_x = 0f,
                pos_y = 0f,
                pos_z = 0f,
                obstaculo = 6
            };

            string json = JsonUtility.ToJson(estado);
            var writer = new NetDataWriter();
            writer.Put(json);
            client.Server.Send(writer, DeliveryMethod.Sequenced);

            Debug.Log($"📤 Poder Hielo enviado: {json}");

        }
    }
}