#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public static partial class ANSI
{
    /// <summary>
    /// Bell 
    /// </summary>
    public const string BEL = "\a";

    /// <summary>
    /// Backspace
    /// </summary>
    public const string BS = "\b";

    /// <summary>
    /// Tab
    /// </summary>
    public const string HT = "\t";

    /// <summary>
    /// Line Feed
    /// </summary>
    public const string LF = "\n";

    /// <summary>
    /// Form Feed
    /// </summary>
    public const string FF = "\f";

    /// <summary>
    /// Carriage Return 
    /// </summary>
    public const string CR = "\r";

    /// <summary>
    /// Escape 
    /// </summary>
    public const string ESC = "\u001b";
}

public static partial class ANSI
{
    /// <summary>
    /// Single Shift Two
    /// </summary>
    public const string SS2 = ESC + "N";

    /// <summary>
    /// Single Shift Three
    /// </summary>
    public const string SS3 = ESC + "O";

    /// <summary>
    /// Device Control String
    /// </summary>
    public const string DCS = ESC + "P";

    /// <summary>
    /// Control Sequence Introducer
    /// </summary>
    public const string CSI = ESC + "[";

    /// <summary>
    /// String Terminator
    /// </summary>
    public const string ST = ESC + "\\";

    /// <summary>
    /// Operating System Command
    /// </summary>
    public const string OSC = ESC + "]";

    /// <summary>
    /// Start of String
    /// </summary>
    public const string SOS = ESC + "X";

    /// <summary>
    /// Privacy Message
    /// </summary>
    public const string PM = ESC + "^";

    /// <summary>
    /// Application Program Command
    /// </summary>
    public const string APC = ESC + "_";
}

public static partial class ANSI
{
    /// <summary>
    /// Cursor Up
    /// </summary>
    public static string CUU(int n) => CSI + n + "A";

    /// <summary>
    /// Cursor Down
    /// </summary>
    public static string CUD(int n) => CSI + n + "B";

    /// <summary>
	/// Cursor Forward
	/// </summary>
    public static string CUF(int n) => CSI + n + "C";

    /// <summary>
	/// Cursor Back
	/// </summary>
    public static string CUB(int n) => CSI + n + "D";

    /// <summary>
	/// Cursor Next Line
	/// </summary>
    public static string CNL(int n) => CSI + n + "E";

    /// <summary>
	/// Cursor Previous Line
	/// </summary>
    public static string CPL(int n) => CSI + n + "F";

    /// <summary>
	/// Cursor Horizontal Absolute
	/// </summary>
    public static string CHA(int n) => CSI + n + "G";

    /// <summary>
	/// Cursor Position
	/// </summary>
    public static string CUP(int n, int m) => CSI + n + ";" + m + "H";

    /// <summary>
	/// Erase in Display
	/// </summary>
    public static string ED(int n) => CSI + n + "J";

    /// <summary>
	/// Erase in Line
	/// </summary>
    public static string EL(int n) => CSI + n + "K";

    /// <summary>
	/// Scroll Up
	/// </summary>
    public static string SU(int n) => CSI + n + "S";

    /// <summary>
	/// Scroll Down
	/// </summary>
    public static string SD(int n) => CSI + n + "T";

    /// <summary>
	/// Horizontal Vertical Position
	/// </summary>
    public static string HVP(int n, int m) => CSI + n + ";" + m + "f";

    /// <summary>
	/// Select Graphic Rendition
	/// </summary>
    public static string SGR(int n) => CSI + n + "m";

    /// <summary>
	/// Device Status Report
	/// </summary>
    public const string DSR = CSI + "6n";
}

public static partial class ANSI
{
    /// <summary>
	/// Save Current Cursor Position
	/// </summary>
    public const string SCP = CSI + "s";

    /// <summary>
	/// Restore Saved Cursor Position
	/// </summary>
    public const string RCO = CSI + "u";

    /// <summary>
	/// Shows The Cursor
	/// </summary>
    public const string CUS = CSI + "?25h";

    /// <summary>
	/// Hides The Cursor
	/// </summary>
    public const string CUH = CSI + "?25l";

    /// <summary>
	/// Enable alternative screen buffer
	/// </summary>
    public const string EAB = CSI + "?1049h";

    /// <summary>
	/// Disable alternative screen buffer
	/// </summary>
    public const string DAB = CSI + "?1049l";
}

public static partial class ANSI
{
    /// <summary>
	/// Clear to end of screen
	/// </summary>
    public const string ED0 = CSI + "0J";

    /// <summary>
    /// Clear to start of screen
    /// </summary>
    public const string ED1 = CSI + "1J";

    /// <summary>
    /// Clear screen
    /// </summary>
    public const string ED2 = CSI + "2J";

    /// <summary>
	/// Clear to end of line
    /// /// </summary>
    public const string EL0 = CSI + "0K";

    /// <summary>
    /// Clear to start of line
    /// </summary>
    public const string EL1 = CSI + "1K";

    /// <summary>
    /// Clear line
    /// </summary>
    public const string EL2 = CSI + "2K";
}

public static partial class ANSI
{
    /// <summary>
    /// Set foreground color
    /// </summary>
    public static string FG(AnsiColor color)
        => CSI + (30 + (int)color) + "m";

    /// <summary>
    /// Set background color
    /// </summary>
    public static string BG(AnsiColor color)
        => CSI + (40 + (int)color) + "m";
}

// TODO: port these
public static partial class ANSI
{
    public const string Reset = CSI + "0m";
    public const string Bold = CSI + "1m";
    public const string Faint = CSI + "2m";
    public const string Italic = CSI + "3m";
    public const string Underline = CSI + "4m";
    public const string SlowBlink = CSI + "5m";
    public const string RapidBlink = CSI + "6m";
    public const string Invert = CSI + "7m";
    public const string Hidden = CSI + "8m";
    public const string Strikethrough = CSI + "9m";

    public const string PrimaryFont = CSI + "10m";
    public const string GothicFont = CSI + "20m";

    public const string NormalIntensity = CSI + "22m";
    public const string NoItalic = CSI + "23m";
    public const string NoUnderline = CSI + "24m";
    public const string NoBlink = CSI + "25m";
    public const string NoInvert = CSI + "27m";
    public const string NoHidden = CSI + "28m";
    public const string NoStrikethrough = CSI + "29m";

    public const string DefaultForeground = CSI + "39m";
    public const string DefaultBackground = CSI + "49m";

    public static string Foreground(int rgb)
        => Foreground((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);

    public static string Foreground(byte r, byte g, byte b)
        => $"{CSI}38;2;{r};{g};{b}m";

    public static string Foreground256(int color)
        => $"{CSI}38;5;{color}m";

    public static string Background(int rgb)
        => Background((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);

    public static string Background(byte r, byte g, byte b)
        => $"{CSI}48;2;{r};{g};{b}m";

    public static string Background256(int color)
        => $"{CSI}48;5;{color}m";
}

public enum AnsiColor
{
    Black = 0,
    Red = 1,
    Green = 2,
    Yellow = 3,
    Blue = 4,
    Magenta = 5,
    Cyan = 6,
    White = 7,

	BrightBlack = 60,
	BrightRed = 61,
	BrightGreen = 62,
	BrightYellow = 63,
	BrightBlue = 64,
	BrightMagenta = 65,
	BrightCyan = 66,
	BrightWhite = 67,
}
