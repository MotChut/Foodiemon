using Godot;
using System;

public partial class Loading : CanvasLayer
{
	const int SIZEX = 1280;
	const int SIZEY = 720;

	Label label;
	Timer timer, loadingTimer;
	AnimationPlayer animationPlayer;

	public override void _Ready()
	{
		label = GetNode<Label>("Control/Label");
		timer = GetNode<Timer>("Control/Timer");
		loadingTimer = GetNode<Timer>("LoadingTimer");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		timer.Connect("timeout", new Callable(this, "TextChange"));
		loadingTimer.Connect("timeout", new Callable(this, "LoadingDone"));

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		GetNode<Control>("Control").Scale = new Vector2(viewportSize.X / SIZEX, viewportSize.Y / SIZEY);

		Random random = new Random();
		GetNode<AnimationPlayer>("Control/AnimationPlayer").Play(random.Next(2).ToString());
	}

	void TextChange()
	{
		label.VisibleCharacters += 1;
		if(label.VisibleCharacters > label.Text.Length) label.VisibleCharacters = 0;
	}

	async void LoadingDone()
	{
		animationPlayer.PlayBackwards("fade");
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		GetTree().ChangeSceneToFile(Resources.NextPath);
	}
}
