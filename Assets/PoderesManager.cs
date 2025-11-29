using UnityEngine;
using UnityEngine.UI; // Necesario para Image y Button
using System.Collections;

public class PoderesManager : MonoBehaviour
{
    [Header("--- 1. Asigna tus Botones Aquí ---")]
    public Button botonCeguera; // Arrastra aquí el botón de la Mancha
    public Button botonHielo;   // Arrastra aquí el botón de Ralentizar

    [Header("--- 2. Referencias Visuales ---")]
    public Image imagenCeguera;      // El Panel negro (Image)
    public GameObject visualAvisoPrefab; // El prefab de la señal/alerta
    public Transform contenedorAvisos;   // El objeto vacío hijo del Player

    [Header("--- 3. Configuración de Poderes ---")]
    
    public float duracionCeguera = 4f;
    
    
    public float duracionRalentizar = 5f;
    [Tooltip("Cantidad de velocidad a restar (Ej: 5)")]
    public float cantidadFrenado = 5f;

    [Header("--- Configuración General ---")]
    public float tiempoDeAviso = 1.5f;

    
    private ControladorGeneral scriptCorredor;
    private bool poderActivo = false;

    private void Start()
    {
       
        scriptCorredor = FindObjectOfType<ControladorGeneral>();
        if (scriptCorredor == null) Debug.LogError("ERROR: No encuentro el script 'ControladorGeneral' en la escena.");

        // B) ASEGURAR QUE LA MANCHA NO SE VEA AL INICIO
        if (imagenCeguera != null) imagenCeguera.gameObject.SetActive(false);

       
        if (botonCeguera != null)
        {
            botonCeguera.onClick.RemoveAllListeners(); 
            botonCeguera.onClick.AddListener(ActivarPoderCeguera);
        }
        else
        {
            Debug.LogWarning("Falta asignar el 'Boton Ceguera' en el Inspector.");
        }

        if (botonHielo != null)
        {
            botonHielo.onClick.RemoveAllListeners();
            botonHielo.onClick.AddListener(ActivarPoderHielo);
        }
        else
        {
            Debug.LogWarning("Falta asignar el 'Boton Hielo' en el Inspector.");
        }
    }

  
    void ActivarPoderCeguera()
    {
        if (!poderActivo && scriptCorredor != null)
        {
            StartCoroutine(RutinaCeguera());
        }
        else
        {
            Debug.Log("No se puede activar: Cooldown activo o Jugador no encontrado.");
        }
    }

    void ActivarPoderHielo()
    {
        if (!poderActivo && scriptCorredor != null)
        {
            StartCoroutine(RutinaRalentizar());
        }
        else
        {
            Debug.Log("No se puede activar: Cooldown activo o Jugador no encontrado.");
        }
    }

    private IEnumerator RutinaCeguera()
    {
        poderActivo = true;

        // 1. AVISO
        yield return StartCoroutine(MostrarAvisoVisual(0));

        // 2. EFECTO
        if (imagenCeguera != null) imagenCeguera.gameObject.SetActive(true);
        
        // 3. ESPERA
        yield return new WaitForSeconds(duracionCeguera);

        // 4. FIN
        if (imagenCeguera != null) imagenCeguera.gameObject.SetActive(false);
        
        poderActivo = false;
    }

    private IEnumerator RutinaRalentizar()
    {
        poderActivo = true;

        // 1. AVISO
        yield return StartCoroutine(MostrarAvisoVisual(0));

        // 2. EFECTO (Usando la lógica del ControladorGeneral)
        scriptCorredor.penalizacionVelocidad = cantidadFrenado;
        scriptCorredor.RecalcularVelocidadFinal();

        // 3. ESPERA
        yield return new WaitForSeconds(duracionRalentizar);

        // 4. FIN
        scriptCorredor.penalizacionVelocidad = 0f;
        scriptCorredor.RecalcularVelocidadFinal();

        poderActivo = false;
    }

    private IEnumerator MostrarAvisoVisual(float carrilX)
    {
        GameObject aviso = null;
        if (visualAvisoPrefab != null && contenedorAvisos != null)
        {
            aviso = Instantiate(visualAvisoPrefab, contenedorAvisos);
            aviso.transform.localPosition = new Vector3(carrilX, 2.5f, 8f);
        }

        yield return new WaitForSeconds(tiempoDeAviso);

        if (aviso != null) Destroy(aviso);
    }
}