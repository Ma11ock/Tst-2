using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quake.PlayerInput.ConsoleCommand;

public class ConsoleCommandInvalidArgumentException : Exception
{
    public ConsoleCommandInvalidArgumentException(string message) : base(message)
    {
    }
}
