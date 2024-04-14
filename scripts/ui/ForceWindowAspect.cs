using Godot;

namespace ldjam55.scripts.ui;

public partial class ForceWindowAspect : Node
{
    [Export] public Vector2I MinWindowSize { get; set; } = new(800, 600);
    [Export] public bool ForceAspectRatio;
    [Export] public double MinAspectRatio = 1.4;
    [Export] public double MaxAspectRatio = 1.8;

    private Vector2I _prevWindowSize;

    public override void _Ready()
    {
        DisplayServer.WindowSetMinSize(MinWindowSize);
        _prevWindowSize = DisplayServer.WindowGetSize();
    }

    public override void _Process(double delta)
    {
        if (!ForceAspectRatio)
        {
            return;
        }

        var curWindowSize = DisplayServer.WindowGetSize();
        var mode = DisplayServer.WindowGetMode();
        if (curWindowSize == _prevWindowSize || curWindowSize.X == 0 || curWindowSize.Y == 0 ||
            mode == DisplayServer.WindowMode.Fullscreen ||
            mode == DisplayServer.WindowMode.ExclusiveFullscreen ||
            mode == DisplayServer.WindowMode.Maximized)
        {
            return;
        }

        var aspectRatio = curWindowSize.X / (double)curWindowSize.Y;
        var changed = false;
        if (aspectRatio < MinAspectRatio)
        {
            aspectRatio = MinAspectRatio;
            changed = true;
        }
        else if (aspectRatio > MaxAspectRatio)
        {
            aspectRatio = MaxAspectRatio;
            changed = true;
        }

        if (changed)
        {
            var nextHeight = curWindowSize.X / aspectRatio;
            curWindowSize.Y = (int)nextHeight;
            DisplayServer.WindowSetSize(curWindowSize);
        }

        _prevWindowSize = curWindowSize;
    }
}
