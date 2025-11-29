using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;

public class PythonReceiver2 : MonoBehaviour
{
    UdpClient udp;
    Thread thread;
    string latestMessage = "";

    private GameObject player;
    private Rigidbody playerRigidbody;

    private float[] lanes = new float[] { -5f, 0f, 5f }; // carriles fijos
    private int currentLane = 1;
    private float laneChangeSpeed = 10f;
    private bool isMoving = false;

    private bool movingForward = false;
    private float moveSpeed = 0f;

    private bool canJump = true;
    private float jumpForce = 5f;

    void Start()
    {
        udp = new UdpClient(5005);
        thread = new Thread(new ThreadStart(ReceiveData));
        thread.IsBackground = true;
        thread.Start();

        player = GameObject.Find("Player");
        if (player != null)
        {
            playerRigidbody = player.GetComponent<Rigidbody>();
        }
        if (player == null || playerRigidbody == null)
        {
            Debug.LogError("Player GameObject or Rigidbody not found. Asegúrate de tener un objeto llamado 'Player' con Rigidbody.");
            return;
        }
    }

    void ReceiveData()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            byte[] data = udp.Receive(ref remoteEP);
            latestMessage = Encoding.UTF8.GetString(data);
        }
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(latestMessage))
        {
            Debug.Log("[PYTHON] Received: " + latestMessage);

            // --- Interpretar mensajes ---
            if (latestMessage.StartsWith("velocidad"))
            {
                string[] parts = latestMessage.Split(' ');
                if (parts.Length == 2 && int.TryParse(parts[1], out int vel))
                {
                    vel = Mathf.Clamp(vel, 1, 5);
                    moveSpeed = vel * 2.0f; // escala de velocidad
                    movingForward = true;
                    Debug.Log($"[PYTHON] corriendo, velocidad {vel}");
                }
            }
            else if (latestMessage == "correr_activado")
            {
                Debug.Log("[PYTHON] Modo correr activado. Esperando pasos...");
                moveSpeed = 0f;
                movingForward = false;
            }
            else if (latestMessage == "no_person")
            {
                Debug.Log("[PYTHON] Sin persona detectada. Detenido.");
                moveSpeed = 0f;
                movingForward = false;
            }
            else
            {
                // Estados combinados: Hands, Horizontal, Vertical
                string[] parts = latestMessage.Split(',');
                if (parts.Length == 3)
                {
                    string hands = parts[0];
                    string horizontal = parts[1];
                    string vertical = parts[2];

                    // --- Movimiento lateral ---
                    if (!isMoving)
                    {
                        if (horizontal == "Left" && currentLane != 0)
                        {
                            ChangeLane(0);
                        }
                        else if (horizontal == "Center" && currentLane != 1)
                        {
                            ChangeLane(1);
                        }
                        else if (horizontal == "Right" && currentLane != 2)
                        {
                            ChangeLane(2);
                        }
                    }

                    // --- Salto ---
                    if (vertical == "Jumping" && canJump && IsGrounded())
                    {
                        Jump();
                    }
                }
            }
        }

        // --- Movimiento hacia adelante ---
        if (movingForward)
        {
            player.transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed, Space.World);
        }

        // --- Movimiento lateral (suavizado) ---
        Vector3 targetPosition = new Vector3(lanes[currentLane], player.transform.position.y, player.transform.position.z);
        player.transform.position = Vector3.Lerp(player.transform.position, targetPosition, Time.deltaTime * laneChangeSpeed);

        if (Vector3.Distance(player.transform.position, targetPosition) < 0.01f)
        {
            isMoving = false;
            player.transform.position = targetPosition;
        }
    }

    private void ChangeLane(int laneIndex)
    {
        if (laneIndex >= 0 && laneIndex < lanes.Length)
        {
            currentLane = laneIndex;
            isMoving = true;
            Debug.Log($"[PYTHON] Cambio de carril a {laneIndex} (x = {lanes[currentLane]})");
        }
    }

    private void Jump()
    {
        Debug.Log("[PYTHON] Salto detectado");
        playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        StartCoroutine(JumpCooldown());
    }

    private IEnumerator JumpCooldown()
    {
        canJump = false;
        yield return new WaitForSeconds(2f); // cooldown de 2 segundos
        canJump = true;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(player.transform.position, Vector3.down, 1.1f);
    }

    private void OnApplicationQuit()
    {
        udp.Close();
        thread.Abort();
    }
}
