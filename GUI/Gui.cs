using System.Numerics;
using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public class Gui
{
    private static readonly KeyboardKey[] PolledKeys =
    [
        KeyboardKey.Space,
        KeyboardKey.Apostrophe,
        KeyboardKey.Comma,
        KeyboardKey.Minus,
        KeyboardKey.Period,
        KeyboardKey.Slash,
        KeyboardKey.Zero,
        KeyboardKey.One,
        KeyboardKey.Two,
        KeyboardKey.Three,
        KeyboardKey.Four,
        KeyboardKey.Five,
        KeyboardKey.Six,
        KeyboardKey.Seven,
        KeyboardKey.Eight,
        KeyboardKey.Nine,
        KeyboardKey.Semicolon,
        KeyboardKey.Equal,
        KeyboardKey.A,
        KeyboardKey.B,
        KeyboardKey.C,
        KeyboardKey.D,
        KeyboardKey.E,
        KeyboardKey.F,
        KeyboardKey.G,
        KeyboardKey.H,
        KeyboardKey.I,
        KeyboardKey.J,
        KeyboardKey.K,
        KeyboardKey.L,
        KeyboardKey.M,
        KeyboardKey.N,
        KeyboardKey.O,
        KeyboardKey.P,
        KeyboardKey.Q,
        KeyboardKey.R,
        KeyboardKey.S,
        KeyboardKey.T,
        KeyboardKey.U,
        KeyboardKey.V,
        KeyboardKey.W,
        KeyboardKey.X,
        KeyboardKey.Y,
        KeyboardKey.Z,
        KeyboardKey.LeftBracket,
        KeyboardKey.Backslash,
        KeyboardKey.RightBracket,
        KeyboardKey.Grave,
        KeyboardKey.Escape,
        KeyboardKey.Enter,
        KeyboardKey.Backspace,
        KeyboardKey.Insert,
        KeyboardKey.Delete,
        KeyboardKey.Right,
        KeyboardKey.Left,
        KeyboardKey.Down,
        KeyboardKey.Up,
        KeyboardKey.PageUp,
        KeyboardKey.PageDown,
        KeyboardKey.Home,
        KeyboardKey.End,
        KeyboardKey.LeftShift,
        KeyboardKey.LeftControl,
        KeyboardKey.LeftAlt,
        KeyboardKey.RightShift,
        KeyboardKey.RightControl,
        KeyboardKey.RightAlt
    ];

    private readonly HashSet<KeyboardKey> downKeys = [];

    public Widget? Root { get; private set; }

    public Widget? Hovered { get; internal set; }
    public Widget? Focused { get; internal set; }
    public Widget? Pressed { get; internal set; }
    public Widget? Captured { get; internal set; }

    public Vector2 ScreenSize;
    public Vector2 MousePosition { get; private set; }
    public Vector2 MouseDelta { get; private set; }

    public void SetRoot(Widget? widget)
    {
        Hovered = null;
        Focus(null);
        Pressed = null;
        Captured = null;

        if (Root is not null)
        {
            Root.Parent = null;
            Root.SetGui(null);
        }

        Root = widget;

        if (Root is not null)
        {
            Root.Parent = null;
            Root.SetGui(this);
        }
    }

    public void Update()
    {
        ScreenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        MousePosition = Raylib.GetMousePosition();
        MouseDelta = Raylib.GetMouseDelta();

        Root?.Update();
        UpdateMouse();
        UpdateKeyboard();
    }

    public void Draw()
    {
        if (Root?.Visible == true)
            Root.Draw();
    }

    public void Focus(Widget? widget)
    {
        if (widget is not null && (!widget.Visible || !widget.Enabled || !widget.Focusable || widget.Gui != this))
            return;

        if (Focused == widget)
            return;

        Widget? previous = Focused;
        Focused = widget;

        previous?.DispatchBlur();
        Focused?.DispatchFocus();
    }

    public void CaptureMouse(Widget? widget)
    {
        if (widget is not null && (!widget.Visible || !widget.Enabled || widget.Gui != this))
            return;

        Captured = widget;
    }

    private void UpdateMouse()
    {
        Widget? hit = Root?.HitTest(MousePosition);

        if (Hovered != hit)
        {
            Hovered?.DispatchMouseLeave(MousePosition);
            Hovered = hit;
            Hovered?.DispatchMouseEnter(MousePosition);
        }

        Widget? mouseTarget = Captured ?? Hovered;

        if (MouseDelta != Vector2.Zero)
            mouseTarget?.DispatchMouseMove(MousePosition, MouseDelta);

        Vector2 wheel = Raylib.GetMouseWheelMoveV();

        if (wheel != Vector2.Zero)
            mouseTarget?.DispatchMouseWheel(MousePosition, wheel);

        foreach (MouseButton button in Enum.GetValues<MouseButton>())
        {
            if (Raylib.IsMouseButtonPressed(button))
            {
                Pressed = mouseTarget;
                mouseTarget?.DispatchMouseDown(button, MousePosition);

                if (mouseTarget?.Focusable == true)
                    Focus(mouseTarget);
            }

            if (Raylib.IsMouseButtonReleased(button))
            {
                Widget? releaseTarget = Captured ?? Pressed ?? mouseTarget;
                releaseTarget?.DispatchMouseUp(button, MousePosition);

                if (Pressed is not null && Pressed == releaseTarget && Pressed == hit)
                    Pressed.DispatchClick(button, MousePosition);

                Pressed = null;
            }
        }
    }

    private void UpdateKeyboard()
    {
        Widget? target = Focused;

        if (target is not null && (!target.Visible || !target.Enabled))
        {
            Focus(null);
            target = null;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Tab))
        {
            FocusNext(Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift));
            target = Focused;
        }

        foreach (KeyboardKey key in PolledKeys)
        {
            bool isDown = Raylib.IsKeyDown(key);

            if (isDown && downKeys.Add(key))
                target?.DispatchKeyDown(key);

            if (!isDown && downKeys.Remove(key))
                target?.DispatchKeyUp(key);
        }

        int codepoint;
        while ((codepoint = Raylib.GetCharPressed()) != 0)
            target?.DispatchTextInput(codepoint);
    }

    private void FocusNext(bool backwards)
    {
        if (Root is null)
            return;

        var widgets = new List<Widget>();
        Root.CollectFocusable(widgets);

        if (widgets.Count == 0)
            return;

        int index = Focused is null ? -1 : widgets.IndexOf(Focused);
        int next = backwards
            ? (index <= 0 ? widgets.Count - 1 : index - 1)
            : (index < 0 || index >= widgets.Count - 1 ? 0 : index + 1);

        Focus(widgets[next]);
    }
}
