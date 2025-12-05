using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LiteNetLib;
using LiteNetLib.Utils;

public class EntradaCamara : MonoBehaviour
{
    private UdpClient udp;
    public int listenPort = 5005;
    public Client client; // Arrastrar en inspector el script Client

    void Start()
    {
        udp = new UdpClient(listenPort);
        udp.Client.Blocking = false;
        Debug.Log("[UDP] Escuchando en puerto " + listenPort);

        // 🔹 Buscar automáticamente el Client si no está asignado
        if (client == null)
        {
            client = FindObjectOfType<Client>();
            if (client == null)
            {
                Debug.LogError("❌ No se encontró ningún script Client en la escena!");
            }
            else
            {
                Debug.Log("✅ Client encontrado automáticamente");
            }
        }
    }

    void Update()
    {
        while (udp.Available > 0)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, listenPort);
                byte[] data = udp.Receive(ref remoteEP);
                string msg = Encoding.UTF8.GetString(data);

                Debug.Log($"[UDP] Recibido: {msg}");

                // 🔹 Limpiar JSON antes de parsear
                msg = msg.Replace(" ", "");

                // 🔹 Intentar parsear a EstadoJugador
                try
                {
                    EstadoJugador estado = JsonUtility.FromJson<EstadoJugador>(msg);

                    Debug.Log($"📦 Parseado - Carril: {estado.poscarril}, Horizontal: {estado.poshorizontal}, Vel: {estado.velocidad}");

                    // 🔹 Encolar movimientos
                    if ((estado.velocidad == -6 || estado.velocidad == -5 || estado.velocidad == -4))
                    {
                        if (GlobalData.error_camara == 0)
                        {
                            GlobalData.error_camara = estado.velocidad;
                            Debug.Log($"❌ Error cámara registrado: {estado.velocidad}");
                        }
                        continue; // Salta al siguiente mensaje UDP
                    }
                    if (!string.IsNullOrEmpty(estado.poscarril))
                    {
                        
                        ControladorGeneral.colaMovimientos.Enqueue(estado.poscarril);
                        Debug.Log($" Encolado carril: {estado.poscarril}");
                    }

                    if (!string.IsNullOrEmpty(estado.poshorizontal))
                    {
                        ControladorGeneral.colaMovimientos.Enqueue(estado.poshorizontal);
                        Debug.Log($" Encolado horizontal: {estado.poshorizontal}");
                    }

                    // 🔹 Encolar velocidad\
                    if (estado.velocidad == -3)
                    {
                        estado.velocidad = 0;
                        GlobalData.pausa = true;
                    }
                    else {
                        GlobalData.pausa = false;
                    }
                    ControladorGeneral.colaVelocidades.Enqueue(estado.velocidad);
                    Debug.Log($" Velocidad encolada: {estado.velocidad}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"⚠ Error parseando mensaje: {e.Message}\nJSON: {msg}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[UDP] Error: {e.Message}");
            }
        }
    }

    void OnApplicationQuit()
    {
        udp?.Close();
    }
}