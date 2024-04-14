using Godot;

namespace ldjam55.scripts.ui;

public partial class InventoryUi : Control
{
    [Export] public Inventory Inventory;

    [Export] public PackedScene InventoryItemScene;

    private FlowContainer _itemsGrid;

    public override void _Ready()
    {
        _itemsGrid = GetNode<FlowContainer>("items_grid");

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
        foreach (var child in _itemsGrid.GetChildren())
        {
            _itemsGrid.RemoveChild(child);
            child.QueueFree();
        }

        foreach (var instance in Inventory.Items)
        {
            var item = (InventoryItem)InventoryItemScene.Instantiate();
            item.Inventory = Inventory;
            item.Item = instance.Item;
            item.Count = 1;
            _itemsGrid.AddChild(item);
        }
    }
}
