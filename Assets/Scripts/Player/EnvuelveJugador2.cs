using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Collections;
public class EnvuelveJugador2 : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject prefabCuadrado;    // Prefab de hielo
    public Transform jugador;            // El jugador
    public float duracionCuadrado = 5f;  // Tiempo congelado

    private bool hieloActivo = false;    // Para evitar duplicados

    void Update()
    {
        // Solo activar si GlobalData.hielo está ON y aún no se puso
        if (GlobalData.hielo && !hieloActivo)
        {
            Debug.Log("enteeeeeeeeeeeeeeeeeeeeeeeeeeeeee hielo");
            StartCoroutine(PonerHielo());
        }
    }

    private IEnumerator PonerHielo()
    {
        hieloActivo = true;

        // Instanciar el hielo pegado al jugador
        GameObject cuadrado = Instantiate(prefabCuadrado, jugador.position, Quaternion.identity);
        cuadrado.transform.SetParent(jugador);
        cuadrado.transform.localPosition = new Vector3(0f, -0.5f, 0f);

        Debug.Log("Jugador congelado por " + duracionCuadrado + " segundos.");

        // Esperar tiempo de congelación
        yield return new WaitForSeconds(duracionCuadrado);

        // Quitar hielo
        Destroy(cuadrado);

        // Apagar el estado GlobalData del hielo
        GlobalData.hielo = false;
        GlobalData.espera = true;
    hieloActivo = false;
    }
}
