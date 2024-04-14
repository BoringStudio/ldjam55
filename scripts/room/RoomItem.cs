using Godot;

namespace ldjam55.scripts.room;

public partial class RoomItem : Node3D
{
    [Export] public Node3D RotationTarget;
    [Export] public float AngleSnap;

    private float _angle;

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
    }

    public void RotateInner(int steps)
    {
        if (RotationTarget == null) return;

        _angle = (_angle + steps * AngleSnap) % 360.0f;
        RotationTarget.RotationDegrees = new Vector3(0.0f, _angle, 0.0f);
    }
}
