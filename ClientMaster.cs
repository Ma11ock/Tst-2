using Serilog;

namespace Quake.Net;
public partial class ClientMaster : ClientBase
{
    public long NetworkId { get; private set; }

    public ClientMaster(long networkId)
    {
        NetworkId = networkId;
    }

    public override void _Ready()
    {
        base._Ready();
        Name = NetworkId.ToString();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    protected override void OnGetMessage(string message)
    {
        // Send to other peers.
        long senderId = Multiplayer.GetRemoteSenderId();
        SendMessage(message);
        Log.Information("Player {0} said: \"{1}\"", senderId, message);
    }

    protected override void OnRecvUserInput(byte[] buffer)
    {
        Input s = new Input();
        Log.Information("Got buffer with len {0}", buffer.Length);
    }

    public override void _NetworkPeerConnected(long id)
    {
    }

    public override void _NetworkPeerDisconnected(long id)
    {
    }
}
