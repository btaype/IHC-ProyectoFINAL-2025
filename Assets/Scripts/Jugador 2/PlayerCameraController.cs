using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;
    
    [Header("LÍMITES")]
    public Transform player;           // ← Jugador1 (barrera Z)
    public bool limiteDetrasJugador = true;  // Activa/desactiva barrera
    
    [Header("Límites Mundo (opcional)")]
    public bool useWorldBounds = false;
    public Vector2 minWorldBounds = new Vector2(-50, -50);
    public Vector2 maxWorldBounds = new Vector2(50, 50);

    private Camera minimapCam;

    void Start()
    {
        minimapCam = GetComponent<Camera>();
    }

    void Update()
    {
        // Movimiento flechas
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 movement = new Vector3(h, 0, v) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // Zoom rueda mouse
        if (minimapCam.orthographic)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            minimapCam.orthographicSize = Mathf.Clamp(
                minimapCam.orthographicSize - scroll * zoomSpeed,
                5f, 50f
            );
        }

        // === BARRERA DETRÁS DEL JUGADOR ===
        if (limiteDetrasJugador && player != null)
        {
            Vector3 pos = transform.position;
            if (pos.z < player.position.z)
            {
                pos.z = player.position.z;  // No pasa del jugador
            }
            transform.position = pos;
        }

        // Límites mundo (opcional)
        if (useWorldBounds)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minWorldBounds.x, maxWorldBounds.x);
            pos.z = Mathf.Clamp(pos.z, minWorldBounds.y, maxWorldBounds.y);
            transform.position = pos;
        }
    }
}