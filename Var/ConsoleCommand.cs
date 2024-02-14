using System;

namespace Quake;


public class ConsoleCommand : ConsoleObject
{
    public delegate void ConsoleCommandCallback(CommandParser parser);

    public ConsoleCommandCallback Callback;

    public ConsoleCommand(string name, string help, ConsoleCommandFlags flags, ConsoleCommandCallback callback)
        : base(name, help, flags)
    {
        if(callback == null) throw new ArgumentNullException(nameof(callback));
        Callback = callback;
    }

    public void Invoke(CommandParser parser) => Callback.Invoke(parser);
}
