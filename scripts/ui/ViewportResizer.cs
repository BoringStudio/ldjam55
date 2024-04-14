using Godot;

namespace ldjam55.scripts.ui;

public partial class ViewportResizer : Node
{
    [Export] public SubViewport TargetViewport;
    [Export] public TextureRect TargetTexture;

    private Window _window;
    private Vector2I _baseViewportSize;

    public override void _Ready()
    {
        _window = GetTree().Root;
        _window.SizeChanged += RootViewportSizeChanged;
        _baseViewportSize = TargetViewport.Size;
    }

    private void RootViewportSizeChanged()
    {
        var windowSize = _window.Size.Y;
        if (windowSize == 0)
        {
            return;
        }

        TargetViewport.Size = (Vector2I)(Vector2.One * windowSize);
        TargetTexture.Scale = new Vector2(1, -1) * _baseViewportSize.Y / windowSize;
    }
}
