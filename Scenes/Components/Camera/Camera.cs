using Godot;
using System;

public partial class Camera : Node3D
{
	[Export] float SPEED = 3.0f;
	Player player;
	Camera3D camera3D;

	public override void _Ready()
	{
		player = GetParent().GetNode<Player>("Player");
		camera3D = GetNode<Camera3D>("Camera3D");
	}

	public override void _PhysicsProcess(double delta)
	{
		Transform3D transform3D = Transform;
		transform3D.Origin.X = (float)Mathf.Lerp(transform3D.Origin.X, player.Transform.Origin.X, SPEED * delta);
		transform3D.Origin.Z = (float)Mathf.Lerp(transform3D.Origin.Z, player.Transform.Origin.Z, SPEED * delta);
		Transform = transform3D;
	}
}
