using Godot;
using Quake.Player;

namespace Quake.Network;

public abstract partial class ClientBase : NetworkManager
{


    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RecvMessage(string message) => OnGetMessage(message);

    protected virtual void OnGetMessage(string message)
    {
    }

    protected virtual void OnRecvUserInput(UserCommand input)
    {
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void RecvUserInput(Vector3 viewAngles,
                              float moveX,
                              float moveY,
                              bool jump)
                              => OnRecvUserInput(new UserCommand(viewAngles, moveX, moveY, jump));

    [Rpc]
    public void RecvClientData(byte[] data)
    {
    }

    protected virtual void OnGetClientData(byte[] data)
    {
    }
}
