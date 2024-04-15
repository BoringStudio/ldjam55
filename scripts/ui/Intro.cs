using Godot;
using Godot.Collections;

namespace ldjam55.scripts.ui;

public partial class Intro : CanvasLayer
{
    [Export] public Button ButtonNext;
    [Export] public TextureRect PageRect;

    [Export] public Array<Texture2D> Pages;

    private int _currentPage;

    [Signal]
    public delegate void FinishedEventHandler();

    public override void _Ready()
    {
        if (Pages.Count > 0)
            PageRect.Texture = Pages[_currentPage];

        ButtonNext.Pressed += NextPage;
    }

    public override void _Process(double delta)
    {
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionReleased("skip"))
        {
            NextPage();
        }
    }


    private void NextPage()
    {
        if (_currentPage >= Pages.Count) return;

        _currentPage += 1;
        if (_currentPage < Pages.Count)
        {
            PageRect.Texture = Pages[_currentPage];
        }
        else
        {
            EmitSignal(SignalName.Finished);
        }
    }
}
