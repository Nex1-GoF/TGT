using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
public static class InputEventBroker
{
    public static event Action<Key>? OnKeyInput;

    public static void RaiseKeyInput(Key key)
    {
        OnKeyInput?.Invoke(key);
    }
}