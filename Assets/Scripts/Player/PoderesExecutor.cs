using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PoderesExecutor : MonoBehaviour
{
    public Image imagenCeguera;
    public GameObject visualAvisoPrefab;
    public Transform contenedorAvisos;

    public float tiempoAviso = 1.5f;
    public float duracionCeguera = 4f;

    public float duracionRalentizar = 5f;
    public float cantidadFrenado = 5f;

    private ControladorGeneral controlador;
    private bool poderEnUso = false;

    private void Start()
    {
        controlador = FindObjectOfType<ControladorGeneral>();

        if (imagenCeguera != null)
            imagenCeguera.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (poderEnUso) return;

        // MANCHA / CEGUERA
        if (GlobalData.mancha)
        {
            GlobalData.mancha = false;
            StartCoroutine(IE_Ceguera());
        }

       
    }

    IEnumerator IE_Ceguera()
    {
        poderEnUso = true;

        yield return StartCoroutine(MostrarAviso());

        imagenCeguera.gameObject.SetActive(true);

        yield return new WaitForSeconds(duracionCeguera);

        imagenCeguera.gameObject.SetActive(false);

        poderEnUso = false;
    }

    IEnumerator IE_Hielo()
    {
        poderEnUso = true;

        yield return StartCoroutine(MostrarAviso());

        controlador.penalizacionVelocidad = cantidadFrenado;
        controlador.RecalcularVelocidadFinal();

        yield return new WaitForSeconds(duracionRalentizar);

        controlador.penalizacionVelocidad = 0;
        controlador.RecalcularVelocidadFinal();

        poderEnUso = false;
    }

    IEnumerator MostrarAviso()
    {
        GameObject aviso = Instantiate(visualAvisoPrefab, contenedorAvisos);
        aviso.transform.localPosition = new Vector3(0, 2.5f, 8f);

        yield return new WaitForSeconds(tiempoAviso);

        Destroy(aviso);
    }
}
