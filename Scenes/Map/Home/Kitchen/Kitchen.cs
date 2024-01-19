using Godot;
using System;

public partial class Kitchen : Node3D
{
	Area3D interactiveArea;
	Node3D notice;

	public override void _Ready()
	{
		interactiveArea = GetNode<Area3D>("InteractiveArea");
		notice = GetNode<Node3D>("InteractiveNotice");

		interactiveArea.Connect("body_entered", new Callable(this, "PlayerEnter"));
		interactiveArea.Connect("body_exited", new Callable(this, "PlayerExit"));
	}

    void PlayerEnter(Node3D body)
	{
		Player player = (Player)body;
		player.canCook = true;
		notice.Visible = true;
	}

	void PlayerExit(Node3D body)
	{
		Player player = (Player)body;
		player.canCook = false;
		notice.Visible = false;
	}
}
