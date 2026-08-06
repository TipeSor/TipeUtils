using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public static partial class Rl
{
    public static DrawContext BeginDrawing() => new DrawContext();
    public static DrawContext BeginDrawing(Color clearColor) => new DrawContext(clearColor);
    public static Mode2DContext BeginMode2D(Camera2D camera) => new Mode2DContext(camera);
    public static Mode3DContext BeginMode3D(Camera3D camera) => new Mode3DContext(camera);
    public static TextureModeContext BeginTextureMode(RenderTexture2D target) => new TextureModeContext(target);
    public static ScissorModeContext BeginScissorMode(Rectangle rect, bool ignoreParent = false) => new ScissorModeContext(rect, ignoreParent);
    public static ShaderModeContext BeginShaderMode(Shader shader) => new ShaderModeContext(shader);
    public static BlendModeContext BeginBlendMode(BlendMode mode) => new BlendModeContext(mode);
    public static VrStereoModeContext BeginVrStereoMode(VrStereoConfig config) => new VrStereoModeContext(config);

    private static readonly Stack<Rectangle> _scissorStack = new();

    public ref struct DrawContext : IDisposable
    {
        public DrawContext() => Raylib.BeginDrawing();

        public DrawContext(Color clearColor)
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(clearColor);
        }

        public readonly void Dispose() => Raylib.EndDrawing();
    }

    public ref struct Mode2DContext : IDisposable
    {
        public Mode2DContext(Camera2D camera) => Raylib.BeginMode2D(camera);
        public readonly void Dispose() => Raylib.EndMode2D();
    }

    public ref struct Mode3DContext : IDisposable
    {
        public Mode3DContext(Camera3D camera) => Raylib.BeginMode3D(camera);
        public readonly void Dispose() => Raylib.EndMode3D();
    }

    public ref struct TextureModeContext : IDisposable
    {
        public TextureModeContext(RenderTexture2D target) => Raylib.BeginTextureMode(target);
        public readonly void Dispose() => Raylib.EndTextureMode();
    }

    public ref struct ScissorModeContext : IDisposable
    {
        public ScissorModeContext(Rectangle rect, bool ignoreParent)
        {
            Rectangle clip = rect;

            if (_scissorStack.Count > 0 && !ignoreParent)
            {
                clip = _scissorStack.Peek().Intersect(rect);
            }

            _scissorStack.Push(clip);

            Raylib.BeginScissorMode((int)clip.X, (int)clip.Y, (int)clip.Width, (int)clip.Height);
        }

        public readonly void Dispose()
        {
            Raylib.EndScissorMode();

            _scissorStack.Pop();

            if (_scissorStack.Count > 0)
            {
                Rectangle clip = _scissorStack.Peek();
                Raylib.BeginScissorMode((int)clip.X, (int)clip.Y, (int)clip.Width, (int)clip.Height);
            }
        }
    }

    public ref struct ShaderModeContext : IDisposable
    {
        public ShaderModeContext(Shader shader) => Raylib.BeginShaderMode(shader);
        public readonly void Dispose() => Raylib.EndShaderMode();
    }

    public ref struct BlendModeContext : IDisposable
    {
        public BlendModeContext(BlendMode mode) => Raylib.BeginBlendMode(mode);
        public readonly void Dispose() => Raylib.EndBlendMode();
    }

    public ref struct VrStereoModeContext : IDisposable
    {
        public VrStereoModeContext(VrStereoConfig config) => Raylib.BeginVrStereoMode(config);
        public readonly void Dispose() => Raylib.EndVrStereoMode();
    }
}
