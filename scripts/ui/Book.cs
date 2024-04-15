using Godot;
using Godot.Collections;

namespace ldjam55.scripts.ui;

public partial class Book : Node
{
    [Export] public Button ButtonPrev;
    [Export] public Button ButtonNext;
    [Export] public Label PageLabel;

    [Export] public Array<Texture2D> Pages;

    private int _currentPage;
    private TextureRect _page;

    public override void _Ready()
    {
        _page = GetNode<TextureRect>("page");
        PageLabel.Text = (_currentPage + 1).ToString();

        if (Pages.Count > 0)
            _page.Texture = Pages[_currentPage];

        ButtonNext.Pressed += NextPage;
        ButtonPrev.Pressed += PrevPage;
    }

    public override void _Process(double delta)
    {
    }

    private void NextPage()
    {
        TurnPage(1);
    }

    private void PrevPage()
    {
        TurnPage(-1);
    }

    private void TurnPage(int n)
    {
        if (Pages.Count == 0) return;

        _currentPage = (_currentPage + n) % Pages.Count;
        if (_currentPage < 0)
            _currentPage += Pages.Count;

        _page.Texture = Pages[_currentPage];
        PageLabel.Text = (_currentPage + 1).ToString();
    }
}
