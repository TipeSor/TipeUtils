using System.Numerics;
using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public sealed class GuiMouseEventArgs : EventArgs
{
    public GuiMouseEventArgs(Vector2 position, Vector2? delta = null, Vector2? wheel = null, MouseButton? button = null)
    {
        Position = position;
        Delta = delta;
        Wheel = wheel;
        Button = button;
    }

    public Vector2 Position { get; }
    public Vector2? Delta { get; }
    public Vector2? Wheel { get; }
    public MouseButton? Button { get; }
}

public sealed class GuiKeyEventArgs : EventArgs
{
    public GuiKeyEventArgs(KeyboardKey key)
    {
        Key = key;
    }

    public KeyboardKey Key { get; }
}

public sealed class GuiTextInputEventArgs : EventArgs
{
    public GuiTextInputEventArgs(int codepoint)
    {
        Codepoint = codepoint;
    }

    public int Codepoint { get; }
}

public sealed class GuiValueChangedEventArgs<T> : EventArgs
{
    public GuiValueChangedEventArgs(T value)
    {
        Value = value;
    }

    public T Value { get; }
}
