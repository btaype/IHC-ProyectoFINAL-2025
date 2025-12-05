using UnityEngine;

public static class GlobalData
{
    public static bool pausa = false;
    public static bool connect = true;
    public static bool mancha = false;
    public static bool hielo = false;
    public static bool hielo2 = false;
    public static bool mancha2 = false;
    public static bool inicio = false;
    public static bool inicio2 = false;
    public static bool espera = true;
    public static bool final = false;
    public static int error_camara = 0;
    public static int forcedConstruction = -1;

    // ?? FUNCION QUE REINICIA TODO
    public static void Reset()
    {
        pausa = false;
        connect = true;
        mancha = false;
        hielo = false;
        hielo2 = false;
        mancha2 = false;
        inicio = false;
        inicio2 = false;
        espera = true;
        final = false;
        error_camara = 0;
        forcedConstruction = -1;
    }
}