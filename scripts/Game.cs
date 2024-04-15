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
    [Export] public AudioStreamPlayer AudioPlayer;

    [Export] public Array<resources.Monster> AllMonsters;

    [Export] public Array<AudioStream> Music;


    private readonly HashSet<int> _summonedMonsters = new();
    private int _currentMusic;

    [Signal]
    public delegate void MonsterSummonedEventHandler(resources.Monster monster);

    public override void _Ready()
    {
        GetTree().Root.MinSize = new Vector2I(1280, 768);

        Debug.Assert(Inventory != null);
        Room.Game = this;

        if (Music.Count > 0)
        {
            AudioPlayer.Stream = Music[_currentMusic];
            AudioPlayer.Play();
            AudioPlayer.Finished += SetNextMusic;
        }
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

    private void SetNextMusic()
    {
        if (Music.Count == 0) return;

        _currentMusic = (_currentMusic + 1) % Music.Count;
        AudioPlayer.Stream = Music[_currentMusic];
        AudioPlayer.Play();
    }
}
