using System;

namespace Quake;

public interface ICommandParser
{

    public Span<string> ArgV { get; }

    public string Command { get; }

    public int ArgC { get; }

    public string this[int i] { get; }

    public string? Find(string needle);

    public double? FindDouble(string needle);
}
