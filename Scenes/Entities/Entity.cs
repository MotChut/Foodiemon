using Godot;
using System;
using System.Collections.Generic;
using static Resources;

public partial class Entity : CharacterBody3D
{
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


	// Practice
	
	SteeringBehavior steer;
	List<Vector3> rayDirections = new List<Vector3>();
	Vector3 currentDir = Vector3.Zero;
	public Vector3 lastDir = Vector3.Zero;
	Vector3 velocity;

	public Vector3 GetCurrentDir() { return currentDir; }


    public override void _Ready()
    {
        raycastsNode = GetNode<Node3D>("Raycasts");

		//target = GetParent().GetParent().GetNode<Player>("Player");
		
		Init();
		GenerateRaycasts();

		steer = new SteeringBehavior(this);
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

	/*
    

	void SetInterest()
	{
		Vector3 direction = GlobalPosition.DirectionTo(player.GlobalPosition);
		for(int i = 0; i < nRaycasts; i++)
		{
			float d = rayDirections[i].Dot(direction);
			interest[i] = Math.Max(0, d);
			//if((GlobalPosition - player.GlobalPosition).Length() < 1) isStopped = true;
		}

		string s = "";
		foreach(float i in interest) s = s + i.ToString() + ", ";
		GD.Print(s);
	}

	void SetDefaultInterest()
	{
		for(int i = 0; i < nRaycasts; i++)
		{
			float d = rayDirections[i].Dot(Transform.Basis.Z) * raycastLength;
			interest[i] = Math.Max(0, d);
			//if((GlobalPosition - player.GlobalPosition).Length() < 1) isStopped = true;
		}
	}

	bool CollisionWithin()
	{
		foreach(RayCast3D rayCast3D in raycastsNode.GetChildren())
		{
			if(rayCast3D.IsColliding()) return true;
		}
		return false;
	}

	void SetDanger()
	{
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		for(int i = 0; i < nRaycasts; i++)
		{
			PhysicsRayQueryParameters3D states = PhysicsRayQueryParameters3D.Create(Position, 
				Position + rayDirections[i].Rotated(Vector3.Up, Rotation.Y) * raycastLength);
			states.Exclude = new Godot.Collections.Array<Rid>(){ GetRid() };
			
			Godot.Collections.Dictionary result = spaceState.IntersectRay(states);
			if (result.Count > 0)
			{
				danger[i] = 1 - GlobalPosition.DistanceTo(((Node3D)result["collider"]).GlobalPosition);
			}
			else danger[i] = 0;
			// RayCast3D cast = (RayCast3D)raycastsNode.GetChild(i);
			// if(cast.IsColliding())
			// {
			// 	danger[i] = 1;
			// 	//(cast.GetCollider() as Node3D).GlobalPosition.DistanceTo(GlobalPosition);
			// }
			// else danger[i] = 0;
		}
		
	}

	void ChooseDirection()
	{
		for(int i = 0; i < nRaycasts; i++)
		{
			if(danger[i] >= interest[i]) interest[i] = 0;
			else interest[i] -= danger[i];
		}
		
		chosenDir = Vector3.Zero;
		for(int i = 0; i < nRaycasts; i++)
		{
			chosenDir += rayDirections[i] * interest[i];
		}
		chosenDir = chosenDir.Normalized();
	}

	*/
}
