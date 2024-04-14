using System.Diagnostics;
using Godot;

namespace ldjam55.scripts.ui;

public partial class InventoryItem : Button
{
    public Inventory Inventory;
    public resources.Item Item;
    public int Count;

    private TextureRect _textureRect;

    public override void _Ready()
    {
        _textureRect = GetNode<TextureRect>("icon");
        Debug.Assert(_textureRect != null);

        SetItem(Item, Count);
    }

    public override void _Process(double delta)
    {
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        var preview = ItemDrag.MakeItemPreview(Item, Size);
        SetDragPreview(preview);

        return new ItemDrag(this, preview, Item);
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return data.Obj is ItemDrag { Source: InventoryItem };
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var obj = data.Obj;
        if (obj is not ItemDrag { Source: InventoryItem source }) return;

        var prevItem = Item;
        var prevCount = Count;
        SetItem(source.Item, source.Count);
        source.SetItem(prevItem, prevCount);
    }

    private void SetItem(resources.Item item, int count)
    {
        Item = item;
        Count = count;

        _textureRect.Texture = item.Texture;
    }
}
