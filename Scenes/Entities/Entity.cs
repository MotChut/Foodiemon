using Godot;
using System;
using System.Collections.Generic;
using static Resources;

public partial class Entity : CharacterBody3D
{
	[Export] public Vector3 objectSize;
	[Export] public int nRaycasts = 32;
	[Export] public float raycastLength = 1.5f;
	[Export] protected float rotAngle = 0.03f;
	[Export] public float minSpd;
	[Export] public float maxSpd;
	[Export] protected float accel;
	[Export] protected float friction;
	[Export] protected int maxHp;
	[Export] protected int attackPoint;
	[Export] protected int attackSpd;
	
	[Export] public Entities entityType;

	protected float speed;
	public Player target = null;

	//protected int workPoint;

	public enum States
	{
		Idle, Walk
	}

	protected States currentState = States.Idle;
	public Vector3 lastDirection = Vector3.Zero;


	public Node3D raycastsNode;
	public Node3D trackersNode;
	Area3D preyTracker;


	// Practice
	
	SteeringBehavior steer;
	List<Vector3> rayDirections = new List<Vector3>();
	Vector3 currentDir = Vector3.Zero;
	public Vector3 lastDir = Vector3.Zero;
	Vector3 velocity;

	public Vector3 GetCurrentDir() { return currentDir; }


    public override void _Ready()
    {
		// Get Nodes
        raycastsNode = GetNode<Node3D>("Raycasts");
        trackersNode = GetNode<Node3D>("Trackers");
		preyTracker = GetNode<Area3D>("Trackers/PreyTracker");
		
		Init(); // Generate Stats
		
		// Steering Behaviors
		GenerateRaycasts();
		steer = new SteeringBehavior(this);

		// Trackers
		preyTracker.Connect("body_entered", new Callable(this, "PreyEnter"));
    }

    public override void _PhysicsProcess(double delta)
    {
		//Chase();
    }

	void Init()
	{
		Random random = new Random();
		speed = (float)(random.NextDouble() * (maxSpd - minSpd) + minSpd);
	}

	void ActionManager()
	{

	}

	void Chase()
	{
		currentDir = Vector3.Zero;
        currentDir += steer.Seek();
		currentDir += steer.Evade();
		currentDir.X = Mathf.Lerp(lastDir.X, currentDir.X, rotAngle);
		currentDir.Z = Mathf.Lerp(lastDir.Z, currentDir.Z, rotAngle);
		currentDir = currentDir.Normalized();
		LookAt(new Vector3(currentDir.X + GlobalPosition.X, GlobalPosition.Y, currentDir.Z + GlobalPosition.Z), Vector3.Up);
        velocity = currentDir * speed;
		velocity.Clamp(Vector3.Zero, new Vector3(1, 0, 1) * speed);
		Velocity = velocity;
		MoveAndSlide();
		lastDir = currentDir;
	}

	void Wander()
	{

	}

	void GenerateRaycasts()
	{
		if(nRaycasts == 0) return;
		for(int i = 0; i < nRaycasts; i++)
		{
			float angle = (float)(i * Math.PI / nRaycasts);
			rayDirections.Add(Vector3.Right.Rotated(Vector3.Up, angle));
            RayCast3D rayCast3D = new RayCast3D
            {
                TargetPosition = new Vector3(raycastLength, 0, 0),
                Rotation = new Vector3(0, angle, 0)
            };
			rayCast3D.SetCollisionMaskValue(2, true);
			rayCast3D.SetCollisionMaskValue(3, true);
            raycastsNode.AddChild(rayCast3D);
		}
	}
}
