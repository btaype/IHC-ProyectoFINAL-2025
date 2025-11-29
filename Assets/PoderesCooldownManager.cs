using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PoderesCooldownManager : MonoBehaviour
{
    [Header("=== PODERES ===")]
    public Button[] botonesPoderes;

    [Header("=== COOLDOWN VISUAL (PRO) ===")]
    public Image[] cooldownOverlays;                    // ← UNA IMAGE POR BOTÓN
    public Color colorCarga = new Color(0f, 1f, 1f, 0.5f); // Cian bonito
    public float tiempoCooldown = 5f;

    [Header("Cooldown individual (opcional)")]
    public float[] tiemposCooldownPorPoder;

    private bool[] enCooldown;

    void Start()
    {
        if (botonesPoderes == null || botonesPoderes.Length == 0)
        {
            Debug.LogError("¡Faltan botones en PoderesCooldownManager!");
            return;
        }

        enCooldown = new bool[botonesPoderes.Length];

        // CONFIGURAR CADA OVERLAY CORRECTAMENTE
        for (int i = 0; i < cooldownOverlays.Length; i++)
        {
            if (cooldownOverlays[i] == null) continue;

            Image img = cooldownOverlays[i];
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillOrigin = (int)Image.OriginHorizontal.Left;
            img.fillAmount = 0f;
            img.color = colorCarga;
            img.raycastTarget = false;        // Importante: no bloquee clics
            img.gameObject.SetActive(false);  // Oculto al inicio
        }

        // Conectar botones
        for (int i = 0; i < botonesPoderes.Length; i++)
        {
            int index = i;
            botonesPoderes[i].onClick.RemoveAllListeners();
            botonesPoderes[i].onClick.AddListener(() => ActivarPoder(index));
        }
    }

    public void ActivarPoder(int indexPoder)

    {   
       
        if (indexPoder < 0 || indexPoder >= botonesPoderes.Length) return;
        
        if (enCooldown[indexPoder]) return;
        if (indexPoder == 0)
        {
            GlobalData.mancha = true;

        }
        else if(indexPoder == 1)
        {
            GlobalData.hielo = true;

        }

        Debug.Log($"PODER {indexPoder} ACTIVADO!");

        // AQUÍ LLAMAS A TU PODER REAL
        // FindObjectOfType<EnvuelveJugador>().ActivarCubo();

        IniciarCooldown(indexPoder);
    }

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
            overlay.fillAmount = 0f;  // ¡EMPIEZA VACÍO!
        }

        float elapsed = 0f;
        while (elapsed < duracion)
        {
            elapsed += Time.unscaledDeltaTime;  // Usa unscaled para que funcione aunque el tiempo esté pausado
            float ratio = elapsed / duracion;

            if (overlay != null)
                overlay.fillAmount = ratio;  // ← SE LLENA DE IZQUIERDA A DERECHA

            yield return null;
        }

        // Finalizar
        if (overlay != null)
        {
            overlay.fillAmount = 1f;
            overlay.gameObject.SetActive(false);
        }

        btn.interactable = true;
        enCooldown[System.Array.IndexOf(botonesPoderes, btn)] = false;
    }

    // Para testing
    public void ResetearTodosLosCooldowns()
    {
        StopAllCoroutines();
        for (int i = 0; i < botonesPoderes.Length; i++)
        {
            if (botonesPoderes[i] != null) botonesPoderes[i].interactable = true;
            enCooldown[i] = false;
            if (i < cooldownOverlays.Length && cooldownOverlays[i] != null)
            {
                cooldownOverlays[i].fillAmount = 0f;
                cooldownOverlays[i].gameObject.SetActive(false);
            }
        }
    }
}