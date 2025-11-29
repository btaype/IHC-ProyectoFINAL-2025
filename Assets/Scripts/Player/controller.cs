using UnityEngine;

namespace Runner.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Carriles")]
        [SerializeField] private float[] lanes = new float[] { -5f, 0f, 5f };
        [SerializeField] private int currentLane = 1;
        [SerializeField] private float laneChangeSpeed = 10f;

        [Header("Movimiento hacia adelante")]
        [SerializeField] private bool movingForward = false;
        [SerializeField] private float forwardSpeed = 3f;

        [Header("Salto")]
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float jumpCooldown = 0.6f;
        [SerializeField] private float extraJumpMultiplier = 1.4f;
        [SerializeField] private float groundRayLength = 1.1f;
        [SerializeField] private LayerMask groundMask = ~0;

        private Rigidbody rb;
        private float targetLaneX;
        private float lastJumpTime = -999f;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            currentLane = Mathf.Clamp(currentLane, 0, lanes.Length - 1);
            targetLaneX = lanes[currentLane];
        }

        void FixedUpdate()
        {
            Vector3 vel = rb.linearVelocity;
            vel.z = movingForward ? forwardSpeed : 0f;

            float newX = Mathf.MoveTowards(rb.position.x, targetLaneX, laneChangeSpeed * Time.fixedDeltaTime);
            Vector3 targetPos = new Vector3(newX, rb.position.y, rb.position.z);

            rb.linearVelocity = new Vector3(vel.x, rb.linearVelocity.y, vel.z);
            rb.MovePosition(targetPos);
        }

        private bool IsGrounded() =>
            Physics.Raycast(rb.position, Vector3.down, groundRayLength, groundMask, QueryTriggerInteraction.Ignore);

        private bool CanJump() => IsGrounded() && (Time.time - lastJumpTime) >= jumpCooldown;

        // API p�blica
        public void SetMovingForward(bool state) => movingForward = state;
        public void MoveLeft() { if (currentLane > 0) SetTargetLane(currentLane - 1); }
        public void MoveRight() { if (currentLane < lanes.Length - 1) SetTargetLane(currentLane + 1); }
        public void SetTargetLane(int laneIndex)
        {
            laneIndex = Mathf.Clamp(laneIndex, 0, lanes.Length - 1);
            currentLane = laneIndex;
            targetLaneX = lanes[currentLane];
        }
        public void Jump()
        {
            if (!CanJump()) return;
            lastJumpTime = Time.time;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        public void JumpHigh()
        {
            if (!CanJump()) return;
            lastJumpTime = Time.time;
            rb.AddForce(Vector3.up * jumpForce * extraJumpMultiplier, ForceMode.Impulse);
        }
    }
}
