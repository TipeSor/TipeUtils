using System.Numerics;
using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public class Container : Widget
{
    private readonly List<Widget> children = [];

    public IReadOnlyList<Widget> Children => children;
    public bool ClipChildren { get; set; }

    public Widget Add(Widget child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (child.Parent is not null)
            throw new InvalidOperationException("Widget already has a parent.");

        children.Add(child);
        child.Parent = this;
        child.SetGui(Gui);
        return child;
    }

    public bool Remove(Widget child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (!children.Remove(child))
            return false;

        child.Parent = null;
        child.SetGui(null);
        return true;
    }

    public void Clear()
    {
        foreach (Widget child in children)
        {
            child.Parent = null;
            child.SetGui(null);
        }

        children.Clear();
    }

    public override void Update()
    {
        LayoutChildren();

        foreach (Widget child in children)
        {
            if (child.Visible && child.Enabled)
                child.Update();
        }
    }

    public override void Draw()
    {
        if (ClipChildren)
        {
            using (Rl.BeginScissorMode(BoundingBox))
                DrawChildren();

            return;
        }

        DrawChildren();
    }

    protected virtual void LayoutChildren() { }

    protected void LayoutLinearChildren(Rectangle content, LayoutOrientation orientation, float spacing)
    {
        bool vertical = orientation == LayoutOrientation.Vertical;
        int visibleCount = 0;
        float fixedLength = 0;
        float totalWeight = 0;

        foreach (Widget child in children)
        {
            if (!child.Visible)
                continue;

            visibleCount++;

            if (child.LayoutWeight > 0)
            {
                totalWeight += child.LayoutWeight;
                fixedLength += vertical ? child.MinimumSize.Y : child.MinimumSize.X;
                continue;
            }

            fixedLength += ResolvePreferredLength(child, vertical);
        }

        float totalSpacing = MathF.Max(0, visibleCount - 1) * spacing;
        float availableLength = vertical ? content.Height : content.Width;
        float remaining = MathF.Max(0, availableLength - fixedLength - totalSpacing);
        float offset = vertical ? content.Y : content.X;

        foreach (Widget child in children)
        {
            if (!child.Visible)
                continue;

            float length = child.LayoutWeight > 0 && totalWeight > 0
                ? (vertical ? child.MinimumSize.Y : child.MinimumSize.X) + remaining * child.LayoutWeight / totalWeight
                : ResolvePreferredLength(child, vertical);

            if (vertical)
            {
                child.BoundingBox = new Rectangle(content.X, offset, content.Width, length);
                offset += length + spacing;
                continue;
            }

            child.BoundingBox = new Rectangle(offset, content.Y, length, content.Height);
            offset += length + spacing;
        }
    }

    private static float ResolvePreferredLength(Widget child, bool vertical)
    {
        float preferred = vertical ? child.PreferredSize.Y : child.PreferredSize.X;

        if (preferred > 0)
            return preferred;

        return vertical ? child.MinimumSize.Y : child.MinimumSize.X;
    }

    protected Rectangle ContentBounds(Thickness padding)
    {
        return new Rectangle(
            BoundingBox.X + padding.Left,
            BoundingBox.Y + padding.Top,
            MathF.Max(0, BoundingBox.Width - padding.Horizontal),
            MathF.Max(0, BoundingBox.Height - padding.Vertical));
    }

    protected void DrawChildren()
    {
        foreach (Widget child in children)
        {
            if (child.Visible)
                child.Draw();
        }
    }

    internal override Widget? HitTest(Vector2 point)
    {
        if (!Visible || !Enabled)
            return null;

        for (int i = children.Count - 1; i >= 0; i--)
        {
            Widget? hit = children[i].HitTest(point);

            if (hit is not null)
                return hit;
        }

        return ContainsPoint(point) ? this : null;
    }

    internal override void SetGui(Gui? gui)
    {
        base.SetGui(gui);

        foreach (Widget child in children)
            child.SetGui(gui);
    }

    internal override void CollectFocusable(List<Widget> widgets)
    {
        base.CollectFocusable(widgets);

        foreach (Widget child in children)
            child.CollectFocusable(widgets);
    }
}
