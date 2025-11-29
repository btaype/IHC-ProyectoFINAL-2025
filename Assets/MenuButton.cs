using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    [Header("Configuración")]
    public string menuJugador1 = "Menu_corredor";
    public string menuJugador2 = "Menu_saboteador";

    public Button miBoton;

    int numeroJugador = 0;

    void Start()
    {
        // Obtener el número de jugador guardado
        numeroJugador = PlayerPrefs.GetInt("NumeroJugador", 0);
        Debug.Log("Jugador detectado: " + numeroJugador);

        // Si no arrastraste el botón, usa el GameObject
        if (miBoton == null)
            miBoton = GetComponent<Button>();

        if (miBoton != null)
        {
            miBoton.onClick.AddListener(VolverAlMenu);
        }
        else
        {
            Debug.LogError("¡Arrastra el botón o pon el script en el botón!");
        }
    }

    public void VolverAlMenu()
    {
        Debug.Log("🔙 Volviendo al menú del jugador " + numeroJugador);

        // Elegir escena según jugador
        if (numeroJugador == 1)
        {
            SceneManager.LoadScene(menuJugador1);
        }
        else if (numeroJugador == 2)
        {
            SceneManager.LoadScene(menuJugador2);
        }
        else
        {
            Debug.LogError("NúmeroJugador inválido (esperado 1 o 2). Valor: " + numeroJugador);
        }
    }
}
