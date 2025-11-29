using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PantallaEsperando : MonoBehaviour
{
    private GameObject panelEsperando;

    void Update()
    {
        // Si inicio es false y no se ha mostrado, lo mostramos
        if (!GlobalData.inicio)
        {
            if (panelEsperando == null)
                MostrarPantallaEsperando();
        }
        else
        {
            // Si inicio es true y existe, lo quitamos
            if (panelEsperando != null)
                QuitarPantallaEsperando();
        }
    }

    void MostrarPantallaEsperando()
    {
        if (panelEsperando != null) return;

        panelEsperando = new GameObject("EsperandoPanel");
        Canvas canvas = panelEsperando.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;  // por si quieres que esté debajo de la pausa

        panelEsperando.AddComponent<CanvasScaler>();
        panelEsperando.AddComponent<GraphicRaycaster>();

        // Fondo oscuro suave
        GameObject fondo = new GameObject("Fondo");
        fondo.transform.SetParent(panelEsperando.transform, false);
        Image imgFondo = fondo.AddComponent<Image>();
        imgFondo.color = new Color(0, 0, 0, 0.4f);

        RectTransform rtFondo = fondo.GetComponent<RectTransform>();
        rtFondo.anchorMin = Vector2.zero;
        rtFondo.anchorMax = Vector2.one;
        rtFondo.offsetMin = Vector2.zero;
        rtFondo.offsetMax = Vector2.zero;

        // Texto principal
        GameObject texto = new GameObject("TextoEsperando");
        texto.transform.SetParent(panelEsperando.transform, false);
        TextMeshProUGUI tmp = texto.AddComponent<TextMeshProUGUI>();
        tmp.text = "Esperando contrincante...";
        tmp.fontSize = 80;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rtTexto = texto.GetComponent<RectTransform>();
        rtTexto.anchorMin = new Vector2(0.5f, 0.5f);
        rtTexto.anchorMax = new Vector2(0.5f, 0.5f);
        rtTexto.sizeDelta = new Vector2(900, 200);
        rtTexto.anchoredPosition = Vector2.zero;
    }

    void QuitarPantallaEsperando()
    {
        if (panelEsperando != null)
        {
            Destroy(panelEsperando);
            panelEsperando = null;
        }
    }
}
