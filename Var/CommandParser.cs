using System.Linq;
using System;

namespace Quake;

public class CommandParser
{
    public string[] _args;

    public string[] ArgV => _args;

    public string[] ArgS => _args.Skip(1).ToArray();

    public int ArgC => _args.Length;

    public string Command => _args.Aggregate("", (current, next) => current + " " + next);

    public string this[int i]
    {
        get => _args[i];
        set => _args[i] = value;
    }

    public string? Find(string needle)
    {
        for (int i = 0; i < ArgC; i++)
        {
            if (string.Equals(needle, ArgV[i], StringComparison.OrdinalIgnoreCase))
            {
                return (i + 1) < ArgC ? ArgV[i] : null;
            }
        }

        return null;
    }

    public double? FindDouble(string needle)
    {
        string? result = Find(needle);
        if (result != null)
        {
            return Double.TryParse(result, out double dResult) ? dResult : null;
        }
        return null;
    }

    public CommandParser(string[] args)
    {
        _args = args;
    }
}
