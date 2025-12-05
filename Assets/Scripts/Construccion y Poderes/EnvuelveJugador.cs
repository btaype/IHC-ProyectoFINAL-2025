using UnityEngine;
using System.Collections;

public class EnvuelveJugador : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject prefabCuadrado;     // Arrastra el prefab del cuadrado
    public Transform jugador;             // Arrastra el jugador
    public float duracionCuadrado = 5f;   // Cuánto dura visible

    void Start()
    {
        GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ActivarEnvuelve);
    }

    public void ActivarEnvuelve()
    {
        if (prefabCuadrado == null || jugador == null) return;

        // Instancia el cuadrado como HIJO del jugador (lo sigue automáticamente)
        GameObject cuadrado = Instantiate(prefabCuadrado, jugador.position, Quaternion.identity);
        cuadrado.transform.SetParent(jugador);  // Se pega al jugador
        cuadrado.transform.localPosition = Vector3.zero;  // Exactamente encima

        // Desaparece después del tiempo
        Destroy(cuadrado, duracionCuadrado);

        Debug.Log("¡Jugador envuelto en cuadrado por " + duracionCuadrado + "s!");
    }
}