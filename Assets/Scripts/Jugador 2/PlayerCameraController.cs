using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;

    [Header("LÍMITES")]
    public Transform player;
    public bool limiteDetrasJugador = true;

    [Header("Límite dinámico con zoom")]
    public float avanceConZoom = 0.7f;
    // Cuánto avanza la cámara hacia delante por cada unidad de zoom out

    [Header("Límites Mundo (opcional)")]
    public bool useWorldBounds = false;
    public Vector2 minWorldBounds = new Vector2(-50, -50);
    public Vector2 maxWorldBounds = new Vector2(50, 50);

    private Camera minimapCam;
    private float lastZoom;

    void Start()
    {
        minimapCam = GetComponent<Camera>();
        lastZoom = minimapCam.orthographicSize;
    }

    void Update()
    {
        // Movimiento normal
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 movement = new Vector3(h, 0, v) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // =======================
        //      Z O O M  
        // =======================
        if (minimapCam.orthographic)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll != 0)
            {
                float oldSize = minimapCam.orthographicSize;

                minimapCam.orthographicSize = Mathf.Clamp(
                    minimapCam.orthographicSize - scroll * zoomSpeed,
                    5f, 50f
                );

                float newSize = minimapCam.orthographicSize;

                // Si hubo ZOOM OUT → la cámara debe avanzar hacia adelante (Z+)
                if (newSize > oldSize)
                {
                    float deltaZoom = newSize - oldSize;

                    transform.position += new Vector3(
                        0,
                        0,
                        deltaZoom * avanceConZoom
                    );
                }
            }
        }

        // =======================
        //  BARRERA DETRÁS DEL JUGADOR
        // =======================
        if (limiteDetrasJugador && player != null)
        {
            Vector3 pos = transform.position;

            if (pos.z < player.position.z)
            {
                pos.z = player.position.z;
            }

            transform.position = pos;
        }

        // =======================
        //     WORLD BOUNDS
        // =======================
        if (useWorldBounds)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minWorldBounds.x, maxWorldBounds.x);
            pos.z = Mathf.Clamp(pos.z, minWorldBounds.y, maxWorldBounds.y);
            transform.position = pos;
        }
    }
}
