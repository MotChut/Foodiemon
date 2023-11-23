using Godot;
using System;
using System.Collections.Generic;
using static Resources;

public partial class Entity : CharacterBody3D
{
	[Export] public Vector3 objectSize;
	[Export] public Entities entityType;

	public StatsSettings statsSettings;
	public float speed;
	public Node3D target = null;
	public Vector3 targetPos = Vector3.Zero;
	public Vector3 wanderDir = Vector3.Zero;

	//protected int workPoint;

	public enum States
	{
		Idle, Walk
	}

	public enum Task
    {
        Idle, Explore, GoHome
    }
    public Task currentTask = Task.Idle;

	public List<Task> tasks = new List<Task>();

	public States currentState = States.Idle;
	
	public Node3D raycastsNode;
	public Node3D trackersNode;
	Area3D vision;	
	public SteeringBehavior steer;
	List<Vector3> rayDirections = new List<Vector3>();
	Vector3 currentDir = Vector3.Zero;
	public Vector3 lastDir = Vector3.Zero;
	Vector3 velocity;
	public Random rnd;
    public Pack pack;

	public Vector3 GetCurrentDir() { return currentDir; }


    public override void _Ready()
    {
		rnd = new Random();
		// Get Nodes
        raycastsNode = GetNode<Node3D>("Raycasts");
        trackersNode = GetNode<Node3D>("Trackers");
		vision = GetNode<Area3D>("Trackers/Vision");
		
		Init(); // Generate Stats
		
		// Steering Behaviors
		GenerateRaycasts();
		steer = new SteeringBehavior(this);

		// Trackers
		vision.Connect("body_entered", new Callable(this, "VisionEntered"));
		vision.Connect("body_exited", new Callable(this, "VisionExited"));
    }

    public override void _PhysicsProcess(double delta)
    {
		if(target != null) KeepTrackOfTarget();
		if(targetPos != Vector3.Zero) Chase();
    }

	void Init()
	{
		statsSettings = StatsSettingsList[(int)entityType];
		Random random = new Random();
		speed = (float)(random.NextDouble() * 
				(statsSettings.maxSpd - statsSettings.minSpd) + statsSettings.minSpd);
	}

	#region Steering Behaviors
	void ActionManager()
	{

	}

	public void Chase()
	{
		currentDir = Vector3.Zero;
        currentDir += steer.Seek();
		currentDir += steer.Evade();
		Move();
	}

	public void Wander()
	{
		currentDir = Vector3.Zero;
		currentDir += wanderDir;
		currentDir += steer.Evade();
		Move();
	}

	void Move()
	{
		currentDir.X = Mathf.Lerp(lastDir.X, currentDir.X, statsSettings.rotAngle);
		currentDir.Z = Mathf.Lerp(lastDir.Z, currentDir.Z, statsSettings.rotAngle);
		currentDir = currentDir.Normalized();
		LookAt(new Vector3(currentDir.X + GlobalPosition.X, GlobalPosition.Y, currentDir.Z + GlobalPosition.Z), Vector3.Up);
        velocity = currentDir * speed;
		velocity.Clamp(Vector3.Zero, new Vector3(1, 0, 1) * speed);
		Velocity = velocity;
		MoveAndSlide();
		lastDir = currentDir;
	}

	void GenerateRaycasts()
	{
		if(statsSettings.nRaycasts == 0) return;
		for(int i = 0; i < statsSettings.nRaycasts; i++)
		{
			float angle = (float)(i * Math.PI / statsSettings.nRaycasts);
			rayDirections.Add(Vector3.Right.Rotated(Vector3.Up, angle));
            RayCast3D rayCast3D = new RayCast3D
            {
                TargetPosition = new Vector3(statsSettings.raycastLength, 0, 0),
                Rotation = new Vector3(0, angle, 0)
            };
			rayCast3D.SetCollisionMaskValue(2, true);
			rayCast3D.SetCollisionMaskValue(3, true);
            raycastsNode.AddChild(rayCast3D);
		}
	}

	#endregion


	void VisionEntered(Node3D body)
	{
		target = body;
	}

	void VisionExited(Node3D body)
	{
		target = null;
	}

	void KeepTrackOfTarget()
	{
		PhysicsDirectSpaceState3D state = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(
            GlobalPosition + new Vector3(0, 0.5f, 0), 
            target.GlobalPosition + new Vector3(0, 0.5f, 0));
        var result = state.IntersectRay(query);
		if(result == null) return;
        if((Node3D)result["collider"] == target)
			targetPos = target.GlobalPosition;
	}



	public virtual void SetPack(Pack pack){}

    public virtual void PlanTask(){}

    public virtual void GiveTask(Chicpea chicpea, Task task){}

    public virtual void ReceiveTask(){}

    public virtual void TaskHandler(){}
}
