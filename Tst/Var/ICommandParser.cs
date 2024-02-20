using System;
using System.Collections.Generic;

namespace Quake;

public interface ICommandParser
{
    public IEnumerable<ReadOnlyMemory<char>> ArgV { get; }

    public string Command { get; }

    public int ArgC { get; }

    public ReadOnlyMemory<char> this[int i] { get; }

    public bool Find(ReadOnlySpan<char> needle, out ReadOnlyMemory<char> result);

    public bool FindDouble(ReadOnlySpan<char> needle, out double result);
}
