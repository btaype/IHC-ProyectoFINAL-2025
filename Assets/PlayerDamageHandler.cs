using UnityEngine;
using System.Collections;
using System;                   // para Array.IndexOf
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class PlayerDamageHandler : MonoBehaviour
{
    [Header("RETROCESO Y ATURDIMIENTO")]
    public float fuerzaRetroceso = 12f;
    public float tiempoAturdido = 2f;

    [Header("EXCEPCIONES (opcional)")]
    public LayerMask capasQueIgnora = 0;   // Pon aquí capas que NO deben aturdir (ej: suelo)
    // Ejemplo: si quieres ignorar el suelo, pon Layer "Ground" en esta máscara

    [Header("EFECTOS")]
    public ParticleSystem efectoGolpe;
    public AudioClip sonidoGolpe;
    public Renderer[] renderersParaParpadeo;
    public Color colorAturdido = new Color(1f, 0.4f, 0.4f, 1f);

    public UnityEvent onRecibirGolpe;

    private Rigidbody rb;
    private AudioSource audioSource;
    private MonoBehaviour[] scriptsDeMovimiento;
    private bool estaAturdido = false;
    private Coroutine aturdimientoCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        scriptsDeMovimiento = GetComponents<MonoBehaviour>();
        if (renderersParaParpadeo == null || renderersParaParpadeo.Length == 0)
            renderersParaParpadeo = GetComponentsInChildren<Renderer>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Ignorar si está en la capa de excepción
        if (((1 << other.gameObject.layer) & capasQueIgnora) != 0)
            return;

        // Ignorar si es el propio jugador o sus hijos
        if (other.transform.IsChildOf(transform))
            return;

        // COLISIONA CON CUALQUIER COSA QUE TENGA COLLIDER
        RecibirGolpe();
    }

    // También funciona con colisiones físicas (no trigger)
    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & capasQueIgnora) != 0)
            return;

        if (collision.transform.IsChildOf(transform))
            return;

        RecibirGolpe();
    }

    public void RecibirGolpe()
    {
        if (estaAturdido) return;

        Debug.Log("¡GOLPE RECIBIDO! (sin tags)");

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(Vector3.back * fuerzaRetroceso, ForceMode.Impulse);

        efectoGolpe?.Play();
        if (sonidoGolpe) audioSource.PlayOneShot(sonidoGolpe);

        if (aturdimientoCoroutine != null) StopCoroutine(aturdimientoCoroutine);
        aturdimientoCoroutine = StartCoroutine(Aturdimiento());

        onRecibirGolpe?.Invoke();
    }

    IEnumerator Aturdimiento()
    {
        estaAturdido = true;

        foreach (var script in scriptsDeMovimiento)
            if (script != this) script.enabled = false;

        Color[] coloresOriginales = new Color[renderersParaParpadeo.Length];
        for (int i = 0; i < renderersParaParpadeo.Length; i++)
            coloresOriginales[i] = renderersParaParpadeo[i].material.color;

        for (int i = 0; i < 6; i++)
        {
            foreach (var r in renderersParaParpadeo) r.material.color = colorAturdido;
            yield return new WaitForSeconds(0.15f);
            for (int j = 0; j < renderersParaParpadeo.Length; j++)
                renderersParaParpadeo[j].material.color = coloresOriginales[j];
            yield return new WaitForSeconds(0.15f);
        }

        foreach (var script in scriptsDeMovimiento)
            if (script != this) script.enabled = true;

        estaAturdido = false;
    }

    public bool EstaAturdido() => estaAturdido;
}