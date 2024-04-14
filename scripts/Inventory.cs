using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace ldjam55.scripts;

public partial class Inventory : Node
{
    [Export] public int Balance;
    [Export] public Array<resources.Item> InitialItems;

    public List<resources.ItemInstance> Items = new();

    [Signal]
    public delegate void ItemsChangedEventHandler();

    public override void _Ready()
    {
        foreach (var item in InitialItems)
        {
            var index = -1;
            for (var i = 0; i < Items.Count; i++)
            {
                if (Items[i].Item.GlobalId != item.GlobalId) continue;
                index = i;
                break;
            }

            if (index >= 0)
            {
                Items[index].Count += 1;
            }
            else
            {
                Items.Add(new resources.ItemInstance
                {
                    Count = 1,
                    Item = item
                });
            }
        }
    }

    public override void _Process(double delta)
    {
    }
}
