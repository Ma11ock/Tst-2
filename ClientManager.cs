using Godot;
using Serilog;

namespace Quake.Net;

// Network manager on the client side.
public partial class ClientManager : ClientBase
{
    public ENetMultiplayerPeer _realClient { get; private set; } = null;
    // TODO will need to make this more robust when we have the ability to move servers.
    public readonly string RemoteHost = "";

    public readonly int RemotePort = 0;

    private Main _Main;

    public ClientManager(string remoteHost, int remotePort)
    {
        RemoteHost = remoteHost;
        RemotePort = remotePort;
    }

    public override void _Ready()
    {
        base._Ready();
        Log.Information("Starting wclient and connecting to server at {0}:{1}...", RemoteHost, RemotePort);

        Multiplayer.ConnectedToServer += _ConnectedToServer;
        Multiplayer.ServerDisconnected += _ServerDisconnected;
        Multiplayer.ConnectionFailed += _ConnectionFailed;
        _realClient = new ENetMultiplayerPeer();
        _realClient.CreateClient(RemoteHost, RemotePort);
        Multiplayer.MultiplayerPeer = _realClient;
        _Main = GetTree().Root.GetNode<Main>("Main");

        // For enet recving.
        Name = Multiplayer.GetUniqueId().ToString();
        Log.Information("Created client on {0}:{1} with unique id {2}.", RemoteHost, RemotePort, Multiplayer.GetUniqueId());
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Log.Information("Ending client session");
    }

    public void _ConnectedToServer()
    {
        Log.Information("Successfully connected to server.");
    }

    public void _ServerDisconnected()
    {
        Log.Information("Server disconnected.");
        ResetNetworkConnection();
    }

    public void _ConnectionFailed()
    {
        Log.Information("Connection to server failed.");
        ResetNetworkConnection();
    }

    protected override void OnGetClientData(byte[] data)
    {
        if (_Main == null)
        {
            return;
        }

        Packet p = new Packet();
        _Main.SetNextWorldState(p);
    }


    protected override void OnGetMessage(string message)
    {
        Log.Information(message);
    }


    public override void SendMessage(string message)
    {
        if (_realClient.GetConnectionStatus() != MultiplayerPeer.ConnectionStatus.Connected) return;
        RpcId(1, new StringName(nameof(ClientMaster.RecvMessage)), message);
    }

    public override void _NetworkPeerConnected(long id)
    {
        if (id == 1) return;

        Log.Information("Connected to player with id {0}", id);
    }

    public override void _NetworkPeerDisconnected(long id)
    {
        if (id == 1) return;

        Log.Information("Disconnected with player with id {0}", id);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_realClient.GetConnectionStatus() != MultiplayerPeer.ConnectionStatus.Connected) return;

        Input i = new Input() { Msg = "Damn" };

        RpcId(1, new StringName(nameof(ClientBase.RecvUserInput)), Variant.From(1));
    }
}
