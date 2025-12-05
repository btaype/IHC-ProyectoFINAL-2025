using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CountdownUI : MonoBehaviour
{
    [Header("=== Configuración del Contador ===")]
    public float tiempoEntreNumeros = 1f;
    public int tamanioFuente = 200;
    public Color colorTexto = Color.white;

    [Header("=== Audio ===")]
    public AudioClip sonidoInicio;
    public float volumenAudio = 1f;

    private GameObject panelPausa;
    private TextMeshProUGUI tmpContador;
    private bool yaSeMostro = false;

    void Update()
    {
        // ===============================================
        //   MODO DE PRUEBA - ACTIVAR Countdown con 'P'
        //   Comenta esta sección para volver al modo normal
        // ===============================================
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Iniciando contador manual con tecla P (modo prueba)");
            yaSeMostro = false;          // para que permita repetir
            GlobalData.inicio = true;    // simula el inicio real
        }
        // ===============================================


        // Modo normal: inicia cuando GlobalData.inicio == true
        if (GlobalData.inicio && !yaSeMostro)
        {
            Debug.Log("ENNNNNNTRRRRRRAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            yaSeMostro = true;
            MostrarCuentaRegresiva();
            
        }
    }

    private void MostrarCuentaRegresiva()
    {
        if (panelPausa != null) return;

        // Crear panel
        panelPausa = new GameObject("PausaPanel");
        Canvas canvas = panelPausa.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        panelPausa.AddComponent<CanvasScaler>();
        panelPausa.AddComponent<GraphicRaycaster>();

        // Fondo
        GameObject fondo = new GameObject("Fondo");
        fondo.transform.SetParent(panelPausa.transform, false);
        Image imgFondo = fondo.AddComponent<Image>();
        imgFondo.color = new Color(0, 0, 0, 0.6f);

        RectTransform rtFondo = fondo.GetComponent<RectTransform>();
        rtFondo.anchorMin = Vector2.zero;
        rtFondo.anchorMax = Vector2.one;
        rtFondo.offsetMin = Vector2.zero;
        rtFondo.offsetMax = Vector2.zero;

        // Texto
        GameObject texto = new GameObject("TextoContador");
        texto.transform.SetParent(panelPausa.transform, false);
        tmpContador = texto.AddComponent<TextMeshProUGUI>();
        tmpContador.fontSize = tamanioFuente;
        tmpContador.color = colorTexto;
        tmpContador.alignment = TextAlignmentOptions.Center;

        RectTransform rtTexto = texto.GetComponent<RectTransform>();
        rtTexto.anchorMin = new Vector2(0.5f, 0.5f);
        rtTexto.anchorMax = new Vector2(0.5f, 0.5f);
        rtTexto.sizeDelta = new Vector2(800, 200);
        rtTexto.anchoredPosition = Vector2.zero;

        // Audio
        if (sonidoInicio != null)
        {
            AudioSource audio = panelPausa.AddComponent<AudioSource>();
            audio.clip = sonidoInicio;
            audio.volume = volumenAudio;
            audio.spatialBlend = 0f; // <<---- AUDIO 2D, suena igual para ambas cámaras
            audio.Play();
        }

        StartCoroutine(ContadorCoroutine());
       
    }

    private IEnumerator ContadorCoroutine()
    {
        for (int i = 3; i >= 1; i--)
        {
            tmpContador.text = i.ToString();
            yield return new WaitForSeconds(tiempoEntreNumeros);
        }

        tmpContador.text = "¡GO!";
        yield return new WaitForSeconds(tiempoEntreNumeros);

        Destroy(panelPausa);
        panelPausa = null;
        GlobalData.inicio2 = true;
    }
}
