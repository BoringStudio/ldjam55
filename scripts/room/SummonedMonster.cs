using Godot;

namespace ldjam55.scripts.room;

public partial class SummonedMonster : Node3D
{
    public resources.Monster Monster;

    private AnimationPlayer _animationPlayer;
    private Sprite3D _sprite;

    [Signal]
    public delegate void AnimationFinishedEventHandler();

    public override void _Ready()
    {
        if (Monster != null)
        {
            _sprite = GetNode<Sprite3D>("sprite");
            _sprite.Texture = Monster.RealImage;
        }

        _animationPlayer = GetNode<AnimationPlayer>("animation");
        _animationPlayer.Play("summon");

        _animationPlayer.AnimationFinished += OnAnimationFinished;
    }

    private void OnAnimationFinished(StringName name)
    {
        EmitSignal(SignalName.AnimationFinished);
    }
}
