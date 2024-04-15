using System.Collections.Generic;
using Godot;

namespace ldjam55.scripts.room;

public partial class ZooMap : Node3D
{
    private readonly Dictionary<int, ZooMapItem> _items = new();

    public override void _Ready()
    {
        foreach (var child in GetChildren())
        {
            if (child is not ZooMapItem item) continue;
            _items.Add(item.Monster.MonsterId, item);
            item.Visible = false;
        }
    }

    public void ShowMonster(resources.Monster monster)
    {
        if (_items.TryGetValue(monster.MonsterId, out var item))
            item.Visible = true;
    }
}
