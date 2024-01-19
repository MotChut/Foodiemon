using Godot;
using System;
using static Resources;

public partial class TitleScreen : CanvasLayer
{
	TextureButton startBtn;
	public override void _Ready()
	{
		startBtn = GetNode<TextureButton>("StartButton");

		startBtn.Connect("pressed", new Callable(this, "StartGame"));
		GetTree().Root.GetNode<AudioController>("AudioController").PlayTitle();
	}

	void StartGame()
	{
		GetTree().Root.GetNode<AudioController>("AudioController").PlayHome();
		NextPath = Home_Path;
		GetTree().ChangeSceneToFile(LoadingScene_Path);
	}
}
