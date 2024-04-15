using System.Diagnostics;
using Godot;
using Godot.Collections;
using ldjam55.scripts.room;

namespace ldjam55.scripts;

public partial class Game : Node
{
    [Export] public Room Room;
    [Export] public Inventory Inventory;

    [Export] public Array<resources.Monster> AllMonsters;

    public override void _Ready()
    {
        Debug.Assert(Inventory != null);
        Room.Game = this;
    }

    public override void _Process(double delta)
    {
    }

    public override void _Notification(int what)
    {
        if (what != NotificationDragEnd) return;
        Room.PlaceCurrentObject();
    }
}
