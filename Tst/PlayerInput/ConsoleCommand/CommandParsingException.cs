using System;

namespace Quake.PlayerInput.ConsoleCommand;

public class CommandParsingException : Exception
{
    public string Command { get; private set; }

    public int Where { get; private set; }

    public CommandParsingException(string what, string command, int @where)
        : base(what ?? "")
    {
        Command = command ?? "";
        Where = @where;
    }
}
