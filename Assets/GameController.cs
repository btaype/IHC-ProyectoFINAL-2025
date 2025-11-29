using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Cámaras")]
    public GameObject camaraCorredor;
    public GameObject camaraSaboteador;

    [Header("UI")]
    public GameObject uiSaboteador;

    [Header("Jugador")]
    public PlayerMovement controladorCorredor; // Script del corredor

    void Start()
    {
        string rol = PlayerPrefs.GetString("RolJugador", "Corredor");

        // El corredor SIEMPRE existe
        // Solo cambiamos qué controlamos y vemos

        if (rol == "Corredor")
        {
            ActivarComoCorredor();
        }
        else
        {
            ActivarComoSaboteador();
        }
    }

    void ActivarComoCorredor()
    {
        camaraCorredor.SetActive(true);
        camaraSaboteador.SetActive(false);
        uiSaboteador.SetActive(false);

        // El corredor se controla con teclado
        if (controladorCorredor != null)
            controladorCorredor.enabled = true;

        Debug.Log("Eres el CORREDOR - Usa WASD");
    }

    void ActivarComoSaboteador()
    {
        camaraCorredor.SetActive(false);
        camaraSaboteador.SetActive(true);
        uiSaboteador.SetActive(true);

        // El corredor se mueve solo o espera (por ahora desactivamos control)
        if (controladorCorredor != null)
            controladorCorredor.enabled = false;

        Debug.Log("Eres el SABOTEADOR - Usa botones para trampas");
    }
}