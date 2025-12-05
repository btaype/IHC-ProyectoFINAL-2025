using UnityEngine;
using System.Text;
using System.Globalization;
using Vosk;
using System.Collections;
using UnityEngine.UI;

public class vozMenuNuevo : MonoBehaviour
{
    [Header("Configuración de Vosk")]
    public string modelRelativePath = "models/es";
    public int targetSampleRate = 16000;

    [Header("Referencias del juego")]
    public VideoButtonController videoController;
    public MainMenu mainMenu;

    [Header("Botones de jugador")]
    public Button botonJugador1;   // 🟢 Lo asignas en el inspector (corredor)
    public Button botonJugador2;   // 🔵 Lo asignas en el inspector (enemigo)

    [Header("Audio local del menú")]
    public AudioSource audioSource;
    public AudioClip audioInstrucciones;
    public float tiempoRepeticion = 20f;

    private Model model;
    private VoskRecognizer recognizer;
    private AudioClip mic;
    private string deviceName = null;
    private float[] floatBuf;
    private byte[] byteBuf;
    private int samplesPerChunk;
    private int lastSamplePos = 0;
    private Coroutine repetirAudioCoroutine;

    private bool enTutorial = false;

    void Start()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Configurar Vosk
        Vosk.Vosk.SetLogLevel(0);
        string modelPath = System.IO.Path.Combine(Application.streamingAssetsPath, modelRelativePath);
        model = new Model(modelPath);
        recognizer = new VoskRecognizer(model, targetSampleRate);

        mic = Microphone.Start(deviceName, true, 1, targetSampleRate);
        while (Microphone.GetPosition(deviceName) <= 0) { }

        samplesPerChunk = Mathf.CeilToInt(targetSampleRate * 0.02f);
        floatBuf = new float[samplesPerChunk];
        byteBuf = new byte[samplesPerChunk * 2];

        repetirAudioCoroutine = StartCoroutine(RepetirAudioLocal());
    }

    IEnumerator RepetirAudioLocal()
    {
        while (true)
        {
            if (!enTutorial && audioInstrucciones != null)
            {
                audioSource.clip = audioInstrucciones;
                audioSource.Play();
                yield return new WaitForSeconds(audioInstrucciones.length + tiempoRepeticion);
            }
            else
            {
                yield return new WaitForSeconds(tiempoRepeticion);
            }
        }
    }

    void Update()
    {
        if (mic == null) return;

        int pos = Microphone.GetPosition(deviceName);
        int delta = pos - lastSamplePos;
        if (delta < 0) delta += mic.samples;

        while (delta >= samplesPerChunk)
        {
            mic.GetData(floatBuf, lastSamplePos);

            for (int i = 0; i < samplesPerChunk; i++)
            {
                short s = (short)Mathf.Clamp(floatBuf[i] * 32767f, short.MinValue, short.MaxValue);
                byteBuf[2 * i] = (byte)(s & 0xff);
                byteBuf[2 * i + 1] = (byte)((s >> 8) & 0xff);
            }

            if (recognizer.AcceptWaveform(byteBuf, byteBuf.Length))
            {
                string result = recognizer.Result();
                Debug.Log("🎤 Reconocido: " + result);
                ProcesarComando(result);
            }

            lastSamplePos = (lastSamplePos + samplesPerChunk) % mic.samples;
            delta -= samplesPerChunk;
        }
    }

    void ProcesarComando(string json)
    {
        json = QuitarTildes(json.ToLower());

        string[] comandosVerTutorial = { "ver tutorial", "abrir tutorial", "tutorial" };
        string[] comandosSalirTutorial = { "regresar menu", "regresar a menu" };
        string[] comandosIniciarCorredor = { "iniciar corredor", "empezar corredor", "correr" };
        string[] comandosIniciarEnemigo = { "iniciar enemigo", "empezar enemigo", "enemigo" };

        // --- Tutorial ---
        foreach (string cmd in comandosVerTutorial)
        {
            if (Similarity(json, cmd) >= 0.25f)
            {
                DetenerAudioMenu();
                videoController.PlayVideo();
                enTutorial = true;
                return;
            }
        }

        foreach (string cmd in comandosSalirTutorial)
        {
            if (Similarity(json, cmd) >= 0.4f)
            {
                videoController.StopVideoFromVoice();
                enTutorial = false;
                ReanudarAudioMenu();
                return;
            }
        }

        // --- Jugador 1 (Corredor) ---
        foreach (string cmd in comandosIniciarCorredor)
        {
            if (Similarity(json, cmd) >= 0.25f)
            {
                if (botonJugador1 != null)
                    botonJugador1.onClick.Invoke();

                enTutorial = false;
                DetenerAudioMenu();
                return;
            }
        }

        // --- Jugador 2 (Enemigo) ---
        foreach (string cmd in comandosIniciarEnemigo)
        {
            if (Similarity(json, cmd) >= 0.35f)
            {
                if (botonJugador2 != null)
                    botonJugador2.onClick.Invoke();

                enTutorial = false;
                DetenerAudioMenu();
                return;
            }
        }
    }

    void DetenerAudioMenu()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
    }

    void ReanudarAudioMenu()
    {
        if (audioInstrucciones != null && !audioSource.isPlaying)
        {
            audioSource.clip = audioInstrucciones;
            audioSource.Play();
        }
    }

    string QuitarTildes(string texto)
    {
        string normalizado = texto.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();
        foreach (char c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];
        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Mathf.Min(
                    Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }

    float Similarity(string a, string b)
    {
        int dist = LevenshteinDistance(a, b);
        int maxL = Mathf.Max(a.Length, b.Length);
        return 1f - (float)dist / maxL;
    }

    void OnDestroy()
    {
        if (mic != null) Microphone.End(deviceName);
        recognizer?.Dispose();
        model?.Dispose();
    }
}
