using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    void Update()
    {
        if (GlobalData.final)
        {
            GlobalData.final = false;
            StartCoroutine(IrAlMenuFinal());
        }
    }

    private IEnumerator IrAlMenuFinal()
    {
        yield return new WaitForSeconds(9f);

        // Destruir todos los objetos de la escena actual
        DestruirTodoEnEscena();

        // Obtener el número de jugador
        int numeroJugador = PlayerPrefs.GetInt("NumeroJugador", 0);

        // Cargar escena según el jugador
        if (numeroJugador == 1)
        {
            SceneManager.LoadScene("Menu_corredor");
        }
        else if (numeroJugador == 2)
        {
            SceneManager.LoadScene("Menu_saboteador");
        }
        else
        {
            Debug.LogWarning("Número de jugador inválido: " + numeroJugador);
        }
    }

    private void DestruirTodoEnEscena()
    {
        // Obtener todos los GameObjects de la escena
        GameObject[] todosLosObjetos = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in todosLosObjetos)
        {
            // No destruir objetos marcados como DontDestroyOnLoad
            if (obj.scene.name != null)
            {
                Destroy(obj);
            }
        }
    }
}