
using System;
using System.Collections.Generic;

namespace Quake.PlayerInput.ConsoleCommand;

public class ConsoleRegistry : IConsoleRegistry
{
    private Dictionary<string, ConsoleObject> _commands = new Dictionary<string, ConsoleObject>();

    /// <summary>
    /// Registers a <see cref="ConsoleObject"/> for access to anything that has a handle to
    //// this <see cref="IConsoleRegistry"/> instance.
    /// </summary>
    /// <param name="consoleObject">The <see cref="ConsoleObject"/> to register.</param>
    /// <returns>
    /// A reference to <see cref="consoleObject"/> if the console object is not already registered.
    /// A reference to an already registered <see cref="ConsoleObject"/> if there is already an object
    /// with name registered previously.
    /// Null if the object cannot be registered.
    /// </returns>
    /// <exception cref="Argumentnullexception">If consoleObject is null.</exception>
    public ConsoleObject? Register(ConsoleObject consoleObject)
    {
        if (consoleObject == null) throw new ArgumentNullException(nameof(consoleObject));

        if (consoleObject.HasFlags(ConsoleCommandFlags.Unregistered)) return null;

        if (_commands.TryGetValue(consoleObject.Name, out var maybeResult) && maybeResult != null)
        {
            return maybeResult;
        }

        _commands[consoleObject.Name] = consoleObject;
        return consoleObject;
    }

    /// <summary>
    /// Gets a registered <see cref="ConsoleObject"/> if it is already registered.
    /// </summary>
    /// <param name="name">The name of a <see cref="ConsoleObject"/>. Null will be treated as empty string.</param>
    /// <returns>
    /// A reference to a <see cref="ConsoleObject"/> if one is registered under name <see cref="name"/>.
    /// Null if there is none.
    /// </returns>
    public ConsoleObject? GetConsoleObject(string name) => _commands.TryGetValue(name ?? "", out var result) ? result : null;
}
