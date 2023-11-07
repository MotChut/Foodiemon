using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 4.0f;
	public const float JumpVelocity = 4.5f;
	public const float AngularAccel = 8.0f;
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();


	Vector3 lastDirection = Vector3.Zero;

	Node3D sprite;

	public override void _PhysicsProcess(double delta)
	{
		sprite = GetNode<Node3D>("Sprite");
		// collisionShape3D = GetNode<CollisionShape3D>("CollisionShape3D");

		Vector3 velocity = Velocity;

		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
	
		if (direction != Vector3.Zero)
		{
			if (direction != lastDirection) lastDirection = direction;
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity.Rotated(Vector3.Up, GetParent().GetNode<Node3D>("Camera").Rotation.Y);
		MoveAndSlide();
		
		RotateToDirection((float)delta);
	}

	void RotateToDirection(float delta)
	{
		sprite.Rotation = GetFacingDirection(sprite, delta);
		// collisionShape3D.Rotation = GetFacingDirection(collisionShape3D ,delta);
	}

	Vector3 GetFacingDirection(Node3D o, float delta)
	{
		Vector3 rotatedDirection = lastDirection.Rotated(Vector3.Up, GetParent().GetNode<Node3D>("Camera").Rotation.Y);
		Vector3 r = new Vector3(o.Rotation.X, 
							Mathf.LerpAngle(o.Rotation.Y, Mathf.Atan2(rotatedDirection.X, rotatedDirection.Z), delta * AngularAccel), 
							o.Rotation.Z);
		return r;
	}
}
