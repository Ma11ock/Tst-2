namespace Quake.PlayerInput.ConsoleCommand;

public interface IConsoleCommandRegistrar
{
    public ConsoleObject RegisterConsoleObject(ConsoleObject command);

    public ConsoleCommand RegisterConsoleCommand(ConsoleCommand command);

    public ConsoleVariable RegisterConsoleVariable(ConsoleVariable command);
}
