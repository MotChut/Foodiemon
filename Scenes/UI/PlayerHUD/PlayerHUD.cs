using Godot;
using System;

public partial class PlayerHUD : CanvasLayer
{
	public override void _Process(double delta)
	{
		GetNode<Label>("FPS").Text = "Fps: " + Engine.GetFramesPerSecond().ToString();
	}
}
