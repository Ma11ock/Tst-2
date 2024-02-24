using System;
using System.Collections.Generic;
using System.Linq;

namespace Quake.PlayerInput.ConsoleCommand;

// TODO use a char[] instead of a string to store the parsed string
// and use Span<char> and Memory<char> instead of strings for ArgV.
// This would mean no allocations and would allow us to support thing
// like escaped strings.

public class CommandParser : ICommandParser
{
    public const int MAX_TOKENS = 1024;

    public const int MAX_COMMAND_BUFFER_SIZE = 4096 * 4;

    private ReadOnlyMemory<char>[] _args = new ReadOnlyMemory<char>[MAX_TOKENS];

    public IEnumerable<ReadOnlyMemory<char>> ArgV => (_args.Clone() as ReadOnlyMemory<char>[])!;

    private char[] _commandBuffer = new char[MAX_COMMAND_BUFFER_SIZE];

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

    public int ArgC { get; private set; } = 0;

    public int CommandBufferSize { get; private set; }

    public ReadOnlyMemory<char> GetNthArg(int i) => i < ArgC ? _args[i] : ReadOnlyMemory<char>.Empty;

    public bool Find(ReadOnlySpan<char> needle, out ReadOnlyMemory<char> result)
    {
        for (var i = 0; i < ArgC; i++)
        {
            if (needle.Equals(_args[i].Span, StringComparison.OrdinalIgnoreCase))
            {
                result = i + 1 < ArgC ? _args[i] : ReadOnlyMemory<char>.Empty;
                return true;
            }
        }

        result = ReadOnlyMemory<char>.Empty;
        return false;
    }

    public bool FindDouble(ReadOnlySpan<char> needle, out double result)
    {
        if (Find(needle, out var arg))
        {
            return double.TryParse(arg.Span, out result);
        }

        result = 0.0;
        return false;
    }

    protected bool ValidateCommandName(string name)
        => name != null && !name.Contains('\\') && !name.Contains('\"') && !name.Contains(';');

    private void PushTokenAt(int start, int len) => _args[ArgC++] = new ReadOnlyMemory<char>(_commandBuffer, start, len);

    private void PushChar(char c)
    {
        if (CommandBufferSize >= MAX_COMMAND_BUFFER_SIZE)
        {
            throw new CommandParsingException("", "", 0);
        }
        _commandBuffer[CommandBufferSize++] = c;
    }

    private bool IsEscapable(char c) => c == '"';

    private char GetEscapedVersion(char c)
    {
        var result = c;
        switch (c)
        {
            case '"':
                result = '"'; break;
            case 'n':
                result = '\n'; break;
            case 't':
                result = '\t'; break;
            case 'r':
                result = '\r'; break;
            default: break;
        }

        return result;
    }

    private void TokenizeCommand(string command)
    {
        // Reset everything.
        ArgC = 0;
        _Command = command ?? "";

        if (string.IsNullOrWhiteSpace(command)) return;

        var len = command.Length;

        // Start 1 character behind the beginning of the string.
        var ptr = -1;
        var curChar = '\0';
        var nextChar = command[0];

        Next();

        while (curChar != '\0')
        {
            // Parse tokens.
            if (ArgC == MAX_TOKENS) return;

            // Parse a single token, ignoring whitespace and comments.

            // Ignore whitespace.
            while (curChar != '\0' && char.IsWhiteSpace(curChar))
            {
                Next();
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
                Next(2);

                var foundEnd = false;
                while (!foundEnd && curChar != '\0')
                {
                    Next();
                    foundEnd = curChar == '*' && nextChar == '/';
                }

                if (!foundEnd)
                {
                    // No terminating */.
                    throw new CommandParsingException("/* comment encountered but no terminating */", command, ptr - 2);
                }
                // Skip the */.
                Next(2);
            }
            else if (curChar == '"')
            {
                // String token
                var strTokenPtr = CommandBufferSize;
                var stringTokenLen = 1; // Includes "
                PushChar(curChar);

                Next();

                while (curChar != '\0' && curChar != '"')
                {
                    if (curChar == '\\')
                    {
                        // Escaped char
                        if (nextChar == '\0')
                        {
                            // We don't allow nullbytes.
                            throw new CommandParsingException("", command, 0);
                        }
                        else if (!IsEscapable(nextChar))
                        {
                            // Inescapable.
                            throw new CommandParsingException("", command, 0);
                        }

                        // Skip the current character.
                        PushChar(GetEscapedVersion(nextChar));
                        Next(2);
                        stringTokenLen++;
                    }
                    else
                    {
                        PushChar(curChar);
                        Next();
                        stringTokenLen++;
                    }
                }

                if (curChar == '\0')
                {
                    // No endquote. Throw an exception.
                    throw new CommandParsingException("Staring quote encountered with no endquote (\")", command, ptr - 1);
                }

                PushChar(curChar);
                stringTokenLen++; // Include end quote.

                PushTokenAt(strTokenPtr, stringTokenLen);

                // Move over endquote.
                Next();
            }
            else
            {
                // Non string token.
                var tokenLen = 0;
                var tokenPtr = CommandBufferSize;
                while (curChar != '\0' && !char.IsWhiteSpace(curChar) &&
                       !(curChar == '/' && (nextChar == '/' || nextChar == '*') ||
                         curChar == '"'))
                {
                    PushChar(curChar);
                    tokenLen++;
                    Next();
                }

                PushTokenAt(tokenPtr, tokenLen);
            }
        }

        // Local function to increment curChar and nextChar.
        void Next(int incrementBy = 1)
        {
            while (incrementBy-- > 0)
            {
                ptr++;
                curChar = nextChar;
                nextChar = ptr + 1 < len ? command[ptr + 1] : '\0';
            }
        }
    }
}
