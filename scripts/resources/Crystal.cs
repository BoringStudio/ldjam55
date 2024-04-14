using Godot;

namespace ldjam55.scripts.resources;

[GlobalClass]
public partial class Crystal : Item
{
    [Export] public CrystalDirections Directions;
}

public enum CrystalDirections
{
    X,
    Y,
    I,
    O,
}
