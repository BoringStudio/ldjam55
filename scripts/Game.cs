using System.Collections.Generic;
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

    private readonly HashSet<int> _summonedMonsters = new();

    [Signal]
    public delegate void MonsterSummonedEventHandler(resources.Monster monster);

    public override void _Ready()
    {
        GetTree().Root.MinSize = new Vector2I(1280, 768);

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

    public void SummonMonster(resources.Monster monster)
    {
        if (!_summonedMonsters.Add(monster.MonsterId)) return;
        Inventory.AddItems(monster.GivesItems);
        EmitSignal(SignalName.MonsterSummoned, monster);
    }
}
