using Godot;

namespace ldjam55.scripts.ui;

public partial class ItemDrag : Node
{
    [Signal]
    public delegate void DragCompletedEventHandler(resources.Item item);

    public Control Source { get; private set; }
    public Control Preview { get; private set; }
    public resources.Item Item { get; private set; }

    public static Control MakeItemPreview(resources.Item item, Vector2 size)
    {
        var node = new Control();

        var textureRect = new TextureRect();
        textureRect.Texture = item.Texture;
        textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        textureRect.StretchMode = TextureRect.StretchModeEnum.Scale;
        textureRect.Size = size;
        textureRect.Position -= size / 2;
        node.AddChild(textureRect);

        return node;
    }

    public ItemDrag()
    {
    }

    public ItemDrag(Control source, Control preview, resources.Item item)
    {
        Source = source;
        Preview = preview;
        Item = item;

        preview.TreeExiting += OnTreeExiting;
    }


    private void OnTreeExiting()
    {
        EmitSignal(SignalName.DragCompleted, Item);
    }
}
