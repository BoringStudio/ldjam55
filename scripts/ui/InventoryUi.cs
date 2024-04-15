using Godot;

namespace ldjam55.scripts.ui;

public partial class InventoryUi : HFlowContainer
{
    [Export] public Inventory Inventory;

    [Export] public PackedScene InventoryItemScene;

    public override void _Ready()
    {
        Inventory.ItemsChanged += OnInventoryChanged;
        ResetLists();
    }

    public override void _Process(double delta)
    {
    }

    private void OnInventoryChanged()
    {
        ResetLists();
    }

    private void ResetLists()
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }

        foreach (var instance in Inventory.Items)
        {
            var item = (InventoryItem)InventoryItemScene.Instantiate();
            item.Inventory = Inventory;
            item.Item = instance.Item;
            item.Count = 1;
            AddChild(item);
        }
    }
}
