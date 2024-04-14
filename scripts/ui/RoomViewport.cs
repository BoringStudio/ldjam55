using Godot;

namespace ldjam55.scripts.ui;

public partial class RoomViewport : Control
{
    [Export] public Node3D Viewport;

    public override void _UnhandledInput(InputEvent @event)
    {
        Viewport._UnhandledInput(@event);
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return true;
    }
}
