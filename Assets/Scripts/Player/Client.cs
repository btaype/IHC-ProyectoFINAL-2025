using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;

public class Client : MonoBehaviour, INetEventListener
{
    private NetManager client;
    private NetPeer server;
    public ReceptorServidor receptorServidor; // Arrastrar en inspector
    public NetPeer Server => server;

    [Header("Configuración de conexión")]
    public string ip = "10.244.234.196";
    public int puerto = 9050;
    public string connectionKey = "game_key";

    private bool intentandoReconectar = false;

    void Start()
    {
        client = new NetManager(this);
        client.Start();
        Conectar();
    }

    void Update()
    {
        client.PollEvents();

        // Desconectar cuando el juego finaliza
        if (GlobalData.final)
        {
            Desconectar();
        }
    }

    public void Conectar()
    {
        Debug.Log("🔌 Intentando conectar al servidor...");
        server = client.Connect(ip, puerto, connectionKey);
    }

    public void Desconectar()
    {
        if (server != null && server.ConnectionState == ConnectionState.Connected)
        {
            Debug.Log("🔌 Desconectando del servidor...");

            // Detener intentos de reconexión
            intentandoReconectar = false;
            StopAllCoroutines();

            // Desconectar del servidor
            server.Disconnect();
            server = null;

            Debug.Log("✅ Desconectado correctamente");
        }

        // Detener el cliente
        if (client != null)
        {
            client.Stop();
        }
    }

    public void OnPeerConnected(NetPeer peer)
    {
        server = peer;
        intentandoReconectar = false;
        Debug.Log("✅ Conectado al servidor!");
    }

    public event System.Action<string> OnMensajeRecibido;

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod method)
    {
        string msg = reader.GetString();
        reader.Recycle();
        OnMensajeRecibido?.Invoke(msg);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
    {
        Debug.LogWarning($"⚠️ Desconectado del servidor: {info.Reason}");
        server = null;

        // Solo reintentar si no es una desconexión intencional
        if (!intentandoReconectar && !GlobalData.final)
        {
            StartCoroutine(ReintentarConexion());
        }
    }

    private IEnumerator ReintentarConexion()
    {
        intentandoReconectar = true;
        int intentos = 0;

        while (server == null && !GlobalData.final)
        {
            intentos++;
            Debug.Log($"🔁 Reintentando conexión... intento #{intentos}");
            Conectar();

            // Espera 3 segundos antes del siguiente intento
            yield return new WaitForSeconds(3f);

            if (server != null && server.ConnectionState == ConnectionState.Connected)
            {
                Debug.Log("✅ Reconectado correctamente!");
                yield break;
            }
        }

        intentandoReconectar = false;
    }

    public void OnNetworkError(System.Net.IPEndPoint ep, System.Net.Sockets.SocketError error)
    {
        Debug.LogError($"❌ Error de red: {error}");
    }

    public void OnNetworkReceiveUnconnected(System.Net.IPEndPoint ep, NetPacketReader reader, UnconnectedMessageType type) { }
    public void OnConnectionRequest(ConnectionRequest request) { request.Accept(); }
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }

    void OnDestroy()
    {
        // Asegurarse de desconectar al destruir el objeto
        Desconectar();
    }
}