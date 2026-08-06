using System.Numerics;
using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public static partial class Extensions
{
    extension (Rectangle self)
    {
        public bool IsEmpty() => self.Width <= 0 || self.Height <= 0;

        public bool Contains(Vector2 point) => Raylib.CheckCollisionPointRec(point, self);
        public bool Intersects(Rectangle other) => !self.Intersect(other).IsEmpty();

        public Rectangle Intersect(Rectangle other)
        {
            float left = MathF.Max(self.X, other.X);
            float top = MathF.Max(self.Y, other.Y);
            float right = MathF.Min(self.X + self.Width, other.X + other.Width);
            float bottom = MathF.Min(self.Y + self.Height, other.Y + other.Height);

            return new Rectangle(
                left,
                top,
                MathF.Max(0, right - left),
                MathF.Max(0, bottom - top));
        }

        public Rectangle Union(Rectangle other)
        {
            float left = MathF.Min(self.X, other.X);
            float top = MathF.Min(self.Y, other.Y);
            float right = MathF.Max(self.X + self.Width, other.X + self.Width);
            float bottom = MathF.Max(self.Y + self.Height, other.Y + self.Height);

            return new Rectangle(
                left, 
                top, 
                right - left,
                bottom - top);
        }
    }
}
