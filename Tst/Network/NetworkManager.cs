
using Godot;
using System.Net;
using System.Net.Sockets;
using Serilog;

namespace Quake.Network;

public abstract partial class NetworkManager : Godot.Node
{
    public const int DEFAULT_PORT = 28960;

    public const int MAX_CLIENTS = 64;

    public const string DEFAULT_IP = "localhost";

    private static readonly IPEndPoint DEFAULT_ENDPOINT = new IPEndPoint(IPAddress.Loopback, port: 0);
    /// <summary>
    /// Get a free UDP port from the OS. This function is a temporary solution until Godot 4.
    /// There could potentially be a race condition where another process uses our port before
    /// we can bind to it. However, this is unlikely since the OS will usually wait until all
    /// other free ports are allocated before wrapping around again.
    /// </summary>
    /// <returns>A (probably valid) free UDP port.</returns>
    public static int FreeUDPPort()
    {
        using (Socket socket =
                   new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            socket.Bind(DEFAULT_ENDPOINT);
            return ((IPEndPoint)socket.LocalEndPoint).Port;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        // Connect networking signals.
        Multiplayer.PeerConnected += _NetworkPeerConnected;
        Multiplayer.PeerDisconnected += _NetworkPeerDisconnected;
    }

    public void ResetNetworkConnection()
    {
        if (Multiplayer.HasMultiplayerPeer())
        {
            Multiplayer.MultiplayerPeer = null;
        }
    }

    public abstract void _NetworkPeerConnected(long id);

    public abstract void _NetworkPeerDisconnected(long id);

    public virtual void SendMessage(string message)
    {
    }

    public bool IsServer() => Multiplayer.IsServer();
    public bool IsAClient() => !Multiplayer.IsServer();

}
