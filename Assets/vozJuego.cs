using UnityEngine;
using Vosk;
using System;
using System.Globalization;
using System.Text;

public class vozJuego : MonoBehaviour
{
    public string modelRelativePath = "models/es";
    private Model model;
    private VoskRecognizer recognizer;
    private AudioClip mic;
    private string deviceName = null;
    private float[] floatBuf;
    private byte[] byteBuf;
    private int samplesPerChunk;
    private int lastSamplePos = 0;
    public int targetSampleRate = 16000;

    void Start()
    {
        Vosk.Vosk.SetLogLevel(0);
        string modelPath = System.IO.Path.Combine(Application.streamingAssetsPath, modelRelativePath);
        model = new Model(modelPath);
        recognizer = new VoskRecognizer(model, targetSampleRate);
        mic = Microphone.Start(deviceName, true, 1, targetSampleRate);
        while (Microphone.GetPosition(deviceName) <= 0) { }

        samplesPerChunk = Mathf.CeilToInt(targetSampleRate * 0.02f);
        floatBuf = new float[samplesPerChunk];
        byteBuf = new byte[samplesPerChunk * 2];
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
                Debug.Log("Reconocido (Juego): " + result);
                ProcesarComando(result);
            }

            lastSamplePos = (lastSamplePos + samplesPerChunk) % mic.samples;
            delta -= samplesPerChunk;
        }
    }

    string QuitarTildes(string texto)
    {
        string normalizado = texto.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();

        foreach (char c in normalizado)
        {
            UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat != UnicodeCategory.NonSpacingMark)
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
        a = a.ToLower();
        b = b.ToLower();
        int distance = LevenshteinDistance(a, b);
        int maxLen = Mathf.Max(a.Length, b.Length);
        return 1f - (float)distance / maxLen;
    }
    void ProcesarComando(string json)
    {
        json = json.ToLower();
        json = QuitarTildes(json);

        string[] comandos = { "volver al menu confirmado", "regresar al menu confirmado", "salir del juego confirmado" };

        foreach (string comando in comandos)
        {
            float similitud = Similarity(json, comando);
            Debug.Log($"Comparando con '{comando}' → similitud {similitud * 100:F1}%");

            if (similitud >= 0.40f) 
            {
                if (comando.Contains("menu"))
                {
                    Debug.Log("🟢 Volviendo al menú principal...");
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
                }
                else if (comando.Contains("salir"))
                {
                    Debug.Log("🟥 Cerrando el juego...");
                    Application.Quit();
                }
                return; 
            }
        }
    }


    void OnDestroy()
    {
        if (mic != null) Microphone.End(deviceName);
        recognizer?.Dispose();
        model?.Dispose();
    }
}
