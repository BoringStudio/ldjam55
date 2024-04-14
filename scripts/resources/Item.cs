using Godot;

namespace ldjam55.scripts.resources;

[GlobalClass]
public partial class Item : Resource
{
    [Export] public int GlobalId;
    [Export] public string Name;
    [Export] public Texture2D Texture;
    [Export] public PackedScene RoomItem;
}

public class ItemInstance
{
    public Item Item;
    public int Count;
}
