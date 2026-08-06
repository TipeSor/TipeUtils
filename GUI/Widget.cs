using System.Numerics;
using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public abstract class Widget
{
    public Widget? Parent { get; internal set; }
    public Gui? Gui { get; internal set; }

    public Vector2 Anchor;       // normalize 0..1
    public Vector2 AnchorOffset; // pixels

    public Vector2 Origin;       // normalize 0..1
    public Vector2 OriginOffset; // pixels

    public Rectangle BoundingBox; // widgets bounding box used for hit tests
    public Vector2 PreferredSize;
    public Vector2 MinimumSize;
    public float LayoutWeight;

    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool Focusable { get; set; }

    public bool IsHovered => Gui?.Hovered == this;
    public bool IsFocused => Gui?.Focused == this;
    public bool IsPressed => Gui?.Pressed == this;
    public bool HasMouseCapture => Gui?.Captured == this;

    public virtual void Update() { }
    public virtual void Draw() { }

    public event EventHandler<GuiMouseEventArgs>? OnMouseEntered;
    public event EventHandler<GuiMouseEventArgs>? OnMouseLeft;
    public event EventHandler<GuiMouseEventArgs>? OnMouseMoved;
    public event EventHandler<GuiMouseEventArgs>? OnMouseDown;
    public event EventHandler<GuiMouseEventArgs>? OnMouseUp;
    public event EventHandler<GuiMouseEventArgs>? OnClicked;
    public event EventHandler<GuiMouseEventArgs>? Clicked;
    public event EventHandler<GuiMouseEventArgs>? OnMouseWheel;

    public event EventHandler? OnFocused;
    public event EventHandler? OnBlurred;
    public event EventHandler<GuiKeyEventArgs>? OnKeyDown;
    public event EventHandler<GuiKeyEventArgs>? OnKeyUp;
    public event EventHandler<GuiTextInputEventArgs>? OnTextInput;

    protected virtual void HandleMouseEnter(GuiMouseEventArgs args) { }
    protected virtual void HandleMouseLeave(GuiMouseEventArgs args) { }
    protected virtual void HandleMouseMove(GuiMouseEventArgs args) { }
    protected virtual void HandleMouseDown(GuiMouseEventArgs args) { }
    protected virtual void HandleMouseUp(GuiMouseEventArgs args) { }
    protected virtual void HandleClick(GuiMouseEventArgs args) { }
    protected virtual void HandleMouseWheel(GuiMouseEventArgs args) { }

    protected virtual void HandleFocus() { }
    protected virtual void HandleBlur() { }
    protected virtual void HandleKeyDown(GuiKeyEventArgs args) { }
    protected virtual void HandleKeyUp(GuiKeyEventArgs args) { }
    protected virtual void HandleTextInput(GuiTextInputEventArgs args) { }

    public bool ContainsPoint(Vector2 point) => BoundingBox.Contains(point);

    public void Focus() => Gui?.Focus(this);
    public void CaptureMouse() => Gui?.CaptureMouse(this);

    public void ReleaseMouse()
    {
        if (Gui?.Captured == this)
            Gui.CaptureMouse(null);
    }

    internal virtual Widget? HitTest(Vector2 point)
    {
        if (!Visible || !Enabled)
            return null;

        return ContainsPoint(point) ? this : null;
    }

    internal virtual void SetGui(Gui? gui)
    {
        Gui = gui;
    }

    internal virtual void CollectFocusable(List<Widget> widgets)
    {
        if (Visible && Enabled && Focusable)
            widgets.Add(this);
    }

    internal void DispatchMouseEnter(Vector2 position)
    {
        var args = new GuiMouseEventArgs(position);
        HandleMouseEnter(args);
        OnMouseEntered?.Invoke(this, args);
    }

    internal void DispatchMouseLeave(Vector2 position)
    {
        var args = new GuiMouseEventArgs(position);
        HandleMouseLeave(args);
        OnMouseLeft?.Invoke(this, args);
    }

    internal void DispatchMouseMove(Vector2 position, Vector2 delta)
    {
        var args = new GuiMouseEventArgs(position, delta: delta);
        HandleMouseMove(args);
        OnMouseMoved?.Invoke(this, args);
    }

    internal void DispatchMouseDown(MouseButton button, Vector2 position)
    {
        var args = new GuiMouseEventArgs(position, button: button);
        HandleMouseDown(args);
        OnMouseDown?.Invoke(this, args);
    }

    internal void DispatchMouseUp(MouseButton button, Vector2 position)
    {
        var args = new GuiMouseEventArgs(position, button: button);
        HandleMouseUp(args);
        OnMouseUp?.Invoke(this, args);
    }

    internal void DispatchClick(MouseButton button, Vector2 position)
    {
        var args = new GuiMouseEventArgs(position, button: button);
        HandleClick(args);
        OnClicked?.Invoke(this, args);
        Clicked?.Invoke(this, args);
    }

    internal void DispatchMouseWheel(Vector2 position, Vector2 wheel)
    {
        var args = new GuiMouseEventArgs(position, wheel: wheel);
        HandleMouseWheel(args);
        OnMouseWheel?.Invoke(this, args);
    }

    internal void DispatchFocus()
    {
        HandleFocus();
        OnFocused?.Invoke(this, EventArgs.Empty);
    }

    internal void DispatchBlur()
    {
        HandleBlur();
        OnBlurred?.Invoke(this, EventArgs.Empty);
    }

    internal void DispatchKeyDown(KeyboardKey key)
    {
        var args = new GuiKeyEventArgs(key);
        HandleKeyDown(args);
        OnKeyDown?.Invoke(this, args);
    }

    internal void DispatchKeyUp(KeyboardKey key)
    {
        var args = new GuiKeyEventArgs(key);
        HandleKeyUp(args);
        OnKeyUp?.Invoke(this, args);
    }

    internal void DispatchTextInput(int codepoint)
    {
        var args = new GuiTextInputEventArgs(codepoint);
        HandleTextInput(args);
        OnTextInput?.Invoke(this, args);
    }
}
