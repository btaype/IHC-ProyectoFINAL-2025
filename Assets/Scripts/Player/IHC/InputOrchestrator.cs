using System.Collections.Generic;
using UnityEngine;

public enum CommandType { RunOn, RunOff, Stop, Jump, Left, Right, Center }
public enum CommandSource { Voice, Keyboard, CameraAI, System }

public struct Command
{
    public CommandType Type;
    public CommandSource Source;
    public float Time;

    public Command(CommandType t, CommandSource s)
    {
        Type = t;
        Source = s;
        Time = UnityEngine.Time.time;
    }
}

[DefaultExecutionOrder(-100)]
public class InputOrchestrator : MonoBehaviour
{
    public static InputOrchestrator Instance { get; private set; }

    // Prioridad por fuente (puedes ajustar)
    private readonly Dictionary<CommandSource, int> priority = new()
    {
        { CommandSource.System,   100 },
        { CommandSource.Voice,     80 },
        { CommandSource.Keyboard,  70 },
        { CommandSource.CameraAI,  10 },
    };

    // Cooldowns por tipo (agregado Center para evitar spam)
    private readonly Dictionary<CommandType, float> cooldown = new()
    {
        { CommandType.Jump,   0.20f },
        { CommandType.Left,   0.08f },
        { CommandType.Right,  0.08f },
        { CommandType.Center, 0.25f }, // ← nuevo
        { CommandType.RunOn,  0.05f },
        { CommandType.RunOff, 0.05f },
        { CommandType.Stop,   0.05f },
    };

    private readonly Dictionary<CommandType, float> lastExec = new();
    private readonly List<Command> inbox = new();

    // ---------- Dwell lateral por fuente para evitar rebotes ----------
    [SerializeField] private double laneDwellSec = 0.45; // tiempo de “pegajosidad”
    private readonly Dictionary<CommandSource, (CommandType type, double t)> lastLateralBySource
        = new();

    private bool BlocksOppositeByDwell(Command c)
    {
        bool isLane = c.Type == CommandType.Left || c.Type == CommandType.Right || c.Type == CommandType.Center;
        if (!isLane) return false;

        if (!lastLateralBySource.TryGetValue(c.Source, out var prev)) return false;
        double dt = Time.timeAsDouble - prev.t;
        if (dt > laneDwellSec) return false; // ya venció ventana de protección

        bool opposite =
            (prev.type == CommandType.Left && c.Type == CommandType.Right) ||
            (prev.type == CommandType.Right && c.Type == CommandType.Left);

        bool undoByCenter =
            (c.Type == CommandType.Center) && (prev.type == CommandType.Left || prev.type == CommandType.Right);

        return opposite || undoByCenter;
    }
    // -----------------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Enqueue(CommandType t, CommandSource s) => inbox.Add(new Command(t, s));

    void Update()
    {
        if (inbox.Count == 0) return;

        // Elegir por tipo el de mayor prioridad (y si empatan, el más reciente)
        var pick = new Dictionary<CommandType, Command>();
        foreach (var c in inbox)
        {
            if (!pick.TryGetValue(c.Type, out var cur) ||
                priority[c.Source] > priority[cur.Source] ||
               (priority[c.Source] == priority[cur.Source] && c.Time >= cur.Time))
            {
                pick[c.Type] = c;
            }
        }

        // Conflictos (mutuamente excluyentes): RunOn vs RunOff vs Stop
        Exec(ChooseHighest(new[] { CommandType.RunOn, CommandType.RunOff, CommandType.Stop }, pick));

        // Carril: un único ganador entre Left/Right/Center (evita pisarse)
        Exec(ChooseHighest(new[] { CommandType.Left, CommandType.Right, CommandType.Center }, pick));

        // Salto (compatible)
        Exec(ChooseHighest(new[] { CommandType.Jump }, pick));

        inbox.Clear();
    }

    private Command? ChooseHighest(CommandType[] set, Dictionary<CommandType, Command> pool)
    {
        Command? best = null; int bestP = int.MinValue;
        foreach (var t in set)
        {
            if (!pool.TryGetValue(t, out var c)) continue;
            int p = priority[c.Source];
            if (p > bestP || (p == bestP && best.HasValue && c.Time > best.Value.Time))
            { best = c; bestP = p; }
        }
        return best;
    }

    private void Exec(Command? maybe)
    {
        if (!maybe.HasValue) return;
        var c = maybe.Value;

        // Cooldown por tipo
        if (cooldown.TryGetValue(c.Type, out var cd))
        {
            float last = lastExec.TryGetValue(c.Type, out var t) ? t : -999f;
            if (Time.time - last < cd) return;
        }

        // Dwell para evitar rebote (p. ej. Camera manda Center tras un Left)
        if (BlocksOppositeByDwell(c)) return;

        switch (c.Type)
        {
            case CommandType.RunOn: PlayerActions.Up(); break;
            case CommandType.RunOff: PlayerActions.Down(); break;
            case CommandType.Stop: PlayerActions.Stop(); break;
            case CommandType.Left: PlayerActions.Left(); break;
            case CommandType.Right: PlayerActions.Right(); break;
            case CommandType.Jump: PlayerActions.Jump(); break;
            case CommandType.Center: PlayerActions.Center(); break;
        }

        lastExec[c.Type] = Time.time;

        // Memoriza último lateral por fuente (para dwell)
        if (c.Type == CommandType.Left || c.Type == CommandType.Right || c.Type == CommandType.Center)
            lastLateralBySource[c.Source] = (c.Type, Time.timeAsDouble);

        Debug.Log($"[ORCH] {c.Type} <- {c.Source}");
    }
}
