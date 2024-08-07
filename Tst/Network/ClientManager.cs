﻿using Godot;
using Serilog;
using Quake.Player;

namespace Quake.Network;

// Network manager on the client side.
public partial class ClientManager : ClientBase
{
    public const int NUM_COMMAND_BACKUPS = 64;
    public const int NUM_PACKET_BACKUPS = 32;

    public static readonly PackedScene SceneManagerClient = ResourceLoader.Load<PackedScene>(@"res://scene_manager_client.tscn");
    public ENetMultiplayerPeer _realClient { get; private set; }
    // TODO will need to make this more robust when we have the ability to move servers.
    public readonly string RemoteHost = "";

    public readonly int RemotePort = 0;

    private Main _main;

    private UserCommand[] _userCommands = new UserCommand[NUM_COMMAND_BACKUPS];

    private int _commandNumber = 0;

    public ClientManager(string remoteHost, int remotePort)
    {
        RemoteHost = remoteHost;
        RemotePort = remotePort;
    }

    public override void _Ready()
    {
        base._Ready();
        Log.Information("Starting client and connecting to server at {0}:{1}...", RemoteHost, RemotePort);

        Multiplayer.ConnectedToServer += _ConnectedToServer;
        Multiplayer.ServerDisconnected += _ServerDisconnected;
        Multiplayer.ConnectionFailed += _ConnectionFailed;
        _realClient = new ENetMultiplayerPeer();
        _realClient.CreateClient(RemoteHost, RemotePort);
        Multiplayer.MultiplayerPeer = _realClient;
        _main = GetTree().Root.GetNode<Main>("Main");

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

        // TODO some other way to determine the scene.
        this.AddChildDeffered(SceneManagerClient.Instantiate<SceneManagerClient>());
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
        if (_main == null)
        {
            return;
        }

        Packet p = new Packet();
        _main.SetNextWorldState(p);
    }


    protected override void OnGetMessage(string message)
    {
        Log.Information(message);
    }

    public void SendInput()
    {
    }

    public override void SendMessage(string message)
    {
        if (_realClient.GetConnectionStatus() != MultiplayerPeer.ConnectionStatus.Connected) return;
        RpcId(1, new StringName(nameof(ClientBase.RecvMessage)), message);
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

        RpcId(1, new StringName(nameof(ClientBase.RecvUserInput)), new Vector3(), 0.0f, 0.0f, false);
    }
}
