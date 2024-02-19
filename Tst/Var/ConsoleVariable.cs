
using System;

namespace Quake;

public class ConsoleVariable : ConsoleObject
{
    public class CVarChangedEventArgs : EventArgs
    {
        public readonly string OldString;

        public readonly long OldInt;

        public readonly double OldValue;

        public CVarChangedEventArgs(string oldString, long oldInt, double oldValue)
        {
            OldString = oldString;
            OldInt = oldInt;
            OldValue = oldValue;
        }
    }

    public event EventHandler<CVarChangedEventArgs>? CVarChanged;

    public delegate string ValidateNewValue(string oldString, double oldValue, string newString, double newValue);

    private ValidateNewValue? ValidationCallback = null;

    public readonly string DefaultValue;

    private string _String = "";

    public string String
    {
        get => _String;
        set {
            if (ValidationCallback != null)
            {
                value = ValidationCallback.Invoke(String, Value, value,
                                                  Double.TryParse(value, out double tmpValue) ? tmpValue : 0.0);
            }

            string oldString = String;
            double oldValue = Value;
            _String = value ?? "";
            _Value = Double.TryParse(value, out double newValue) ? newValue : 0.0;
            ModificationCount++;
            CVarChanged?.Invoke(this, new CVarChangedEventArgs(oldString, (long)oldValue, oldValue));
        }
    }

    private double _Value;

    public double Value
    {
        get => _Value;
        set => String = value.ToString();
    }

    public long Int
    {
        get => (long)Value;
        set => Value = (double)value;
    }

    public int ModificationCount { get; protected set; } = 0;

    public void Reset() => String = DefaultValue;

    public ConsoleVariable(string name, string help, ConsoleCommandFlags flags, string defaultValue,
                           ValidateNewValue validationCallback)
        : base(name, help, flags)
    {
        DefaultValue = defaultValue;
        ValidationCallback = validationCallback;
    }

    public void SetValue(string value)
    {
        throw new System.NotImplementedException();
    }

    public void SetValue(long value)
    {
        throw new System.NotImplementedException();
    }

    public void SetValue(double value)
    {
        throw new System.NotImplementedException();
    }
}
