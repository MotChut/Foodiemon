using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export] public float maxSpd = 6.0f;
	[Export] public float accel = 3.0f;
	[Export] public float friction = 3.0f;
	public const float JumpVelocity = 4.5f;
	public const float AngularAccel = 8.0f;
	public const float AttackAngularAccel = 15.0f;
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	float angular = AngularAccel;

	Node3D sprite, areas;
	AnimationPlayer animationPlayer;
	Camera3D camera;
	Area3D attackArea;

	public enum States
	{
		Idle, Walk, Attack, Dodge
	}

	public States currentState = States.Idle;
	public Vector3 lastDirection = Vector3.Zero;


    public override void _Ready()
    {
		sprite = GetNode<Node3D>("Sprite");
		areas = GetNode<Node3D>("Areas");
		attackArea = areas.GetNode<Area3D>("AttackArea");
		animationPlayer = GetNode<AnimationPlayer>("Sprite/AnimationPlayer");
		camera = GetParent().GetNode<Camera>("Camera").GetNode<Camera3D>("Camera3D");
    }

    public override void _PhysicsProcess(double delta)
	{
		#region Movement
		if(currentState != States.Attack && Input.IsActionJustPressed("attack"))
		{
			Attack();
		}
		else if(currentState != States.Attack)
		{
			Vector3 velocity = Velocity;
			Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
			Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Rotated(Vector3.Up, GetParent().GetNode<Node3D>("Camera").Rotation.Y).Normalized();
		
			if(direction != Vector3.Zero)
			{
				if (direction != lastDirection) lastDirection = direction;
				if (currentState != States.Walk) SetState(States.Walk);
				velocity = velocity.MoveToward(maxSpd * direction, accel);
			}
			else
			{
				if(currentState != States.Idle) SetState(States.Idle);
				velocity = velocity.MoveToward(Vector3.Zero, friction);
			}

			velocity += new Vector3(0, -10, 0);
			Velocity = velocity;
			
			MoveAndSlide();
		}
		RotateToDirection((float)delta);
		
		#endregion
		
	}

	void RotateToDirection(float delta)
	{
		sprite.Rotation = GetFacingDirection(sprite, delta);
		areas.Rotation = sprite.Rotation;
	}

	Vector3 GetFacingDirection(Node3D o, float delta)
	{
		Vector3 rotatedDirection = lastDirection;
		Vector3 r = new Vector3(o.Rotation.X, 
							Mathf.LerpAngle(o.Rotation.Y, Mathf.Atan2(rotatedDirection.X, rotatedDirection.Z), delta * angular), 
							o.Rotation.Z);
		return r;
	}

	void SetState(States state)
	{
		currentState = state;
		animationPlayer.Play(Enum.GetName(state));
	}

	async void Attack()
	{
		SetState(States.Attack);
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		Vector2 mousePos = GetViewport().GetMousePosition();
		Vector3 origin = camera.ProjectRayOrigin(mousePos);
		Vector3 end = origin + camera.ProjectRayNormal(mousePos) * 1000;
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(origin, end);
		query.CollideWithBodies = true;

		var result = spaceState.IntersectRay(query);
		if(result != null)
		{
			angular = AttackAngularAccel;
			lastDirection = GlobalPosition.DirectionTo((Vector3)result["position"]);
			var difference = Mathf.Abs(Mathf.Wrap(Mathf.Atan2(lastDirection.X, lastDirection.Z) - sprite.Rotation.Y, 0.0f, Mathf.Tau));
   			var distance = Mathf.Abs(Mathf.Wrap(2.0 * difference, 0.0f, Mathf.Tau) - difference);
			await ToSignal(GetTree().CreateTimer(distance * 0.1), "timeout");
			DealDamage();
			SetState(States.Idle);
			angular = AngularAccel;
		}
		
	}

	void DealDamage()
	{
		var targets = attackArea.GetOverlappingBodies();
		foreach(var target in targets)
		{
			if(target is Entity)
			{
				Entity e = target as Entity;
				e.GetHit(GlobalPosition, 3);
			}
		}
	}
}
