using Godot;
using System.Collections.Generic;
using static Resources;

public partial class SettingUI : CanvasLayer
{
	private SettingUI()
    {
    }
    private static SettingUI _instance;
    public static SettingUI GetInstance()
    {
        if (_instance == null)
        {
            _instance = new SettingUI();
        }
        return _instance;
    }

	List<Vector2I> resolutions = new List<Vector2I>()
	{
		new Vector2I(640, 360), new Vector2I(1280, 720), new Vector2I(1920, 1080)
	};
	const int SIZEX = 1280;
	const int SIZEY = 720;

	public int resolutionIndex = 1;
	public bool showHP = false;

	Button resolutionBtn, volumnBtn, showHPBtn, toTitleBtn;
	public override void _Ready()
	{
		resolutionBtn = GetNode<Button>("Menu/TextureRect/MarginContainer/ScrollContainer/MarginContainer/Inventory/Resolution");
		volumnBtn = GetNode<Button>("Menu/TextureRect/MarginContainer/ScrollContainer/MarginContainer/Inventory/Volumn");
		showHPBtn = GetNode<Button>("Menu/TextureRect/MarginContainer/ScrollContainer/MarginContainer/Inventory/ShowHP");
		toTitleBtn = GetNode<Button>("Menu/TextureRect/MarginContainer/ScrollContainer/MarginContainer/Inventory/ToTitle");

		resolutionBtn.Connect("pressed", new Callable(this, "ChangeResolution"));
		showHPBtn.Connect("pressed", new Callable(this, "ShowHideHP"));
		toTitleBtn.Connect("pressed", new Callable(this, "BackToTitle"));
		volumnBtn.Connect("pressed", new Callable(this, "ChangeVolumn"));

		UpdateSize();
	}

    public override async void _PhysicsProcess(double delta)
    {
        if(Input.IsActionJustPressed("escape") && Visible)
		{
			Visible = false;
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			GetTree().Root.GetNode<PlayerHUD>("PlayerHud").Visible = true;
			GetTree().Paused = false;
		}
    }

    void UpdateSize()
	{
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		Scale = new Vector2(viewportSize.X / SIZEX, viewportSize.Y / SIZEY);
	}

	void ChangeResolution()
	{
		if(resolutionIndex == resolutions.Count) resolutionIndex = 0;
		else resolutionIndex++;
		
		switch(resolutionIndex)
		{
			case 0:
			case 1:
			case 2:
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
			DisplayServer.WindowSetSize(resolutions[resolutionIndex]);
			DisplayServer.WindowSetPosition(DisplayServer.ScreenGetSize() / 2 - (Vector2I)GetViewport().GetVisibleRect().Size / 2);
			resolutionBtn.Text = resolutions[resolutionIndex].X.ToString() + " x " + resolutions[resolutionIndex].Y.ToString();
			break;
			case 3:
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
			resolutionBtn.Text = "Fullscreen";
			break;
		}

		UpdateSize();
	}

	void ShowHideHP()
	{
		showHP = !showHP;
		if(!showHP) showHPBtn.Text = "Show Enemy HP";
		else showHPBtn.Text = "Hide Enemy HP";
	}

	void BackToTitle()
	{
		NextPath = TitleScreen_Path;
		GetTree().ChangeSceneToFile(LoadingScene_Path);
	}

	void ChangeVolumn()
	{
		string text = GetTree().Root.GetNode<AudioController>("AudioController").SetVolumnRate();
		volumnBtn.Text = "Volumn: " + text;
	}
}
