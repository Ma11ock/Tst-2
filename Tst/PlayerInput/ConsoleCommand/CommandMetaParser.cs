using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quake.PlayerInput.ConsoleCommand;

public class CommandMetaParser : ICommandMetaParser
{

    public ReadOnlyMemory<char>[] ParseCommands(ReadOnlyMemory<char> commands)
    {
        bool isInString = false;
        bool nextCharIsEscaped = false;
        int numTotalCommands = 0;
        foreach (char c in commands.Span)
        {
            // Find all the commands.
            if (c == '\\')
            {
                nextCharIsEscaped = !nextCharIsEscaped;
                continue;
            }
            switch (c)
            {
                case '"':
                    isInString = (!nextCharIsEscaped && !isInString);
                    break;
                case ';':
                    numTotalCommands += isInString ? 0 : 1;
                    break;
                default:
                    break;
            }
            nextCharIsEscaped = false;
        }

        if (isInString)
        {
            // Should not end in a string.
            throw new CommandParsingException("", "", 0);
        }

        ReadOnlyMemory<char>[] result = new ReadOnlyMemory<char>[numTotalCommands];
        isInString = false;
        int curSpan = 0;
        int begin = 0;
        int end = 0;
        foreach (char c in commands.Span)
        {
            // Find all the commands.
            if (c == ';' && !isInString)
            {
                result[curSpan++] = commands.Slice(begin, end);
                begin = end + 1;
                continue;
            }
            else if (c == '"')
            {
                if (isInString)
                {
                    result[curSpan++] = commands.Slice(begin, end);
                }
                isInString = !isInString;
            }

            end++;
        }

        return result;
    }

    public ReadOnlyMemory<char>[] ParseCommands(StreamReader reader)
    {
        return null;
    }
}
