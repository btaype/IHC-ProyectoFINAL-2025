using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ComboPowerManager : MonoBehaviour
{
    [Header("Referencias UI (arrastra desde Inspector)")]
    public Button[] constructionButtons; // [0]: HotDogStand Limpio, [1]: Barrier
    public Image[] cooldownImages;        // [0]: Cooldown botón 0, [1]: botón 1

    [Header("Efectos Visuales")]
    public Color blinkColor = Color.yellow;  
    public float blinkSpeed = 4f;            

    private Coroutine blinkCoroutine;
    private int currentForced = -1;

    void Start()
    {
        // 🔧 FIJAR EL BUG: Forzar -1 al inicio para NO parpadear desde el arranque
        GlobalData.forcedConstruction = -1;
        currentForced = -1;
        
        EnableAllButtons();
        Debug.Log("🎮 ComboPowerManager INICIADO → Todos botones LIBRES (sin parpadeo inicial)");
    }

    void Update()
    {
        // Solo detectar CAMBIOS en forcedConstruction (no al inicio)
        if (GlobalData.forcedConstruction != currentForced)
        {
            currentForced = GlobalData.forcedConstruction;
            
            if (currentForced == -1)
            {
                EnableAllButtons();
                Debug.Log("✅ COMBO COMPLETADO → Todos botones LIBRES otra vez");
            }
            else
            {
                ApplyForcedConstruction(currentForced);
                Debug.Log($"🔥 PODER USADO → ¡OBLIGADO a usar BOTÓN {currentForced}!");
            }
        }
    }

    void EnableAllButtons()
    {
        StopBlink();
        for (int i = 0; i < constructionButtons.Length; i++)
        {
            Button btn = constructionButtons[i];
            btn.interactable = true;
            
            Image btnImg = btn.GetComponent<Image>();
            Color c = btnImg.color;
            c.a = 1f; 
            btnImg.color = c;

            RectTransform btnRT = btn.GetComponent<RectTransform>();
            btnRT.localScale = Vector3.one;

            if (i < cooldownImages.Length)
            {
                cooldownImages[i].transform.localScale = Vector3.zero; 
            }
        }
    }

    void ApplyForcedConstruction(int index)
    {
        if (index < 0 || index >= constructionButtons.Length) return;

        StopBlink();

        for (int i = 0; i < constructionButtons.Length; i++)
        {
            Button btn = constructionButtons[i];
            Image btnImg = btn.GetComponent<Image>();
            RectTransform btnRT = btn.GetComponent<RectTransform>();

            if (i != index)
            {
                // ❌ OTROS: Grises, desactivados, pequeñitos
                btn.interactable = false;
                Color c = btnImg.color;
                c.a = 0.3f; 
                btnImg.color = c;
                btnRT.localScale = Vector3.one * 0.8f;
            }
            else
            {
                // ✅ ESTE: Parpadea + pulsa ÉPICO
                btn.interactable = true;
                Color c = btnImg.color;
                c.a = 1f;
                btnImg.color = c;
                btnRT.localScale = Vector3.one;

                blinkCoroutine = StartCoroutine(BlinkButton(btn));
            }
        }
    }

    void StopBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        foreach (var btn in constructionButtons)
        {
            Image img = btn.GetComponent<Image>();
            RectTransform rt = btn.GetComponent<RectTransform>();
            Color c = img.color;
            c.a = 1f;
            img.color = c;
            rt.localScale = Vector3.one;
        }
    }

    IEnumerator BlinkButton(Button button)
    {
        Image img = button.GetComponent<Image>();
        RectTransform rt = button.GetComponent<RectTransform>();
        Color originalColor = img.color;

        while (GlobalData.forcedConstruction == currentForced)  // 🆕 Para cuando cambie
        {
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);

            Color lerpColor = Color.Lerp(originalColor, blinkColor, Mathf.Sin(t * Mathf.PI));
            img.color = lerpColor;

            Vector3 pulseScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one * 0.85f, Mathf.Sin(t * Mathf.PI * 2f));
            rt.localScale = pulseScale;

            yield return null;
        }
    }
}