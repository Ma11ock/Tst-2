
namespace Quake;

public interface IConsoleRegistry
{
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
    public ConsoleObject? Register(ConsoleObject consoleObject);

    /// <summary>
    /// Gets a registered <see cref="ConsoleObject"/> if it is already registered.
    /// </summary>
    /// <param name="name">The name of a <see cref="ConsoleObject"/>. Null will be treated as empty string.</param>
    /// <returns>
    /// A reference to a <see cref="ConsoleObject"/> if one is registered under name <see cref="name"/>.
    /// Null if there is none.
    /// </returns>
    public ConsoleObject? GetConsoleObject(string name);
}
