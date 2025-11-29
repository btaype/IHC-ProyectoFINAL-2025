using System;
using System.Collections.Generic;
using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;

public class GameServer : INetEventListener
{
    private NetManager server;

    private class Room
    {
        public NetPeer player1;
        public NetPeer player2;
        public DateTime lastActivity;
    }

    private List<Room> rooms = new List<Room>();
    private List<NetPeer> waitingPlayers = new List<NetPeer>();

    public static void Main()
    {
        new GameServer().Run();
    }

    public void Run()
    {
        server = new NetManager(this);
        server.Start(9050);
        Console.WriteLine("Servidor con salas activo en puerto 9050");

        while (true)
        {
            server.PollEvents();
            CheckRoomTimeouts();
            System.Threading.Thread.Sleep(15);
        }
    }


    private void CreateRoom(NetPeer p1, NetPeer p2)
    {
        var room = new Room()
        {
            player1 = p1,
            player2 = p2,
            lastActivity = DateTime.Now
        };

        rooms.Add(room);

        Console.WriteLine($"Sala creada: {p1.Address} <--> {p2.Address}");

        var writer = new NetDataWriter();
        writer.Put("{\"start\":1}");

        p1.Send(writer, DeliveryMethod.ReliableOrdered);
        p2.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private Room GetRoomOf(NetPeer peer)
    {
        foreach (var r in rooms)
        {
            if (r.player1 == peer || r.player2 == peer)
                return r;
        }
        return null;
    }

    private void DestroyRoom(Room room)
    {
        if (room == null) return;

        Console.WriteLine("Sala eliminada");

        // Desconectar a los dos
        room.player1?.Disconnect();
        room.player2?.Disconnect();

        rooms.Remove(room);
    }



    public void OnConnectionRequest(ConnectionRequest request)
    {
        request.Accept();
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Console.WriteLine($"Cliente conectado: {peer.Address}");

        waitingPlayers.Add(peer);

       
        if (waitingPlayers.Count >= 2)
        {
            var p1 = waitingPlayers[0];
            var p2 = waitingPlayers[1];

            waitingPlayers.RemoveAt(0);
            waitingPlayers.RemoveAt(0);

            CreateRoom(p1, p2);
        }
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
    {
        Console.WriteLine($"Cliente desconectado: {peer.Address}");

        
        waitingPlayers.Remove(peer);

       
        var room = GetRoomOf(peer);
        if (room != null)
        {
            DestroyRoom(room);
        }
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        string msg = reader.GetString();
        reader.Recycle();

        var room = GetRoomOf(peer);
        if (room == null) return;

        room.lastActivity = DateTime.Now; // actividad reciente

        // reenviar al compañero solamente
        NetPeer other = (room.player1 == peer) ? room.player2 : room.player1;

        if (other != null)
        {
            var writer = new NetDataWriter();
            writer.Put(msg);
            other.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
    public void OnNetworkError(IPEndPoint endPoint, System.Net.Sockets.SocketError socketError)
    {
        Console.WriteLine("Error de red: " + socketError);
    }
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }


   

    private void CheckRoomTimeouts()
    {
        var now = DateTime.Now;
        List<Room> toRemove = new List<Room>();

        foreach (var room in rooms)
        {
            if ((now - room.lastActivity).TotalMinutes >= 5)
            {
                Console.WriteLine("Sala desconectada por inactividad (5 min)");
                toRemove.Add(room);
            }
        }

        foreach (var room in toRemove)
            DestroyRoom(room);
    }
}
