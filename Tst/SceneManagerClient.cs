using Godot;
using Quake.Network;
using Quake.PlayerInput.ConsoleCommand;

namespace Quake;
internal partial class SceneManagerClient : SceneManager
{
    public ConsoleCommand Bind;
    private void BindCallback(ICommandParser commandParser)
    {
        // Needs 3 arguments: bind name argument_list
        if(commandParser.ArgC < 3)
        {
            throw new ConsoleCommandInvalidArgumentException("");
        }

        var bindName = commandParser.GetNthArg(1).Span;
        var commandName = commandParser.GetNthArg(2).Span;
    }

    public void RecvWorldState(Packet packet)
    {
    }

    public override void _Ready()
    {
        base._Ready();

        Bind = new ConsoleCommand("bind", "Binds a keypress to a command list", ConsoleCommandFlags.None,
                                  BindCallback);

        Registry.Register(Bind);


        this.AddChildDeffered(SceneManagerServer.TestStairsScene.Instantiate());
    }
}
