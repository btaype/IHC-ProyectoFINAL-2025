using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class ErrorCameraUI_Autonomo : MonoBehaviour
{
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI mensajeArriba;
    private TextMeshProUGUI mensajeAbajo;
    private Image manchaRoja;

    [Header("Settings")]
    public float duracion = 2f;
    public float fadeTime = 0.3f;
    public int numeroJugador = 0;
    private bool mostrando = false;
    private bool uiCreada = false;

    void Start()
    {
        numeroJugador = PlayerPrefs.GetInt("NumeroJugador", 0);
    }

    void Update()
    {
        if (mostrando || numeroJugador != 1) return;

        switch (GlobalData.error_camara)
        {
            case -4:
            case -5:
            case -6:
                if (!uiCreada)
                {
                    CrearUI();
                }
                StartCoroutine(MostrarMensaje(GlobalData.error_camara));
                break;
        }
    }

    void CrearUI()
    {
        // Crear Canvas
        GameObject canvasGO = new GameObject("ErrorCameraCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Crear Panel (mancha roja)
        GameObject panelGO = new GameObject("ManchaRoja");
        panelGO.transform.SetParent(canvas.transform);
        manchaRoja = panelGO.AddComponent<Image>();
        manchaRoja.color = new Color(1f, 0f, 0f, 0.2f); // rojo transparente
        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.05f, 0.1f); // más abajo y más ancho a la izquierda
        panelRT.anchorMax = new Vector2(0.95f, 0.8f); // parte superior igual, más ancho a la derecha
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // Añadir CanvasGroup al panel
        canvasGroup = panelGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // Crear mensaje arriba
        GameObject textoArribaGO = new GameObject("MensajeArriba");
        textoArribaGO.transform.SetParent(panelGO.transform);
        mensajeArriba = textoArribaGO.AddComponent<TextMeshProUGUI>();
        mensajeArriba.alignment = TextAlignmentOptions.Center;
        mensajeArriba.fontSize = 80;
        mensajeArriba.color = Color.white;
        RectTransform rtArriba = textoArribaGO.GetComponent<RectTransform>();
        rtArriba.anchorMin = new Vector2(0.1f, 0.8f); // subir
        rtArriba.anchorMax = new Vector2(0.9f, 0.95f);
        rtArriba.offsetMin = Vector2.zero;
        rtArriba.offsetMax = Vector2.zero;

        // Crear mensaje abajo
        GameObject textoAbajoGO = new GameObject("MensajeAbajo");
        textoAbajoGO.transform.SetParent(panelGO.transform);
        mensajeAbajo = textoAbajoGO.AddComponent<TextMeshProUGUI>();
        mensajeAbajo.alignment = TextAlignmentOptions.Center;
        mensajeAbajo.fontSize = 70;
        mensajeAbajo.color = Color.white;
        RectTransform rtAbajo = textoAbajoGO.GetComponent<RectTransform>();
        rtAbajo.anchorMin = new Vector2(0.1f, 0.05f);
        rtAbajo.anchorMax = new Vector2(0.9f, 0.2f);
        rtAbajo.offsetMin = Vector2.zero;
        rtAbajo.offsetMax = Vector2.zero;

        uiCreada = true;
    }

    IEnumerator MostrarMensaje(int error)
    {
        mostrando = true;

        // Mensaje arriba según error
        switch (error)
        {
            case -4:
                mensajeArriba.text = "Estás muy cerca";
                break;
            case -6:
                mensajeArriba.text = "Estás muy a la izquierda";
                break;
            case -5:
                mensajeArriba.text = "Estás muy a la derecha";
                break;
        }

        // Mensaje abajo siempre igual
        mensajeAbajo.text = "Regresar a la zona segura marcada por la cinta";

        // Fade-in
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Esperar duración
        yield return new WaitForSeconds(duracion);

        // Fade-out
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        GlobalData.error_camara = 0;
        mostrando = false;
    }
}