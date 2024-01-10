using Godot;
using System;
using System.Collections.Generic;
using static Resources;

public partial class Player : CharacterBody3D
{
	[Export] public float maxSpd = 3.0f;
	[Export] public float dodgeSpd = 12.0f;
	[Export] public float accel = 3.0f;
	[Export] public float friction = 3.0f;
	public const float JumpVelocity = 4.5f;
	public const float AngularAccel = 8.0f;
	public const float AttackAngularAccel = 15.0f;
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public bool canCook = false;
	public bool canSmith = false;
	public bool canTeleport = false;

	float angular = AngularAccel;
	int attack = 1;
	float knockForce = 3;

	Node3D sprite, areas, timers;
	AnimationPlayer effectPlayer;
	AnimationTree animationTree;
	AnimatedSprite2D effectSprite;
	Sprite3D effectNode;
	Camera3D camera;
	Area3D attackArea;
	Timer dodgeTimer;

	public enum States
	{
		Idle, Walk, Attack, Dodge
	}
	List<States> notInteruptedStates = new List<States>()
	{
		States.Attack, States.Dodge
	};

	public States currentState = States.Idle;
	public Vector3 lastDirection = Vector3.Zero;


    public override void _Ready()
    {
		sprite = GetNode<Node3D>("Sprite");
		areas = GetNode<Node3D>("Areas");
		timers = GetNode<Node3D>("Timers");
		attackArea = areas.GetNode<Area3D>("AttackArea");
		effectNode = GetNode<Sprite3D>("Effects/AttackEffect");
		effectPlayer = GetNode<AnimationPlayer>("Effects/AttackEffect/SubViewport/AnimationPlayer");
		effectSprite = GetNode<AnimatedSprite2D>("Effects/AttackEffect/SubViewport/AnimatedSprite2D");
		animationTree = GetNode<AnimationTree>("AnimationTree");
		camera = GetParent().GetNode<Camera>("Camera").GetNode<Camera3D>("Camera3D");
		dodgeTimer = timers.GetNode<Timer>("DodgeTimer");

		dodgeTimer.Connect("timeout", new Callable(this, "DodgeTimeout"));
    }

    public override void _PhysicsProcess(double delta)
	{
		#region Interact
		if(Input.IsActionJustPressed("interact"))
		{
			if(canTeleport)
			{
				canTeleport = false;
				NextPath = ProceduralGeneration_Path;
				GetTree().ChangeSceneToFile(LoadingScene_Path);
				return;
			}
			else if(canCook)
			{
				canCook = false;
				CookUI cookUI = (CookUI)((PackedScene)ResourceLoader.Load(CookScene_Path)).Instantiate();
				GetParent().AddChild(cookUI);
				GetTree().Paused = true;
				canCook = true;
			}
		}

		#endregion

		#region Movement
		if(!notInteruptedStates.Contains(currentState) && Input.IsActionJustPressed("dodge"))
		{
			Dodge();
		}
		else if(!notInteruptedStates.Contains(currentState) && Input.IsActionJustPressed("attack"))
		{
			Attack();
		}
		else if(!notInteruptedStates.Contains(currentState))
		{
			Vector3 velocity = Velocity;
			Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
			Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Rotated(Vector3.Up, GetParent().GetNode<Node3D>("Camera").Rotation.Y).Normalized();

			if(direction != Vector3.Zero)
			{
				if (direction != lastDirection) lastDirection = direction;
				if (currentState != States.Walk) 
				{
					SetState(States.Walk);
					animationTree.Set("parameters/Movement/transition_request", "walk");
				}
				velocity = velocity.MoveToward(maxSpd * direction, accel);
			}
			else
			{
				if(currentState != States.Idle)
				{
					SetState(States.Idle);
					animationTree.Set("parameters/Movement/transition_request", "idle");
				}
				velocity = velocity.MoveToward(Vector3.Zero, friction);
			}

			velocity += new Vector3(0, -10, 0);
			Velocity = velocity;
			
			MoveAndSlide();
		}

		if(currentState == States.Dodge)
		{
			Vector3 velocity = dodgeSpd * lastDirection;
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
		effectNode.Rotation = new Vector3(effectNode.Rotation.X, sprite.Rotation.Y + 90, effectNode.Rotation.Z);
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
	}

	void PlayEffect()
	{
		effectSprite.Play(currentState.ToString());
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
			await ToSignal(GetTree().CreateTimer(distance * 0.05f), "timeout");
			effectPlayer.Play(currentState.ToString());
			DealDamage();
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
				e.GetHit(GlobalPosition, this, knockForce, attack);
			}
		}
	}

	void Dodge()
	{
		if(dodgeTimer.IsStopped())
		{
			dodgeTimer.Start();
			SetState(States.Dodge);
		}
	}
	void DodgeTimeout()
	{
		SetState(States.Idle);
	}
}
