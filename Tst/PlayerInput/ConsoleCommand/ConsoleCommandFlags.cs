
using System;

namespace Quake.PlayerInput.ConsoleCommand;

[Flags]
public enum ConsoleCommandFlags
{
    /// <summary>
    /// No flags.
    /// </summary>
    None = 0,
    /// <summary>
    /// Don't add to a registry.
    /// </summary>
    Unregistered = 1,
    /// <summary>
    /// Does not show up in autocompletes, etc..
    /// </summary>
    Hidden = 1 << 1,
    /// <summary>
    /// Only the server or an admin can change this variable.
    /// </summary>
    ServerOrCheat = 1 << 2,
    /// <summary>
    /// Save this variable.
    /// </summary>
    Save = 1 << 3,
    /// <summary>
    /// Notify the players when changed.
    /// </summary>
    Notify = 1 << 4,
    /// <summary>
    /// Do not ever print this variable to the screen.
    /// </summary>
    NoPrint = 1 << 5,
    /// <summary>
    /// This variable's string value can only contain printable values.
    /// </summary>
    PrintableOnly = 1 << 6,
    /// <summary>
    /// Log this variable's changes.
    /// </summary>
    Log = 1 << 7,
    /// <summary>
    /// If a server variable this value is replicated on clients.
    /// If a client we will try to replicate it on the server.
    /// </summary>
    Replicated = 1 << 8,
    /// <summary>
    /// This command is executable/controllable by the server.
    /// </summary>
    RemoteControllable = 1 << 9,
}
