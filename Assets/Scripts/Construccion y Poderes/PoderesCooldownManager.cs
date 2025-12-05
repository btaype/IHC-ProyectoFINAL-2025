using UnityEngine;
using UnityEngine.UI;
using TMPro;          // ← necesario si usas TextMeshPro
using System.Collections;

public class PoderesCooldownManager : MonoBehaviour
{
    [Header("=== PODERES ===")]
    public Button[] botonesPoderes;

    [Header("=== COOLDOWN VISUAL (PRO) ===")]
    public Image[] cooldownOverlays;
    public Color colorCarga = new Color(0f, 1f, 1f, 0.5f);
    public float tiempoCooldown = 5f;

    [Header("Cooldown individual (opcional)")]
    public float[] tiemposCooldownPorPoder;

    [Header("=== USOS ===")]
    public int usosIniciales = 5;                    // cantidad por cada poder
    public int[] usosRestantes;                     // contador interno
    public TMP_Text[] textosUsos;                   // "x5", "x4", ...

    private bool[] enCooldown;

    void Start()
    {
        if (botonesPoderes == null || botonesPoderes.Length == 0)
        {
            Debug.LogError("¡Faltan botones!");
            return;
        }

        enCooldown = new bool[botonesPoderes.Length];
        usosRestantes = new int[botonesPoderes.Length];

        // Inicializar usos
        for (int i = 0; i < usosRestantes.Length; i++)
        {
            usosRestantes[i] = usosIniciales;
            if (textosUsos.Length > i && textosUsos[i] != null)
                textosUsos[i].text = "x" + usosRestantes[i];
        }

        // Configurar overlays
        for (int i = 0; i < cooldownOverlays.Length; i++)
        {
            if (cooldownOverlays[i] == null) continue;

            Image img = cooldownOverlays[i];
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillOrigin = (int)Image.OriginHorizontal.Left;
            img.fillAmount = 0f;
            img.color = colorCarga;
            img.raycastTarget = false;
            img.gameObject.SetActive(false);
        }

        // Conectar botones
        for (int i = 0; i < botonesPoderes.Length; i++)
        {
            int idx = i;
            botonesPoderes[i].onClick.RemoveAllListeners();
            botonesPoderes[i].onClick.AddListener(() => ActivarPoder(idx));

            ActualizarEstadoVisual(idx);
        }
    }


    // ======================
    //     ACTIVAR PODER
    // ======================
    public void ActivarPoder(int indexPoder)
    {
        if (indexPoder < 0 || indexPoder >= botonesPoderes.Length) return;

        if (enCooldown[indexPoder]) return;

        // ✔ No permitir uso si está en 0
        if (usosRestantes[indexPoder] <= 0) return;

        // Lógica de poderes personales:
        if (indexPoder == 0) GlobalData.mancha = true;
        if (indexPoder == 1) GlobalData.hielo = true;

        Debug.Log($"PODER {indexPoder} ACTIVADO!");

        // Reducir usos
        usosRestantes[indexPoder]--;

        if (textosUsos[indexPoder] != null)
            textosUsos[indexPoder].text = "x" + usosRestantes[indexPoder];

        // Si se quedó sin usos → bloquear botón
        if (usosRestantes[indexPoder] <= 0)
        {
            botonesPoderes[indexPoder].interactable = false;
            ActualizarEstadoVisual(indexPoder);
            return; // ya no inicia cooldown
        }

        // Iniciar cooldown normal
        IniciarCooldown(indexPoder);
    }


    // ======================
    //     INICIAR COOLDOWN
    // ======================
    public void IniciarCooldown(int index)
    {
        Button btn = botonesPoderes[index];
        Image overlay = (index < cooldownOverlays.Length) ? cooldownOverlays[index] : null;

        btn.interactable = false;
        enCooldown[index] = true;

        float tiempo = tiempoCooldown;
        if (tiemposCooldownPorPoder != null && index < tiemposCooldownPorPoder.Length)
            tiempo = tiemposCooldownPorPoder[index] > 0 ? tiemposCooldownPorPoder[index] : tiempoCooldown;

        StartCoroutine(CooldownPro(btn, overlay, tiempo));
    }

    private IEnumerator CooldownPro(Button btn, Image overlay, float duracion)
    {
        if (overlay != null)
        {
            overlay.gameObject.SetActive(true);
            overlay.fillAmount = 0f;
        }

        float elapsed = 0f;

        while (elapsed < duracion)
        {
            elapsed += Time.unscaledDeltaTime;
            float ratio = elapsed / duracion;

            if (overlay != null)
                overlay.fillAmount = ratio;

            yield return null;
        }

        if (overlay != null)
        {
            overlay.fillAmount = 1f;
            overlay.gameObject.SetActive(false);
        }

        int index = System.Array.IndexOf(botonesPoderes, btn);
        enCooldown[index] = false;

        // Solo reactiva si quedan usos
        if (usosRestantes[index] > 0)
            btn.interactable = true;

        ActualizarEstadoVisual(index);
    }

    // ======================
    //     VISUAL SIN USOS
    // ======================
    private void ActualizarEstadoVisual(int i)
    {
        if (usosRestantes[i] <= 0)
        {
            // opaco
            Color c = botonesPoderes[i].image.color;
            c.a = 0.35f;
            botonesPoderes[i].image.color = c;
        }
        else
        {
            // normal
            Color c = botonesPoderes[i].image.color;
            c.a = 1f;
            botonesPoderes[i].image.color = c;
        }
    }
}
