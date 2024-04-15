using Godot;
using Godot.Collections;

namespace ldjam55.scripts.room;

public partial class RoomItem : Node3D
{
    [Export] public Node3D RotationTarget;
    [Export] public float AngleSnap;

    public resources.Item Item;
    public Array<CrystalRay> Rays = new();

    private float _angle;

    public override void _Ready()
    {
        switch (RotationTarget)
        {
            case null:
                return;
            case CrystalRay ray:
                Rays.Add(ray);
                break;
            default:
            {
                foreach (var child in RotationTarget.GetChildren())
                    if (child is CrystalRay ray)
                        Rays.Add(ray);
                break;
            }
        }
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
