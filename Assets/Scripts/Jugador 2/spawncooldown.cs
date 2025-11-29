using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class spawncooldown : MonoBehaviour
{
    [Header("Cámaras")]
    public Camera camaraOrtogonal;
    public Camera camaraPrincipal;

    [Header("Minimap UI")]
    public RawImage rawImageMinimap;

    [Header("Configuración")]
    public float[] carrilesX = { 5f, 0f, -5f };
    public float alturaFija = 1f;

    [Header("Prefabs (dentro del script)")]
    public GameObject prefabTorreta1;
    public GameObject prefabTorreta2;

    [Header("Botones")]
    public Button[] botonesObjetos;
    public Button btnCancelar;

    [Header("Cooldown Visual")]
    public Image cooldown1;   // ⬅ Asignas desde el inspector (overlay del botón 1)
    public Image cooldown2;   // ⬅ Asignas desde el inspector (overlay del botón 2)
    public float tiempoCooldown = 5f;

    private GameObject objetoSeleccionado;
    private bool puedeColocar = false;

    void Start()
    {
        // Botones originales (NO SE CAMBIA NADA)
        if (botonesObjetos.Length >= 2)
        {
            botonesObjetos[0].onClick.AddListener(() =>
            {
                Seleccionar(prefabTorreta1, "Torreta1");
                //IniciarCooldown(0);
            });

            botonesObjetos[1].onClick.AddListener(() =>
            {
                Seleccionar(prefabTorreta2, "Torreta2");
                //IniciarCooldown(1);
            });
        }

        if (btnCancelar) btnCancelar.onClick.AddListener(Cancelar);

        // Minimap (igual)
        if (rawImageMinimap != null)
        {
            var trigger = rawImageMinimap.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((data) => ClickEnMinimap((PointerEventData)data));
            trigger.triggers.Add(entry);
            rawImageMinimap.raycastTarget = true;
        }

        // Inicializar cooldown visual
        if (cooldown1 != null) cooldown1.fillAmount = 0;
        if (cooldown2 != null) cooldown2.fillAmount = 0;
    }

    void Update()
    {
        if (!puedeColocar || objetoSeleccionado == null) return;

        if (camaraOrtogonal.targetTexture == null)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                ColocarConCamara(camaraOrtogonal);
            }
        }
    }

    void ClickEnMinimap(PointerEventData data)
    {
        if (!puedeColocar || objetoSeleccionado == null) return;
        if (rawImageMinimap.texture == null) return;

        RectTransform rt = rawImageMinimap.rectTransform;
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, data.position, data.pressEventCamera, out localPoint))
        {
            Vector2 norm = new Vector2(
                (localPoint.x - rt.rect.xMin) / rt.rect.width,
                (localPoint.y - rt.rect.yMin) / rt.rect.height
            );

            Vector2 pixel = new Vector2(norm.x * rawImageMinimap.texture.width, norm.y * rawImageMinimap.texture.height);
            Ray ray = camaraOrtogonal.ScreenPointToRay(pixel);
            ColocarConRayo(ray);
        }
    }

    void ColocarConRayo(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            int carril = EncontrarCarril(hit.point.x);
            Vector3 pos = new Vector3(carrilesX[carril], alturaFija, hit.point.z);
            Instantiate(objetoSeleccionado, pos, Quaternion.identity);
            Debug.Log($"OBJETO COLOCADO → Carril {carril + 1} (X={carrilesX[carril]})");

            // 🔥 NUEVO → Iniciar cooldown según el prefab seleccionado
            if (objetoSeleccionado == prefabTorreta1)
                IniciarCooldown(0);

            if (objetoSeleccionado == prefabTorreta2)
                IniciarCooldown(1);
        }

        Cancelar();
    }


    void ColocarConCamara(Camera cam)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        ColocarConRayo(ray);
    }

    void Seleccionar(GameObject prefab, string nombre)
    {
        objetoSeleccionado = prefab;
        puedeColocar = true;
        Debug.Log($"SELECCIONADO: {nombre}");
    }

    int EncontrarCarril(float x)
    {
        int best = 0;
        float minDist = Mathf.Abs(carrilesX[0] - x);
        for (int i = 1; i < carrilesX.Length; i++)
        {
            float d = Mathf.Abs(carrilesX[i] - x);
            if (d < minDist) { minDist = d; best = i; }
        }
        return best;
    }

    void Cancelar()
    {
        puedeColocar = false;
        objetoSeleccionado = null;
    }

    // ---------------------------------------------------------------------
    // ----------------------- SISTEMA DE COOLDOWN -------------------------
    // ---------------------------------------------------------------------

    void IniciarCooldown(int index)
    {
        Button btn = botonesObjetos[index];
        Image img = (index == 0) ? cooldown1 : cooldown2;

        btn.interactable = false;
        StartCoroutine(CooldownRutina(btn, img));
    }

    IEnumerator CooldownRutina(Button btn, Image overlay)
    {
        float t = tiempoCooldown;

        if (overlay != null)
            overlay.transform.localScale = Vector3.one;

        while (t > 0)
        {
            t -= Time.deltaTime;

            if (overlay != null)
            {
                float factor = t / tiempoCooldown;
                overlay.transform.localScale = new Vector3(factor, 1f, 1f);
            }

            yield return null;
        }

        if (overlay != null)
            overlay.transform.localScale = Vector3.zero;

        btn.interactable = true;
    }

}
