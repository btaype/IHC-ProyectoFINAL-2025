using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PausaController : MonoBehaviour
{
    private GameObject panelPausa;

    void Update()
    {
        if (GlobalData.pausa && GlobalData.inicio)
        {
            MostrarPantallaPausa();
          
        }
        else
        {
            QuitarPantallaPausa();
            
        }
    }

    void MostrarPantallaPausa()
    {
        
        if (panelPausa != null) return;

        
        panelPausa = new GameObject("PausaPanel");
        Canvas canvas = panelPausa.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        panelPausa.AddComponent<CanvasScaler>();
        panelPausa.AddComponent<GraphicRaycaster>();

        
        GameObject fondo = new GameObject("Fondo");
        fondo.transform.SetParent(panelPausa.transform, false);
        Image imgFondo = fondo.AddComponent<Image>();
        imgFondo.color = new Color(0, 0, 0, 0.6f);   // negro 60%

        RectTransform rtFondo = fondo.GetComponent<RectTransform>();
        rtFondo.anchorMin = Vector2.zero;
        rtFondo.anchorMax = Vector2.one;
        rtFondo.offsetMin = Vector2.zero;
        rtFondo.offsetMax = Vector2.zero;

        // Texto PAUSA
        GameObject texto = new GameObject("TextoPausa");
        texto.transform.SetParent(panelPausa.transform, false);
        TextMeshProUGUI tmp = texto.AddComponent<TextMeshProUGUI>();
        tmp.text = "S T O P";
        tmp.fontSize = 150;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rtTexto = texto.GetComponent<RectTransform>();
        rtTexto.anchorMin = new Vector2(0.5f, 0.5f);
        rtTexto.anchorMax = new Vector2(0.5f, 0.5f);
        rtTexto.sizeDelta = new Vector2(800, 200);
        rtTexto.anchoredPosition = Vector2.zero;
    }

    void QuitarPantallaPausa()
    {
        if (panelPausa != null)
        {
            Destroy(panelPausa);
            panelPausa = null;
        }
    }
}