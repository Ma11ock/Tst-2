

namespace Quake;

public abstract class ConsoleObject
{
    public readonly string Name;

    public readonly string Help;

    public ConsoleCommandFlags Flags { get; protected set; }

    public ConsoleCommandFlags AddFlags(ConsoleCommandFlags flags) => Flags |= flags;

    public ConsoleCommandFlags AddFlags(params ConsoleCommandFlags[] flags)
    {
        foreach (ConsoleCommandFlags flag in flags)
        {
            AddFlags(flag);
        }

        return Flags;
    }

    public bool HasFlags(ConsoleCommandFlags flags) => (Flags & flags) == flags;

    public ConsoleObject(string name, string help, ConsoleCommandFlags flags = ConsoleCommandFlags.None)
    {
        Name = name ?? "";
        Help = help ?? "";
        Flags = flags;
    }
}
