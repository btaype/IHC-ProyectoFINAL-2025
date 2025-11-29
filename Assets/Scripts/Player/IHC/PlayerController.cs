using UnityEngine;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider col;
    [SerializeField] private LayerMask groundMask;

    [Header("Forward / Run")]
    [SerializeField] private float runSpeed = 6f;
    private bool isRunning = false;

    [Header("Lanes")]
    [SerializeField] private int laneCount = 3;              // 3 carriles: izq-cent-der
    [SerializeField] private float laneWidth = 5f;
    [SerializeField] private float laneChangeDuration = 0.35f; // duración de transición
    private float[] lanesX;
    private int laneIdx;                                       // 0..laneCount-1
    private float laneStartX, laneTargetX, laneT;
    public bool IsLaneChanging { get; private set; }

    // Exponer duración (por si quieres consultarlo)
    public float LaneChangeDuration => laneChangeDuration;

    // Cola de siguiente carril si entra comando durante transición
    private int? pendingLane = null;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundExtra = 0.05f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!rb) rb = GetComponent<Rigidbody>();
        if (!col) col = GetComponent<CapsuleCollider>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Construir carriles centrados (-W, 0, +W)
        lanesX = new float[laneCount];
        int mid = laneCount / 2;
        for (int i = 0; i < laneCount; i++) lanesX[i] = (i - mid) * laneWidth;
        laneIdx = mid;

        // Constraints recomendadas
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void FixedUpdate()
    {
        // Avance hacia adelante
        if (isRunning)
            rb.MovePosition(rb.position + Vector3.forward * runSpeed * Time.fixedDeltaTime);

        // Transición de carril con easing visible
        if (IsLaneChanging)
        {
            laneT += Time.fixedDeltaTime / laneChangeDuration;
            if (laneT >= 1f) { laneT = 1f; IsLaneChanging = false; }

            // EASING: SmoothStep para que se vea fluido
            float t = Mathf.SmoothStep(0f, 1f, laneT);
            float newX = Mathf.Lerp(laneStartX, laneTargetX, t);
            Vector3 p = rb.position;
            rb.MovePosition(new Vector3(newX, p.y, p.z));

            // Si terminó y había un carril pendiente, lánzalo ahora
            if (!IsLaneChanging && pendingLane.HasValue)
            {
                int idx = pendingLane.Value; pendingLane = null;
                MoveToLane(idx);
            }
        }
    }

    // ==== API (la llama PlayerActions) ====
    public void SetRun(bool on) => isRunning = on;

    public void Jump()
    {
        if (IsGrounded()) rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void MoveLeft() => MoveToLane(Mathf.Clamp(laneIdx - 1, 0, laneCount - 1));
    public void MoveRight() => MoveToLane(Mathf.Clamp(laneIdx + 1, 0, laneCount - 1));
    public void MoveCenter() => MoveToLane(laneCount / 2);

    public void Stop()
    {
        isRunning = false;
        rb.linearVelocity = Vector3.zero;
    }

    public void MoveToLane(int idx)
    {
        idx = Mathf.Clamp(idx, 0, laneCount - 1);

        // Si está en transición, no cortar: encolar siguiente carril
        if (IsLaneChanging)
        {
            pendingLane = idx;
            return;
        }

        // Si ya está en ese carril y no hay transición, no hacer nada
        if (idx == laneIdx && !IsLaneChanging) return;

        laneIdx = idx;
        laneStartX = rb.position.x;
        laneTargetX = lanesX[laneIdx];
        laneT = 0f;
        IsLaneChanging = true;
    }

    private bool IsGrounded()
    {
        if (col != null)
        {
            Vector3 center = col.bounds.center;
            Vector3 bottom = new Vector3(center.x, col.bounds.min.y + 0.05f, center.z);
            float radius = Mathf.Max(0.05f, col.radius * 0.9f);
            int mask = (groundMask.value != 0) ? (int)groundMask : ~0; // All layers si no asignas mask
            return Physics.CheckCapsule(center, bottom, radius, mask, QueryTriggerInteraction.Ignore);
        }
        // Fallback
        return Physics.Raycast(rb.position, Vector3.down, 1.1f);
    }
}
