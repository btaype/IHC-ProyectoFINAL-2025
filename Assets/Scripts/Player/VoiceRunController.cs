using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

public class VoiceRunController : MonoBehaviour
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> acciones;
    private GameObject player;
    private float moveSpeed = 3f; 
    private bool movingForward = false;

    void Start()
    {
        // Detecta micrófonos disponibles
        foreach (var device in Microphone.devices)
        {
            Debug.Log("Microphone detected: " + device);
        }

        player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Player GameObject not found. Asegúrate de que exista un objeto llamado 'Player'.");
            return;
        }

        // Solo comandos de correr/detenerse
        acciones = new Dictionary<string, System.Action>(System.StringComparer.OrdinalIgnoreCase)
        {
            // Variantes de "correr"
            { "correr", () => { Debug.Log("[VOZ] Acción: CORRER"); movingForward = true; }},
            { "corer", () => { Debug.Log("[VOZ] Acción: CORRER (corer detected)"); movingForward = true; }},
            { "corrre", () => { Debug.Log("[VOZ] Acción: CORRER (corrre detected)"); movingForward = true; }},
            { "korrer", () => { Debug.Log("[VOZ] Acción: CORRER (korrer detected)"); movingForward = true; }},
            { "corre", () => { Debug.Log("[VOZ] Acción: CORRER (corre detected)"); movingForward = true; }},

            // Variantes de "detente"
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