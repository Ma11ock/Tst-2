using System;
using System.Collections.Generic;

namespace Quake.PlayerInput.ConsoleCommand;

public interface ICommandParser
{
    public IEnumerable<ReadOnlyMemory<char>> ArgV { get; }

    public string Command { get; }

    public int ArgC { get; }

    public ReadOnlyMemory<char> GetNthArg(int i);

    public bool Find(ReadOnlySpan<char> needle, out ReadOnlyMemory<char> result);

    public bool FindDouble(ReadOnlySpan<char> needle, out double result);
}
