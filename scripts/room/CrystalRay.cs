using Godot;

namespace ldjam55.scripts.room;

public partial class CrystalRay : Node3D
{
    [Export] public StaticBody3D Crystal;
    [Export] public float RaySpeed = 0.2f;

    public Node3D ClosestNode { get; private set; }

    private Sprite3D _sprite;
    private ShaderMaterial _spriteMaterial;

    private float _spriteLengthSquared;

    private Area3D _area;

    private float _shift;

    public int Angle
    {
        get
        {
            var globalRotation = GlobalRotation.Y;
            if (globalRotation < 0.0f && Mathf.Abs(globalRotation) + 0.0001f >= Mathf.Pi)
                globalRotation += 2 * Mathf.Pi;

            var degrees = Mathf.RoundToInt(Mathf.RadToDeg(globalRotation) / 15.0f) * 15;
            return degrees;
        }
    }

    public override void _Ready()
    {
        _sprite = GetNode<Sprite3D>("sprite");
        _spriteMaterial = (ShaderMaterial)_sprite.MaterialOverride;

        _spriteLengthSquared = _sprite.PixelSize * _sprite.Texture.GetHeight();
        _spriteLengthSquared *= _spriteLengthSquared;

        _spriteMaterial.SetShaderParameter("albedo_texture", _sprite.Texture);

        _area = GetNode<Area3D>("area");

        _area.BodyEntered += OnBodyEntered;
        _area.BodyExited += OnBodyExited;
    }

    public override void _Process(double delta)
    {
        var spriteScale = 1.0f;
        if (ClosestNode != null)
        {
            if (ClosestNode.IsInsideTree())
            {
                var distanceSquared = ClosestNode.GlobalPosition.DistanceSquaredTo(GlobalPosition);
                spriteScale = Mathf.Clamp(distanceSquared / _spriteLengthSquared, 0.0f, 1.0f);

                _shift = (float)(_shift + delta * RaySpeed) % 1.0f;
            }
            else
            {
                ClosestNode = null;
            }
        }

        _spriteMaterial.SetShaderParameter("ray_length", spriteScale);
        _spriteMaterial.SetShaderParameter("ray_shift", _shift);
    }

    public override void _PhysicsProcess(double delta)
    {
        _area.SetCollisionLayerValue(32, !_area.GetCollisionLayerValue(32));
    }

    private void OnBodyEntered(Node3D body)
    {
        ComputeClosestNode();
    }

    private void OnBodyExited(Node3D body)
    {
        ComputeClosestNode();
    }

    private void ComputeClosestNode()
    {
        var bodies = _area.GetOverlappingBodies();

        Node3D closest = null;
        var minDistance = Mathf.Inf;
        foreach (var body in bodies)
        {
            if (body == Crystal) continue;

            var distanceToOrigin = body.GlobalPosition.DistanceSquaredTo(GlobalPosition);
            if (distanceToOrigin >= minDistance) continue;

            minDistance = distanceToOrigin;
            closest = body;
        }

        ClosestNode = closest;
    }
}
