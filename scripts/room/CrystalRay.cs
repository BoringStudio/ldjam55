using Godot;

namespace ldjam55.scripts.room;

public partial class CrystalRay : Node3D
{
    [Export] public StaticBody3D Crystal;
    [Export] public float RaySpeed = 0.2f;

    private Sprite3D _sprite;
    private ShaderMaterial _spriteMaterial;

    private float _spriteLengthSquared;

    private Area3D _area;

    private Node3D _closestNode;
    private float _shift;

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
        if (_closestNode != null)
        {
            var distanceSquared = _closestNode.GlobalPosition.DistanceSquaredTo(GlobalPosition);
            spriteScale = Mathf.Clamp(distanceSquared / _spriteLengthSquared, 0.0f, 1.0f);

            _shift = (float)(_shift + delta * RaySpeed) % 1.0f;
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

        _closestNode = closest;
    }
}
