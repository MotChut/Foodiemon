using Godot;
using System;

public partial class Player : Entity
{
	public float speed = 0f;
	public const float JumpVelocity = 4.5f;
	public const float AngularAccel = 8.0f;
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	Node3D sprite;
	AnimationPlayer animationPlayer;

	public override void _PhysicsProcess(double delta)
	{
		sprite = GetNode<Node3D>("Sprite");
		animationPlayer = GetNode<AnimationPlayer>("Sprite/AnimationPlayer");

		#region Movement
		Vector3 velocity = Velocity;
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Rotated(Vector3.Up, GetParent().GetNode<Node3D>("Camera").Rotation.Y).Normalized();
	
		if (direction != Vector3.Zero)
		{
			if (direction != lastDirection) lastDirection = direction;
			if (currentState != States.Walk) SetState(States.Walk);
			velocity = velocity.MoveToward(maxSpd * direction, accel);
		}
		else
		{
			if (currentState != States.Idle) SetState(States.Idle);
			velocity = velocity.MoveToward(Vector3.Zero, friction);
		}

		Velocity = velocity;
		MoveAndSlide();
		
		RotateToDirection((float)delta);

		#endregion
		
	}

	void RotateToDirection(float delta)
	{
		sprite.Rotation = GetFacingDirection(sprite, delta);
		// collisionShape3D.Rotation = GetFacingDirection(collisionShape3D ,delta);
	}

	Vector3 GetFacingDirection(Node3D o, float delta)
	{
		Vector3 rotatedDirection = lastDirection;
		Vector3 r = new Vector3(o.Rotation.X, 
							Mathf.LerpAngle(o.Rotation.Y, Mathf.Atan2(rotatedDirection.X, rotatedDirection.Z), delta * AngularAccel), 
							o.Rotation.Z);
		return r;
	}

	void SetState(States state)
	{
		currentState = state;
		animationPlayer.Play(Enum.GetName(state));
	}
}
