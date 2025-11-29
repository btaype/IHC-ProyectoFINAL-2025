using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

public class VoiceController : MonoBehaviour
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> acciones;
    private GameObject player;
    private Rigidbody playerRigidbody;
    private float[] lanes = new float[] { -5f, 0f, 5f }; // Match PlayerMovement
    private int currentLane = 1;
    private float laneChangeSpeed = 10f; // Match PlayerMovement
    private bool isMoving = false;
    private bool movingForward = false;
    private float moveSpeed = 3f; // Match PlayerMovement

    void Start()
    {
        // Log available microphones
        foreach (var device in Microphone.devices)
        {
            Debug.Log("Microphone detected: " + device);
        }

        player = GameObject.Find("Player");
        if (player != null)
        {
            playerRigidbody = player.GetComponent<Rigidbody>();
        }
        if (player == null || playerRigidbody == null)
        {
            Debug.LogError("Player GameObject or Rigidbody not found. Ensure a GameObject named 'Player' with a Rigidbody exists.");
            return;
        }

        // Define Spanish voice commands and their variants
        acciones = new Dictionary<string, System.Action>(System.StringComparer.OrdinalIgnoreCase)
        {
            // Correr variants
            { "correr", () => { Debug.Log("[VOZ] Acción: CORRER"); movingForward = true; }},
            { "corer", () => { Debug.Log("[VOZ] Acción: CORRER (corer detected)"); movingForward = true; }},
            { "corrre", () => { Debug.Log("[VOZ] Acción: CORRER (corrre detected)"); movingForward = true; }},
            { "korrer", () => { Debug.Log("[VOZ] Acción: CORRER (korrer detected)"); movingForward = true; }},
            { "corre", () => { Debug.Log("[VOZ] Acción: CORRER (corre detected)"); movingForward = true; }},

            // Saltar variants
            { "saltar", () => { if (IsGrounded()) { Debug.Log("[VOZ] Acción: SALTAR"); playerRigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse); } }},
            { "saltarr", () => { if (IsGrounded()) { Debug.Log("[VOZ] Acción: SALTAR (saltarr detected)"); playerRigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse); } }},
            { "salta", () => { if (IsGrounded()) { Debug.Log("[VOZ] Acción: SALTAR (salta detected)"); playerRigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse); } }},
            { "zaltar", () => { if (IsGrounded()) { Debug.Log("[VOZ] Acción: SALTAR (zaltar detected)"); playerRigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse); } }},
            { "saltarrr", () => { if (IsGrounded()) { Debug.Log("[VOZ] Acción: SALTAR (saltarrr detected)"); playerRigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse); } }},

            // Saltar mas alto variants
            { "saltar mas alto", () => { if (IsGrounded()) { Debug.Log("[VOZ] Acción: SALTAR MÁS ALTO"); playerRigidbody.AddForce(Vector3.up * 7f, ForceMode.Impulse); } }},
            { "saltar más alto", () => { if (IsGrounded()) { Debug.Log("[VOZ] Acción: SALTAR MÁS ALTO (saltar más alto detected)"); playerRigidbody.AddForce(Vector3.up * 7f, ForceMode.Impulse); } }},
            { "saltar alto", () => { if (IsGrounded()) { Debug.Log("[VOZ] Acción: SALTAR MÁS ALTO (saltar alto detected)"); playerRigidbody.AddForce(Vector3.up * 7f, ForceMode.Impulse); } }},
            { "salta mas alto", () => { if (IsGrounded()) { Debug.Log("[VOZ] Acción: SALTAR MÁS ALTO (salta mas alto detected)"); playerRigidbody.AddForce(Vector3.up * 7f, ForceMode.Impulse); } }},
            { "salta alto", () => { if (IsGrounded()) { Debug.Log("[VOZ] Acción: SALTAR MÁS ALTO (salta alto detected)"); playerRigidbody.AddForce(Vector3.up * 7f, ForceMode.Impulse); } }},

            // Izquierda variants
            { "izquierda", () => { Debug.Log("[VOZ] Acción: IZQUIERDA"); if (!isMoving) ChangeLane(-1); }},
            { "iquierda", () => { Debug.Log("[VOZ] Acción: IZQUIERDA (iquierda detected)"); if (!isMoving) ChangeLane(-1); }},
            { "izkierda", () => { Debug.Log("[VOZ] Acción: IZQUIERDA (izkierda detected)"); if (!isMoving) ChangeLane(-1); }},
            { "isquierda", () => { Debug.Log("[VOZ] Acción: IZQUIERDA (isquierda detected)"); if (!isMoving) ChangeLane(-1); }},
            { "izquierd", () => { Debug.Log("[VOZ] Acción: IZQUIERDA (izquierd detected)"); if (!isMoving) ChangeLane(-1); }},
            { "izqierda", () => { Debug.Log("[VOZ] Acción: IZQUIERDA (izqierda detected)"); if (!isMoving) ChangeLane(-1); }},

            // Derecha variants
            { "derecha", () => { Debug.Log("[VOZ] Acción: DERECHA"); if (!isMoving) ChangeLane(1); }},
            { "derech", () => { Debug.Log("[VOZ] Acción: DERECHA (derech detected)"); if (!isMoving) ChangeLane(1); }},
            { "drecha", () => { Debug.Log("[VOZ] Acción: DERECHA (drecha detected)"); if (!isMoving) ChangeLane(1); }},
            { "dercha", () => { Debug.Log("[VOZ] Acción: DERECHA (dercha detected)"); if (!isMoving) ChangeLane(1); }},
            { "derezha", () => { Debug.Log("[VOZ] Acción: DERECHA (derezha detected)"); if (!isMoving) ChangeLane(1); }},

            // Detente variants
            { "detente", () => { Debug.Log("[VOZ] Acción: DETENTE"); movingForward = false; }},
            { "deten", () => { Debug.Log("[VOZ] Acción: DETENTE (deten detected)"); movingForward = false; }},
            { "detent", () => { Debug.Log("[VOZ] Acción: DETENTE (detent detected)"); movingForward = false; }},
            { "dente", () => { Debug.Log("[VOZ] Acción: DETENTE (dente detected)"); movingForward = false; }},
            { "detentte", () => { Debug.Log("[VOZ] Acción: DETENTE (detentte detected)"); movingForward = false; }}
        };

        var palabras = acciones.Keys.ToArray();
        if (palabras.Length == 0)
        {
            Debug.LogError("No hay palabras clave configuradas.");
            return;
        }

        Debug.Log("Comandos registrados: " + string.Join(", ", palabras));
        Debug.Log("Estado del reconocimiento: " + PhraseRecognitionSystem.Status);

        keywordRecognizer = new KeywordRecognizer(palabras, ConfidenceLevel.Low);
        keywordRecognizer.OnPhraseRecognized += OnFraseReconocida;
        keywordRecognizer.Start();

        Debug.Log("Reconocedor iniciado. Di alguna de estas palabras: " + string.Join(", ", palabras));
    }

    void Update()
    {
        if (movingForward)
        {
            Debug.Log("Moving forward at speed: " + moveSpeed);
            player.transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed, Space.World);
        }

        Vector3 targetPosition = new Vector3(lanes[currentLane], player.transform.position.y, player.transform.position.z);
        player.transform.position = Vector3.Lerp(player.transform.position, targetPosition, Time.deltaTime * laneChangeSpeed);

        if (Vector3.Distance(player.transform.position, targetPosition) < 0.01f)
        {
            isMoving = false;
            player.transform.position = targetPosition;
        }
    }

    private void ChangeLane(int direction)
    {
        int newLane = currentLane + direction;
        if (newLane >= 0 && newLane < lanes.Length)
        {
            currentLane = newLane;
            isMoving = true;
            Debug.Log($"Changing lane to {currentLane} (x = {lanes[currentLane]})");
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(player.transform.position, Vector3.down, 1.1f);
    }

    private void OnFraseReconocida(PhraseRecognizedEventArgs args)
    {
        Debug.Log($"Heard: \"{args.text}\" (Confidence: {args.confidence})");

        if (acciones.TryGetValue(args.text, out var accion))
        {
            accion.Invoke();
        }
        else
        {
            Debug.Log($"[VOZ] No action mapped for: \"{args.text}\"");
        }
    }

    private void OnApplicationQuit()
    {
        if (keywordRecognizer != null)
        {
            if (keywordRecognizer.IsRunning) keywordRecognizer.Stop();
            keywordRecognizer.OnPhraseRecognized -= OnFraseReconocida;
            keywordRecognizer.Dispose();
        }
    }
#else
    Debug.LogError("El reconocimiento nativo funciona solo en Windows. En Mac/Linux usa Vosk o un servicio en la nube.");
#endif
}