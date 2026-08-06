#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public static class Cursor
{
    public static int Left
    {
        get => Console.CursorLeft;
        set => Console.CursorLeft = value;
    }

    public static int Top
    {
        get => Console.CursorTop;
        set => Console.CursorTop = value;
    }

    public static (int left, int top) Position
    {
        get => Console.GetCursorPosition();
        set => Console.SetCursorPosition(value.left, value.top);
    }

    public static bool Visible
    {
        get => OperatingSystem.IsWindows() && Console.CursorVisible;
        set => Console.CursorVisible = value;
    }

    public static void MoveLeft(int amount = 1) => Left -= amount;
    public static void MoveRight(int amount = 1) => Left += amount;
    public static void MoveUp(int amount = 1) => Top -= amount;
    public static void MoveDown(int amount = 1) => Top += amount;

    public static void SetPosition(int left, int top) => Position = (left, top);

    public static void Hide() => Visible = false;
    public static void Show() => Visible = true;
}
