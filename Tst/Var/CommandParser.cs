using System.Linq;
using System;

namespace Quake;

public class CommandParser : ICommandParser
{
    public const int MAX_TOKENS = 1024;

    // TODO this is a naive implementation that requires constantly deep copying strings.
    private string[] _args = new string[MAX_TOKENS];

    public Span<string> ArgV => new Span<string>(_args, 0, ArgC);

    public string _Command = "";

    public string Command
    {
        get => _Command;
        set
        {
            value ??= "";
            TokenizeCommand(value);
            _Command = value;
        }
    }

    public Span<string> ArgS => ArgC == 0 ? new Span<string>() : new Span<string>(_args, 1, ArgC - 1);

    public int ArgC { get; private set; } = 0;

    public string this[int i]
    {
        get => ArgV[i];
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

    protected bool ValidateCommandName(string name)
        => (name != null) && !name.Contains('\\') && !name.Contains('\"') && !name.Contains(';');

    private void PushToken(string token) => _args[ArgC++] = token;

    private void TokenizeCommand(string command)
    {
        // Reset everything.
        ArgC = 0;
        _Command = command ?? "";

        if (string.IsNullOrWhiteSpace(command)) return;

        int len = command.Length;
        int ptr = 0;
        while(ptr < len)
        {
            // Parse tokens.
            if (ArgC == MAX_TOKENS) return;

            char curChar = command[ptr];
            char nextChar = ptr + 1 < len ? command[ptr + 1] : '\0';

            while (ptr < len)
            {
                // Parse a single token, ignoring whitespace.

                while (curChar != '\0' && Char.IsWhiteSpace(curChar))
                {
                    // Ignore whitespace.
                    curChar = nextChar;
                    nextChar = ptr + 1 < len ? command[ptr + 1] : '\0';
                    ptr++;
                }

                if (curChar == '/' && nextChar == '/')
                {
                    // Ignore // Comments
                    // Command should only be a single line, so we're done.
                    return;
                }
                else if (curChar == '/' && nextChar == '*')
                {
                    // Ignore Between /* until */
                    ptr += 2;


                    bool foundEnd = false;
                    while (ptr + 1 < len && !foundEnd)
                    {
                        curChar = nextChar;
                        nextChar = ptr + 1 < len ? command[ptr + 1] : '\0';

                        foundEnd = curChar == '/' && nextChar == '*';
                        ptr += 2;
                    }

                    if (!foundEnd)
                    {
                        // No terminating */, error
                        throw new CommandParsingException("/* comment encountered but no terminating */", command, ptr - 2);
                    }
                }
                else if (curChar == '”')
                {
                    throw new CommandParsingException("Ending quote ” encountered with no opening quote (either \" or “)", command, ptr);
                }
                else if (curChar == '"' || curChar == '“')
                {
                    // String token
                    int strTokenPtr = ++ptr;
                    int stringTokenLen = 0;

                    curChar = nextChar;
                    nextChar = ptr + 1 < len ? command[ptr + 1] : '\0';

                    while (curChar != '\0' && curChar != '"' && curChar != '”')
                    {
                        stringTokenLen++;
                        ptr++;

                        curChar = nextChar;
                        nextChar = ptr + 1 < len ? command[ptr + 1] : '\0';
                    }

                    PushToken(command.Substring(strTokenPtr, stringTokenLen));
                    break;
                }

                int tokenLen = 0;
                int tokenPtr = ptr;
                while (curChar != '\0')
                {
                    // Non string token.

                    // Check for comment or quote.
                    if ((curChar == '/' && (nextChar == '/' || nextChar == '*')) ||
                        (curChar == '"' || curChar != '”' || curChar == '“'))
                    {
                        break;
                    }

                    tokenLen++;
                    ptr++;
                    curChar = nextChar;
                    nextChar = ptr + 1 < len ? command[ptr + 1] : '\0';
                }

                PushToken(command.Substring(tokenPtr, tokenLen));
            }
        }
    }
}
