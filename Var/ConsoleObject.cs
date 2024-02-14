

namespace Quake;

public abstract class ConsoleObject
{
    public readonly string Name;

    public readonly string Help;

    public ConsoleCommandFlags Flags { get; }

    public ConsoleObject(string name, string help, ConsoleCommandFlags flags)
    {
        Name = name ?? "";
        Help = help ?? "";
        Flags = flags;
    }
}
