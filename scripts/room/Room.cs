using Godot;
using ldjam55.scripts.ui;

namespace ldjam55.scripts.room;

public partial class Room : Node3D
{
    [Export] public Game Game;

    [Export] public Control RenderTarget;
    [Export] public Vector2 SummoningAreaSize = Vector2.One * 2.0f;

    public Node3D GrabbedObject;

    private Window _window;
    private Camera3D _camera;
    private ItemDrag _lastItemDrag;

    public override void _Ready()
    {
        _window = GetTree().Root;
        _camera = GetViewport().GetCamera3D();
    }

    public override void _Process(double delta)
    {
        var windowMousePosition = _window.GetMousePosition();
        var itemInViewport = RenderTarget.GetGlobalRect().HasPoint(windowMousePosition);

        DrawDragPreview(itemInViewport);

        if (!_window.GuiIsDragging())
        {
            TryGrabObject(windowMousePosition);
            TryReleaseObject();
        }

        if (GrabbedObject != null)
        {
            var relativeMousePosition = ConvertMousePosition(windowMousePosition);
            var from = _camera.ProjectRayOrigin(relativeMousePosition);
            var dir = _camera.ProjectRayNormal(relativeMousePosition);
            var globalPosition = Plane.PlaneXZ.IntersectsRay(from, dir);

            if (globalPosition == null)
            {
                GrabbedObject.Visible = false;
            }
            else
            {
                GrabbedObject.Visible = itemInViewport;
                GrabbedObject.GlobalPosition = ClampPositionToSummoningArea(globalPosition.Value);
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        var steps = 0;
        if (@event.IsActionPressed("rotate_right"))
        {
            steps = 1;
        }
        else if (@event.IsActionPressed("rotate_left"))
        {
            steps -= 1;
        }

        if (steps != 0 && GrabbedObject is RoomItem roomItem)
        {
            roomItem.RotateInner(steps);
        }
    }

    public void PlaceCurrentObject()
    {
        var itemInViewport = RenderTarget.GetGlobalRect().HasPoint(_window.GetMousePosition());
        if (itemInViewport)
        {
            GrabbedObject = null;
        }
    }

    private void TryGrabObject(Vector2 windowMousePosition)
    {
        if (GrabbedObject != null) return;

        if (Input.IsActionPressed("grab"))
        {
            var mousePosition = ConvertMousePosition(windowMousePosition);
            var state = GetWorld3D().DirectSpaceState;
            var from = _camera.ProjectRayOrigin(mousePosition);
            var to = from + _camera.ProjectRayNormal(mousePosition) * 20.0f;
            var query = PhysicsRayQueryParameters3D.Create(from, to);
            var result = state.IntersectRay(query);

            if (result.Count > 0 && result["collider"].Obj is Node3D node)
            {
                GrabbedObject = (Node3D)node.GetParent();
            }
        }
    }

    private void TryReleaseObject()
    {
        if (GrabbedObject == null || _lastItemDrag != null) return;

        if (!Input.IsActionPressed("grab"))
        {
            GrabbedObject = null;
        }
    }

    private void DrawDragPreview(bool itemInViewport)
    {
        var dragData = _window.GuiGetDragData();
        if (dragData.Obj is not ItemDrag itemDrag)
        {
            _lastItemDrag = null;
            return;
        }

        itemDrag.Preview.Visible = !itemInViewport;

        if (_lastItemDrag != itemDrag)
        {
            SetCurrentItemPreview(itemDrag.Item);
        }

        _lastItemDrag = itemDrag;
    }

    private Vector3 ClampPositionToSummoningArea(Vector3 position)
    {
        var half = SummoningAreaSize / 2;
        position.X = Mathf.Clamp(position.X, -half.X, half.X);
        position.Z = Mathf.Clamp(position.Z, -half.Y, half.Y);
        return position;
    }

    public Vector2 ConvertMousePosition(Vector2 mousePosition)
    {
        var viewportSize = GetViewport().GetVisibleRect().Size;

        float scale;
        var diff = Vector2.Zero;
        if (RenderTarget.Size.Y > RenderTarget.Size.X)
        {
            scale = viewportSize.Y / RenderTarget.Size.Y;
            diff.X = (viewportSize.X / scale - RenderTarget.Size.X) / 2;
        }
        else
        {
            scale = viewportSize.X / RenderTarget.Size.X;
            diff.Y = (viewportSize.Y / scale - RenderTarget.Size.Y) / 2;
        }

        mousePosition -= RenderTarget.GlobalPosition;
        mousePosition += diff;
        mousePosition *= new Vector2(scale, scale);

        return mousePosition;
    }

    private void SetCurrentItemPreview(resources.Item item)
    {
        RemoveCurrentItemPreview();
        if (item.RoomItem == null) return;

        GrabbedObject = (Node3D)item.RoomItem.Instantiate();
        AddChild(GrabbedObject);
    }

    private void RemoveCurrentItemPreview()
    {
        if (GrabbedObject == null) return;
        RemoveChild(GrabbedObject);
        GrabbedObject.QueueFree();
        GrabbedObject = null;
    }
}