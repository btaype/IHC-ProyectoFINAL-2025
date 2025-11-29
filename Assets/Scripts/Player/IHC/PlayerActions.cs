using UnityEngine;

public static class PlayerActions
{
    private static PlayerController Ctl => PlayerController.Instance;

    public static void Jump() { if (!Ctl) { Debug.LogWarning("No Ctl -> Jump"); return; } Ctl.Jump(); Debug.Log("→ JUMP()"); }
    public static void Crouch() { Debug.Log("→ CROUCH()"); /* implementar si quieres en el controller */ }
    public static void Shoot() { Debug.Log("→ SHOOT()"); }
    public static void Reload() { Debug.Log("→ RELOAD()"); }

    public static void Left() { if (!Ctl) { Debug.LogWarning("No Ctl -> Left"); return; } Ctl.MoveLeft(); Debug.Log("→ MOVE LEFT()"); }
    public static void Right() { if (!Ctl) { Debug.LogWarning("No Ctl -> Right"); return; } Ctl.MoveRight(); Debug.Log("→ MOVE RIGHT()"); }

    // Up = Run ON, Down = Run OFF (como acordamos)
    public static void Up() { if (!Ctl) { Debug.LogWarning("No Ctl -> Up"); return; } Ctl.SetRun(true); Debug.Log("→ MOVE UP() (RUN ON)"); }
    public static void Down() { if (!Ctl) { Debug.LogWarning("No Ctl -> Down"); return; } Ctl.SetRun(false); Debug.Log("→ MOVE DOWN() (RUN OFF)"); }
    public static void Stop() { if (!Ctl) { Debug.LogWarning("No Ctl -> Stop"); return; } Ctl.Stop(); Debug.Log("→ STOP()"); }

    public static void Pause() { Debug.Log("→ PAUSE()"); }
    public static void Inventory() { Debug.Log("→ INVENTORY()"); }
    public static void Center()
    {
        if (!Ctl) { Debug.LogWarning("No Ctl -> Center"); return; }
        Ctl.MoveCenter();
        Debug.Log("→ MOVE CENTER()");
    }

    // Si prefieres por índice explícito:
    public static void Lane(int idx)
    {
        if (!Ctl) { Debug.LogWarning("No Ctl -> Lane(" + idx + ")"); return; }
        Ctl.MoveToLane(idx);
        Debug.Log($"→ MOVE TO LANE {idx}");
    }

}

