using Godot;

namespace Quake.Network;
public abstract partial class ClientBase : NetworkManager
{


    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RecvMessage(string message) => OnGetMessage(message);

	protected virtual void OnGetMessage(string message)
	{
	}

	protected virtual void OnRecvUserInput(byte[] buffer)
	{
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void RecvUserInput(byte[] b) => OnRecvUserInput(b);

	[Rpc]
	public void RecvClientData(byte[] data)
	{
	}

	protected virtual void OnGetClientData(byte[] data)
	{
	}
}
