using Godot;
using Serilog;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;

namespace Quake.Net;
// Server running server side
public partial class ServerManager : Quake.Net.NetworkManager
{
	private ENetMultiplayerPeer _realServer = null;

	public readonly int LocalPort;

	private Dictionary<int, ClientMaster> _clients = new Dictionary<int, ClientMaster>();

	public static string GetInitFileDirectory() => Path.GetTempPath();

	public static string GetInitFilePath(int port) => Path.Combine(GetInitFileDirectory(), $"Tst_{port}.txt"); 

	public ServerManager() : this(0)
	{
	}
	public ServerManager(int localPort)
	{
		LocalPort = localPort;

		if (LocalPort == 0)
		{
			LocalPort = FreeUDPPort();
		}
	}

	public override void _Ready()
	{
		base._Ready();
		// For enet.
		Name = "1";

		Log.Information("Starting a server on port {0}...", LocalPort);
		_realServer = new ENetMultiplayerPeer();
		_realServer.SetBindIP("*"); // TODO configuration option for this.
		switch(_realServer.CreateServer(LocalPort, MAX_CLIENTS))
		{
			case Error.AlreadyInUse:
				Log.Error("Could not create server on port {0}: there is already a server listening with peers", LocalPort);
				GetTree().Quit();
				break;
			case Error.CantCreate:
				Log.Error("Could not create server on port {0}", LocalPort);
				GetTree().Quit();
				break;
			default:
				break;
		}
		Multiplayer.MultiplayerPeer = _realServer;
		string initFilePath = GetInitFilePath(LocalPort);
		Log.Information("Started server on port {0}... creating init file at '{1}' to tell the client", LocalPort, initFilePath);
		try
		{
			File.Create(initFilePath).Close();
		}
		catch (Exception ex)
		{
			Log.Error("Could not create file '{0}': {1}", initFilePath, ex);
		}

		// TODO some other way to determine the scene.
		this.AddChildDeffered(new SceneManagerServer());
	}

	private void FailedToAcceptClient(object sender, Exception ex)
	{
		Log.Error("Could not accept client: {0}", ex);
	}

	private void OnClientConnected(object sender, EventArgs e)
	{
		Log.Information("Client connected to the pipe.");
	}

	private void OnClientDisconnected(object sender, EventArgs e)
	{
		Log.Information("Client disconnected to the pipe.");
	}

	private void OnGotMessage(object sender, byte[] data)
	{
		Log.Information("Got information from the pipe.");
		try
		{
			Log.Information("It's {0}", Encoding.UTF8.GetString(data));
		}
		catch
		{

		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		Log.Information("Ending server session");
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}


	public override void SendMessage(string message)
	{
		Rpc(new StringName(nameof(ClientManager.RecvMessage)), message);
	}

	public override void _NetworkPeerConnected(long longId)
	{
		// Casting the long to an int is not a problem, enet only uses the bottom 32 bits.
		int id = (int)longId;
		ENetPacketPeer p = _realServer.GetPeer(id);
		ClientMaster cm = new ClientMaster(p.GetRemoteAddress(), p.GetRemotePort(), id);
		GetTree().Root.AddChild(cm);
		_clients[id] = cm;
		Log.Information("Added remote client with id {0} from {1}:{2}", id, cm.RemoteIp, cm.RemotePort);
	}

	public override void _NetworkPeerDisconnected(long longId)
	{
		int id = (int)longId;
		ClientMaster disconnectingClient = _clients[id];
		Log.Information("Removing remote client with id {0} from {1}:{2}", id, disconnectingClient.RemoteIp, disconnectingClient.RemotePort);

		_clients.Remove(id);
		disconnectingClient.QueueFree();

		// Remove player from the scene tree

	}
}
