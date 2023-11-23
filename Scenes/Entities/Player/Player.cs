using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export] public float maxSpd = 6.0f;
	[Export] public float accel = 3.0f;
	[Export] public float friction = 3.0f;
	public const float JumpVelocity = 4.5f;
	public const float AngularAccel = 8.0f;
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	Node3D sprite;
	AnimationPlayer animationPlayer;

	public enum States
	{
		Idle, Walk
	}

	public States currentState = States.Idle;
	public Vector3 lastDirection = Vector3.Zero;


    public override void _Ready()
    {
		sprite = GetNode<Node3D>("Sprite");
		animationPlayer = GetNode<AnimationPlayer>("Sprite/AnimationPlayer");
    }

    public override void _PhysicsProcess(double delta)
	{
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
		RotateToDirection((float)delta);
		MoveAndSlide();
		
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
