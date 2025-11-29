using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Configuración")]
    public float tiempoTotal = 60f;
    public float velocidadJugador1 = 1f;

    [Header("UI")]
    public Image timerBarFill;
    public TextMeshProUGUI timerText;

    [Header("Condiciones de Victoria")]
    public Transform metaJugador1;
    public Transform jugador1;

    // Estados
    private float tiempoRestante;
    private bool juegoTerminado = false;
    private GameObject panelMensaje;  // Panel reutilizable

    void Start()
    {
        tiempoRestante = tiempoTotal;
    }

    void Update()
    {
        // No avanza si el juego terminó, no inició, o está en pausa
        if (juegoTerminado || !GlobalData.inicio || (GlobalData.inicio == true && GlobalData.pausa == true))return;

        // Disminuir el tiempo
        tiempoRestante -= Time.deltaTime;

        // Actualizar UI
        ActualizarUI();

        // Si el tiempo llega a 0 → gana el maestro de obstáculos
        if (tiempoRestante <= 0 && GlobalData.final == false)
        {
            TerminarJuego("El maestro de los obstáculos domina la partida y gana");
            GlobalData.final = true;
        }

        // Verificar llegada del corredor
        if (metaJugador1 != null && jugador1 != null)
        {
            float distanciaMeta = Vector3.Distance(jugador1.position, metaJugador1.position);

            if (distanciaMeta < 5f && GlobalData.final == false)
            {
                TerminarJuego("Gano El Corredor De la Partida");
                GlobalData.final = true;
            }
        }
    }

    void ActualizarUI()
    {
        if (timerBarFill != null)
            timerBarFill.fillAmount = tiempoRestante / tiempoTotal;

        if (timerText != null)
            timerText.text = $"{tiempoRestante:F1}s";
    }

    void TerminarJuego(string mensaje)
    {
        juegoTerminado = true;
        MostrarPantallaMensaje(mensaje);
    }

    // ============================================================
    //                PANEL DE MENSAJE (SIN CORUTINA)
    // ============================================================
    void MostrarPantallaMensaje(string mensaje)
    {
        // Evitar que se cree más de un panel
        if (panelMensaje != null) return;

        panelMensaje = new GameObject("PanelMensaje");
        Canvas canvas = panelMensaje.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        panelMensaje.AddComponent<CanvasScaler>();
        panelMensaje.AddComponent<GraphicRaycaster>();

        // ------------------ Fondo Negro ------------------
        GameObject fondo = new GameObject("Fondo");
        fondo.transform.SetParent(panelMensaje.transform, false);
        Image imgFondo = fondo.AddComponent<Image>();
        imgFondo.color = new Color(0, 0, 0, 0.75f);

        RectTransform rtFondo = fondo.GetComponent<RectTransform>();
        rtFondo.anchorMin = Vector2.zero;
        rtFondo.anchorMax = Vector2.one;
        rtFondo.offsetMin = Vector2.zero;
        rtFondo.offsetMax = Vector2.zero;

        // ------------------ Texto Dinámico ------------------
        GameObject texto = new GameObject("TextoMensaje");
        texto.transform.SetParent(panelMensaje.transform, false);

        TextMeshProUGUI tmp = texto.AddComponent<TextMeshProUGUI>();
        tmp.text = mensaje;
        tmp.fontSize = 90;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rtTexto = texto.GetComponent<RectTransform>();
        rtTexto.anchorMin = new Vector2(0.5f, 0.5f);
        rtTexto.anchorMax = new Vector2(0.5f, 0.5f);
        rtTexto.sizeDelta = new Vector2(1100, 300);
        rtTexto.anchoredPosition = Vector2.zero;
    }
}
