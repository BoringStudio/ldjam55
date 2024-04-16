using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using ldjam55.scripts.resources;
using ldjam55.scripts.ui;

namespace ldjam55.scripts.room;

public partial class Room : Node3D
{
    [Export] public Game Game;
    [Export] public PackedScene QuestItem;
    [Export] public PackedScene SummonedMonsterScene;
    [Export] public PackedScene SummonedNothingScene;

    [Export] public Control RenderTarget;
    [Export] public Vector2 SummoningAreaSize = Vector2.One * 2.0f;

    public Node3D GrabbedObject;
    public SummonedMonster SummonedMonster;

    private Window _window;
    private Camera3D _camera;
    private ItemDrag _lastItemDrag;
    private ZooMap _zooMap;
    private Node3D _summoningArea;

    public override void _Ready()
    {
        _window = GetTree().Root;
        _camera = GetViewport().GetCamera3D();
        _zooMap = (ZooMap)GetNode("map");
        _summoningArea = (Node3D)GetNode("area");

        Debug.Assert(Game != null);
        Game.MonsterSummoned += OnNewMonster;
    }

    public override void _Process(double delta)
    {
        var windowMousePosition = _window.GetMousePosition();
        var itemInViewport = RenderTarget.GetGlobalRect().HasPoint(windowMousePosition);

        DrawDragPreview(itemInViewport);

        if (!_window.GuiIsDragging())
        {
            TryGrabObject(windowMousePosition);
            TryReleaseObject(itemInViewport);
        }

        if (GrabbedObject != null)
        {
            var relativeMousePosition = ConvertMousePosition(windowMousePosition);
            var from = _camera.ProjectRayOrigin(relativeMousePosition);
            var dir = _camera.ProjectRayNormal(relativeMousePosition);
            var globalPosition = Plane.PlaneXZ.IntersectsRay(from - dir * 10.0f, dir);

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
        if (!Game.IsRunning) return;

        if (@event.IsActionPressed("summon"))
        {
            GD.Print("Try Summon");
            TrySummon();
            return;
        }

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

    private void TryReleaseObject(bool itemInViewport)
    {
        if (GrabbedObject == null || _lastItemDrag != null) return;

        if (!Input.IsActionPressed("grab"))
        {
            if (!itemInViewport)
            {
                _summoningArea.RemoveChild(GrabbedObject);
                GrabbedObject.QueueFree();
            }

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

    private void SetCurrentItemPreview(Item item)
    {
        RemoveCurrentItemPreview();

        var scene = item.RoomItem ?? QuestItem;
        GrabbedObject = (Node3D)scene.Instantiate();
        if (GrabbedObject is RoomItem roomItem)
        {
            roomItem.Item = item;
            if (item.RoomItem == null)
                roomItem.GetNode<Sprite3D>("sprite").Texture = item.Texture;
        }

        _summoningArea.AddChild(GrabbedObject);
    }

    private void RemoveCurrentItemPreview()
    {
        if (GrabbedObject == null) return;
        _summoningArea.RemoveChild(GrabbedObject);
        GrabbedObject.QueueFree();
        GrabbedObject = null;
    }

    private void ClearSummoningArea()
    {
        foreach (var child in _summoningArea.GetChildren())
        {
            if (child == GrabbedObject) continue;

            _summoningArea.RemoveChild(child);
            child.QueueFree();
        }
    }

    private void TrySummon()
    {
        if (SummonedMonster != null)
        {
            return;
        }

        var children = _summoningArea.GetChildren();

        var visited = new System.Collections.Generic.Dictionary<int, GraphNode>();
        var stack = new Stack<(RoomItem, GraphNode, int)>();

        var startingCrystals = new List<GraphNode>();
        foreach (var child in children)
        {
            if (child is not RoomItem roomItem) continue;

            stack.Clear();
            stack.Push((roomItem, null, 0));

            while (stack.Count > 0)
            {
                var (last, lastParent, lastParentAngle) = stack.Pop();
                var index = last.GetIndex();


                GraphNode node;
                if (visited.TryGetValue(index, out var existing))
                {
                    node = existing;
                }
                else
                {
                    switch (last.Item)
                    {
                        case Crystal crystal:
                        {
                            var edge = new GraphNode
                            {
                                Index = index,
                                Item = crystal,
                                Links = new()
                            };

                            if (crystal.Directions == CrystalDirections.I)
                            {
                                startingCrystals.Add(edge);
                            }

                            foreach (var ray in last.Rays)
                            {
                                if (ray.ClosestNode?.GetParent() is not RoomItem linkedItem) continue;
                                var nextIndex = linkedItem.GetIndex();

                                if (visited.TryGetValue(nextIndex, out var nextNode))
                                {
                                    edge.AddLink(ray.Angle, nextNode);
                                    nextNode.AddLink(InvertAngle(ray.Angle), edge);
                                }
                                else
                                {
                                    stack.Push((linkedItem, edge, ray.Angle));
                                }
                            }

                            node = edge;
                            break;
                        }
                        case QuestItem item:
                            node = new GraphNode
                            {
                                Index = index,
                                Item = item,
                                Links = new()
                            };
                            break;
                        default:
                            continue;
                    }

                    visited.Add(index, node);
                }

                if (lastParent != null)
                {
                    node.AddLink(InvertAngle(lastParentAngle), lastParent);
                    lastParent.AddLink(lastParentAngle, node);
                }
            }
        }

        foreach (var start in startingCrystals)
        {
            start.Normalize();
            start.Print();

            foreach (var monster in Game.AllMonsters)
            {
                if (start.Compare(monster.Diagram))
                {
                    SpawnMonster(monster);
                    return;
                }
            }
        }

        SpawnNothing();
    }

    private void SpawnMonster(Monster monster)
    {
        SummonedMonster = (SummonedMonster)SummonedMonsterScene.Instantiate();
        SummonedMonster.Monster = monster;
        SummonedMonster.AnimationFinished += OnMonsterSpawned;
        AddChild(SummonedMonster);

        Game.SummonMonster(monster);
    }

    private void SpawnNothing()
    {
        SummonedMonster = (SummonedMonster)SummonedNothingScene.Instantiate();
        SummonedMonster.AnimationFinished += OnNothingSpawned;
        AddChild(SummonedMonster);
    }

    private void OnMonsterSpawned()
    {
        ClearSummoningArea();
        if (SummonedMonster != null)
        {
            RemoveChild(SummonedMonster);
            SummonedMonster.QueueFree();
        }

        SummonedMonster = null;

        Game.CheckVictory();
    }

    private void OnNothingSpawned()
    {
        if (SummonedMonster != null)
        {
            RemoveChild(SummonedMonster);
            SummonedMonster.QueueFree();
        }

        SummonedMonster = null;
    }

    private void OnNewMonster(Monster monster)
    {
        _zooMap.ShowMonster(monster);
    }

    private int InvertAngle(int angle)
    {
        angle = (angle + 180) % 360;
        if (angle > 180)
            angle -= 360;
        return angle;
    }
}

internal class GraphNode
{
    public int Index;
    public Item Item;
    public List<(int, GraphNode)> Links;

    public void Print()
    {
        var visited = new HashSet<int>();
        var queue = new Stack<(GraphNode, GraphNode, int)>();

        var n = 0;

        queue.Push((this, null, 0));
        while (queue.Count > 0)
        {
            var (last, prev, lastAngle) = queue.Pop();
            if (!visited.Add(last.Index)) continue;

            if (prev != null)
            {
                GD.Print("#[", n, ", ", last.Item.Name.PadLeft(16), "]:\t", last.Item.GlobalId, ", ", lastAngle);
                n += 1;
            }

            if (last.Links.Count == 0) continue;
            for (var i = last.Links.Count - 1; i >= 0; i--)
            {
                var (angle, next) = last.Links[i];
                if (visited.Contains(next.Index)) continue;
                queue.Push((next, last, angle));
            }
        }
    }

    public bool Compare(Array<Vector2I> items)
    {
        var visited = new HashSet<int>();
        var queue = new Stack<(GraphNode, GraphNode, int)>();

        var pos = 0;

        queue.Push((this, null, 0));
        while (queue.Count > 0)
        {
            var (last, prev, lastAngle) = queue.Pop();
            if (!visited.Add(last.Index)) continue;

            if (pos >= items.Count)
            {
                GD.Print("Too many");
                return false;
            }

            if (prev != null)
            {
                GD.Print("Node", prev.Index, " -> Node", last.Index, "[label=\"", lastAngle, "\", id=\"",
                    last.Item.GlobalId, "\"]");

                var (expectedId, expectedAngle) = items[pos];
                if (last.Item.GlobalId != expectedId || expectedAngle != lastAngle) return false;
                pos += 1;
            }

            if (last.Links.Count == 0) continue;
            for (var i = last.Links.Count - 1; i >= 0; i--)
            {
                var (angle, next) = last.Links[i];
                if (visited.Contains(next.Index)) continue;
                queue.Push((next, last, angle));
            }
        }

        return pos == items.Count;
    }

    public bool AddLink(int angle, GraphNode node)
    {
        if (Links.Any(item => item.Item2.Index == node.Index)) return false;
        Links.Add((angle, node));
        return true;
    }

    public void Normalize()
    {
        var visited = new HashSet<int>();
        var stack = new Queue<GraphNode>();

        stack.Enqueue(this);
        while (stack.Count > 0)
        {
            var last = stack.Dequeue();
            if (!visited.Add(last.Index)) continue;

            last.Links.Sort((left, right) => left.Item1.CompareTo(right.Item1));

            foreach (var (_, next) in last.Links)
            {
                if (visited.Contains(next.Index)) continue;
                stack.Enqueue(next);
            }
        }
    }
}
