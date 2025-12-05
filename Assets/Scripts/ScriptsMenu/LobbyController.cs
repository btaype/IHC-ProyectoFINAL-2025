using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour
{
    [Header("Botones del Lobby")]
    public Button buttonCorredor;
    public Button buttonSaboteador;

    void Start()
    {
        // Asegurarnos de que los botones no sean null
        if (buttonCorredor != null)
            buttonCorredor.onClick.AddListener(() => CargarJuegoComo("Corredor"));

        if (buttonSaboteador != null)
            buttonSaboteador.onClick.AddListener(() => CargarJuegoComo("Saboteador"));
    }

    void CargarJuegoComo(string rol)
    {
        // Guardar el rol
        PlayerPrefs.SetString("RolJugador", rol);

        // 🆕 NUEVO: guardar número según el rol
        if (rol == "Corredor")
            PlayerPrefs.SetInt("NumeroJugador", 1);
        else if (rol == "Saboteador")
            PlayerPrefs.SetInt("NumeroJugador", 2);

        PlayerPrefs.Save(); // ¡Importante!

        // Cargar escena
        SceneManager.LoadScene("SampleScene");
    }
}