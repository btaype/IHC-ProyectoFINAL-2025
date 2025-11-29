using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Text;
using Vosk; 

public class VoskSpeech : MonoBehaviour
{
    [Header("Modelo (relativo a StreamingAssets)")]
    public string modelRelativePath = "models/es";

    [Header("Audio")]
    public int targetSampleRate = 16000; // Vosk recomienda 16k mono
    public int chunkMs = 15;             // 20–40 ms va bien

    [Header("Matching")]
    [Range(0f, 1f)] public float fuzzyThreshold = 0.60f;
    public bool showPartialInConsole = false;


    [Header("Low-latency")]
    public bool earlyFireFromPartial = true;
    [Range(0f, 1f)] public float partialThreshold = 0.70f; 
    [Tooltip("Cuántos frames parciales consecutivos deben coincidir")]
    public int partialStableFrames = 2;
    public double partialCooldownSec = 0.50;
    public bool oneCommandPerUtterance = true;
    public int minSilenceToUnlockMs = 250;   // pausa mínima para aceptar el siguiente
    public float vadSilenceRms = 0.006f;     // umbral de silencio (ajústalo)

    private bool lockedUntilSilence = false; // bloqueado tras disparar
    private int currentSilenceMs = 0;
    private Model model;
    private VoskRecognizer recognizer;
    private AudioClip mic;
    private string deviceName = null;
    private float[] floatBuf;
    private byte[] byteBuf;
    private int samplesPerChunk;
    private int lastSamplePos = 0;
    private ConcurrentQueue<string> results = new ConcurrentQueue<string>();
    private StringBuilder transcript = new StringBuilder();

    
    private string lastCmd = null;
    private double lastCmdTime = 0;
    public double debounceSeconds = 0.35;
    private string lastPartialBest = null;
    private int partialStableCount = 0;
    private double lastFireTime = 0;
    void Start()
    {
        Application.runInBackground = true;
        Vosk.Vosk.SetLogLevel(0);

        string modelPath = System.IO.Path.Combine(Application.streamingAssetsPath, modelRelativePath);
        model = new Model(modelPath);

        string grammarJson = BuildGrammar(VoiceCommands.COMANDOS);
        recognizer = new VoskRecognizer(model, targetSampleRate, grammarJson);

        mic = Microphone.Start(deviceName, true, 1, targetSampleRate);
        while (Microphone.GetPosition(deviceName) <= 0) { }

        samplesPerChunk = Mathf.CeilToInt(targetSampleRate * (chunkMs / 1000f));
        floatBuf = new float[samplesPerChunk];
        byteBuf = new byte[samplesPerChunk * 2];

        Debug.Log("[Vosk] OK. Gramática: " + grammarJson);
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

            bool isFinal = recognizer.AcceptWaveform(byteBuf, byteBuf.Length);
            if (isFinal)
            {
                results.Enqueue(recognizer.Result());        
            }
            else if (showPartialInConsole)
            {
                results.Enqueue(recognizer.PartialResult()); 
            }

            lastSamplePos = (lastSamplePos + samplesPerChunk) % mic.samples;
            delta -= samplesPerChunk;
        }

        while (results.TryDequeue(out var json))
        {
            string text = ParseTextFromJson(json);
            if (!string.IsNullOrWhiteSpace(text))
            {
                transcript.AppendLine(text);
                TryMatchAndDispatch(text);
            }
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 900, 400), transcript.ToString());
    }

    void OnDestroy()
    {
        if (mic != null) Microphone.End(deviceName);
        recognizer?.Dispose();
        model?.Dispose();
    }

    string BuildGrammar(string[] frases)
    {
        var sb = new StringBuilder();
        sb.Append('[');
        for (int i = 0; i < frases.Length; i++)
        {
            sb.Append('"').Append(frases[i].Replace("\"", "\\\"")).Append('"');
            if (i < frases.Length - 1) sb.Append(',');
        }
        sb.Append(']');
        return sb.ToString();
    }

    string ParseTextFromJson(string json)
    {
        string key = "\"text\"";
        int i = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (i < 0) { key = "\"partial\""; i = json.IndexOf(key, StringComparison.OrdinalIgnoreCase); }
        if (i < 0) return null;
        int start = json.IndexOf('"', i + key.Length);
        if (start < 0) return null;
        int end = json.IndexOf('"', start + 1);
        if (end < 0) return null;
        return json.Substring(start + 1, end - (start + 1));
    }

    void TryMatchAndDispatch(string heardRaw)
    {
        var (best, score) = Fuzzy.Best(heardRaw, VoiceCommands.COMANDOS);
        if (score >= fuzzyThreshold)
        {
            double now = Time.timeAsDouble;
            if (best == lastCmd && (now - lastCmdTime) < debounceSeconds) return;
            lastCmd = best; lastCmdTime = now;

            Debug.Log($"[Voz] '{heardRaw}' → '{best}' (score={score:0.00})");
            OnCommand(best, score);
        }
        else
        {
            Debug.Log($"[Voz] bajo score: '{heardRaw}' (score={score:0.00})");
        }
    }
    void TryEarlyFire(string heardPartial)
    {
        // 1) Fuzzy contra comandos
        var (best, score) = Fuzzy.Best(heardPartial, VoiceCommands.COMANDOS);

        // 2) Comprobar umbral para parciales
        if (score >= partialThreshold)
        {
            // 3) Estabilidad: mismo "best" en frames consecutivos
            if (best == lastPartialBest) partialStableCount++;
            else { lastPartialBest = best; partialStableCount = 1; }

            // 4) Cooldown para no spamear
            double now = Time.timeAsDouble;
            if (partialStableCount >= partialStableFrames && (now - lastFireTime) >= partialCooldownSec)
            {
                // Log de depuración
                Debug.Log($"[EARLY] '{heardPartial}' → '{best}' (score={score:0.00})");

                // 5) Dispara como si fuese final (en tu OnCommand)
                OnCommand(best, score);

                lastFireTime = now;
                partialStableCount = 0;
                lastPartialBest = null;
            }
        }
        else
        {
            // Si baja el score, resetea estabilidad
            partialStableCount = 0;
        }
    }
    void OnCommand(string cmd, double score)
    {
        
        string k = Fuzzy.Normalize(cmd);

        
        CommandType? tipo = k switch
        {
            "salta" => CommandType.Jump,
            "agachate" => null,   
            "agáchate" => null,
            "dispara" => null,
            "recarga" => null,
            "izquierda" => CommandType.Left,
            "derecha" => CommandType.Right,
            "correr" => CommandType.RunOn,   // correr/avanzar
            "abajo" => CommandType.RunOff,  // detener avance
            "pausa" => null,
            "inventario" => null,
            _ => null
        };

        if (tipo.HasValue)
        {
            InputOrchestrator.Instance.Enqueue(tipo.Value, CommandSource.Voice);
            Debug.Log($"[Voz→Orch] {tipo.Value} (score={score:0.00})");
        }
        else
        {
            Debug.Log($"[Voz] comando sin mapeo: '{k}' (score={score:0.00})");
        }
    }
}
