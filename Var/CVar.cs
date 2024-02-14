using System;

namespace Quake;

public abstract class CVar
{
    public class CVarChangedEventArgs : EventArgs
    {
        public readonly string NewValue;

        public CVarChangedEventArgs(string newValue)
        {
            NewValue = newValue;
        }
    }

    public event EventHandler<CVarChangedEventArgs> CVarChanged;

    public readonly string Name;

    public string _String = "";

    public string String
    {
        get => _String;
        set
        {
            _String = value;
            ModificationCount++;
            CVarChanged?.Invoke(this, new CVarChangedEventArgs(value));
        }
    }

    public int ModificationCount { get; protected set; }

    public CVar(string name)
    {
        Name = name;
    }
}
