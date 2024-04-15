using Godot;
using Godot.Collections;

namespace ldjam55.scripts.resources;

[GlobalClass]
public partial class Monster : Resource
{
    [Export] public int MonsterId;
    [Export] public string Name;
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] public Texture2D BookImage;
    [Export] public Texture2D RealImage;
    [Export] public Array<Vector2I> Diagram;
}
