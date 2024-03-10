using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quake.PlayerInput.ConsoleCommand;

public interface ICommandMetaParser
{
    ReadOnlyMemory<char>[] ParseCommands(ReadOnlyMemory<char> commands);
    ReadOnlyMemory<char>[] ParseCommands(StreamReader reader);
}
