using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quake.PlayerInput.ConsoleCommand;

public interface IConsoleCommandMetaParser
{
    IEnumerable<ReadOnlyMemory<char>> GetCommandsFrom(ReadOnlySpan<char> @string);
    IEnumerable<ReadOnlyMemory<char>> GetCommandsFrom(StreamReader reader);
}
