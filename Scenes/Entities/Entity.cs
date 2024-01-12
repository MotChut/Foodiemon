using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Resources;

public partial class Entity : CharacterBody3D
{
	[Export] public Vector3 objectSize;
	[Export] public Entities entityType;
	[Export] public int nFormation;
	[Export] public bool isLeader = false;
	[Export] public float waitFormationTime;
	public Vector3 followOffset = Vector3.Zero;

	public enum States
	{
		Idle, Walk, Hurt, AttackReady, Attack
	}
	public enum Task
    {
        Idle, Continue, Explore, FollowExplore,
		CollectFood, CollectMaterial, RetrieveResource, RetrieveResourceHome, CollectMaterialHome, CollectNatureFood,
		GoHome, ForceHome, HomeRest,
		Runaway, Hunt
    }
	
	// Local
	#region Local
	public StatsSettings statsSettings;
	public Dictionary<string, string> appearances = new Dictionary<string, string>();
	public Dictionary<string, List<string>> gout = new Dictionary<string, List<string>>();
	public List<Task> tasks = new List<Task>(3){ Task.Idle, Task.Idle, Task.Idle };
	public Task savedTask = Task.Idle;
	public Task mainTask = Task.Idle;
    public Task currentTask = Task.Idle;
	public States currentState = States.Walk;
	public Vector3 attackSourcePos;
	public float attackSourceKnock;
	public int hp;
	public bool canAttack = true;
	public bool isHome = false;
	[Export] public int currentHunger;
	public bool isBusy = false;
	public bool isAttacking = false;
	public bool isKid = false;
	public bool isPregnant = false;
	public float workPoint = 0;
	public MaterialType? broughtType;
	public int broughtAmount = 0;
	public int currentEstrous = 0;
	public int countEstrous = 0;
	public int currentPregnant = 0;
	public int currentGrow = 0;

	public Node3D danger = null;
	public Node3D target = null;
	public Node3D followo = null;
	public Vector3 targetPos = Vector3.Zero;
	public SteeringBehavior steer;
	List<Vector3> rayDirections = new List<Vector3>();
	Vector3 currentDir = Vector3.Zero;
	Vector3 lastDir = Vector3.Zero;

	[Export] public float speed;
	[Export] public float runSpd;
	Vector3 velocity;
	public Random rnd;
    public Pack pack;

	#endregion

	// Nodes
	public Node3D raycastsNode, trackersNode, timersNode;
	public Area3D vision, self, dangersense, attackArea;
	public Timer exploreTimer, invincibleTimer, knockbackTimer, huntTimer, giveupTimer;
	Marker3D carryPoint;

    public override void _Ready()
    {
		rnd = new Random();
		// Get Nodes
        raycastsNode = GetNode<Node3D>("Raycasts");
        trackersNode = GetNode<Node3D>("Trackers");
        timersNode = GetNode<Node3D>("Timers");
		vision = trackersNode.GetNode<Area3D>("Vision");
		self = trackersNode.GetNode<Area3D>("Self");
		dangersense = trackersNode.GetNode<Area3D>("Dangersense");
		attackArea = trackersNode.GetNode<Area3D>("AttackArea");
		exploreTimer = timersNode.GetNode<Timer>("ExploreTimer");
		invincibleTimer = timersNode.GetNode<Timer>("InvincibleTimer");
		knockbackTimer = timersNode.GetNode<Timer>("KnockbackTimer");
		huntTimer = timersNode.GetNode<Timer>("HuntTimer");
		giveupTimer = timersNode.GetNode<Timer>("GiveupTimer");
		carryPoint = GetNode<Marker3D>("CarryPoint");

		// Stats
		statsSettings = StatsSettingsList[(int)entityType];
		Random random = new Random();
		speed = (float)(random.NextDouble() * 
				(statsSettings.maxSpd - statsSettings.minSpd) + statsSettings.minSpd);
		runSpd = speed * 1.5f;
		currentHunger = statsSettings.maxHunger;
		hp = statsSettings.maxHp;

		// Steering Behaviors
		GenerateRaycasts();
		steer = new SteeringBehavior(this);

		// Trackers
		vision.Connect("body_entered", new Callable(this, "VisionEntered_Body"));
		vision.Connect("body_exited", new Callable(this, "VisionExited_Body"));
		vision.Connect("area_entered", new Callable(this, "VisionEntered_Area"));
		self.Connect("area_entered", new Callable(this, "SelfEntered_Area"));
		self.Connect("area_exited", new Callable(this, "SelfExited_Area"));
		//dangersense.Connect("body_entered", new Callable(this, "VisionEntered_Body"));
		dangersense.Connect("body_exited", new Callable(this, "DangerExited_Body"));
		dangersense.Connect("body_entered", new Callable(this, "DangerEntered_Body"));
		attackArea.Connect("body_entered", new Callable(this, "AttackArea_Body"));

		exploreTimer.Connect("timeout", new Callable(this, "ExploreTimeout"));
		knockbackTimer.Connect("timeout", new Callable(this, "KnockbackTimeout"));
		huntTimer.Connect("timeout", new Callable(this, "GiveUpPrey"));
		giveupTimer.Connect("timeout", new Callable(this, "LoseTarget"));
    }

    public override void _PhysicsProcess(double delta)
    {
		if(target != null) KeepTrackOfTarget();
		if(targetPos != Vector3.Zero) Chase();
    }

	public virtual void GenerateAppearance(){}
	public virtual void GenerateGout(){}

	public virtual void UpdateCurrentTask()
	{
		SetCurrentTask(tasks[(int)CurrentTime]);
	}

	#region Steering Behaviors
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
		currentDir += pack.wanderDir;
		currentDir += steer.Evade();
		Move();
		UpdateMaxWanderDistance();
	}

	public void Runaway()
	{
		currentDir = Vector3.Zero;
		currentDir += steer.Runaway();
		currentDir += steer.Evade();
		Move();
	}

	void UpdateMaxWanderDistance()
	{
		Vector3 housePos = pack.structures[0].GlobalPosition;
		if(housePos.DistanceTo(GlobalPosition) > pack.maxExploreDistance)
		{
			pack.maxExploreDistance = housePos.DistanceTo(GlobalPosition);
			exploreTimer.Stop();
		}
		else if(exploreTimer.IsStopped()) exploreTimer.Start();
	}

	void Move()
	{
		currentDir.X = Mathf.Lerp(lastDir.X, currentDir.X, statsSettings.rotAngle);
		currentDir.Z = Mathf.Lerp(lastDir.Z, currentDir.Z, statsSettings.rotAngle);
		currentDir = currentDir.Normalized();
		if(!GlobalTransform.Origin.IsEqualApprox(new Vector3(currentDir.X + GlobalPosition.X, GlobalPosition.Y, currentDir.Z + GlobalPosition.Z)))
			LookAt(new Vector3(currentDir.X + GlobalPosition.X, GlobalPosition.Y, currentDir.Z + GlobalPosition.Z), Vector3.Up);

		if(currentTask != Task.Runaway)
		{
			velocity = currentDir * speed * ((float)currentHunger / statsSettings.maxHunger);
			velocity.Clamp(Vector3.Zero, new Vector3(1, 0, 1) * speed * (currentHunger / statsSettings.maxHunger));
		}
		else
		{
			velocity = currentDir * runSpd * ((float)currentHunger / statsSettings.maxHunger);
			velocity.Clamp(Vector3.Zero, new Vector3(1, 0, 1) * runSpd * (currentHunger / statsSettings.maxHunger));
		}
		
		Velocity = velocity + new Vector3(0, -10, 0);
		MoveAndSlide();
		if(GlobalPosition.Y <= 0.5f) GlobalPosition = new Vector3(GlobalPosition.X, 0.5f, GlobalPosition.Z);
		lastDir = currentDir;
	}

	void GenerateRaycasts()
	{
		if(statsSettings.nRaycasts == 0) return;
		for(int i = 0; i < statsSettings.nRaycasts; i++)
		{
			float angle = (float)(i * 2 * Math.PI / statsSettings.nRaycasts);
			rayDirections.Add(Vector3.Right.Rotated(Vector3.Up, angle));
            RayCast3D rayCast3D = new RayCast3D
            {
                TargetPosition = new Vector3(statsSettings.raycastLength, 0, 0),
                Rotation = new Vector3(0, angle, 0)
            };
			rayCast3D.Position += new Vector3(0, 0.1f, 0);
			rayCast3D.SetCollisionMaskValue(2, true);
			rayCast3D.SetCollisionMaskValue(3, true);
            raycastsNode.AddChild(rayCast3D);
		}
	}

	#endregion

	#region Signals
	public virtual void VisionEntered_Body(Node3D body)
	{
		if(body is Player)
		{
			giveupTimer.Stop();
			huntTimer.Stop();
		}
	}
	public void VisionExited_Body(Node3D body)
	{
		if(body == target)
        {
            giveupTimer.Start();
            huntTimer.Start();
        }
	}
	public virtual void VisionEntered_Area(Area3D area3D){}
	public virtual void SelfEntered_Area(Area3D area3D){}
	public virtual void SelfExited_Area(Area3D area3D){}
	public virtual void DangerEntered_Body(Node3D body)
	{
		if(danger == body)
		{
			SetCurrentTask(Task.Runaway);
		}
	}
	public virtual void DangerExited_Body(Node3D body)
	{
		if(danger == body)
		{
			SetCurrentTask(mainTask);
		}
	}
	public virtual void AttackArea_Body(Node3D body){}

	public void KeepTrackOfTarget()
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
	void ExploreTimeout()
	{
		pack.directionPoints[pack.wanderDir] = 0;
		SetCurrentTask(Task.ForceHome);
	}

	#endregion

	#region Task Handler Virtual
	public virtual void SetCurrentTask(Task task){}
    public virtual void IntoGroups(){}
    public virtual void PlanTasks(){}
	public virtual void TaskByGroup(Task task, Entity entity){}
    public virtual void GiveTask(Entity entity, GameTime time, Task task){}
    public virtual void TaskHandler(){}

	#endregion

	#region Mating
	public int CountGoutMatches(Entity entity)
    {
        int count = 0;
        for(int i = 0; i < gout.Keys.Count; i++)
        {
            List<string> goutList = gout.Values.ToList()[i];
            List<string> keyList = gout.Keys.ToList();
            if(goutList.Contains(entity.appearances[keyList[i]])) count++;
        }

        return count;
    }
	public void EstrousPregnancy()
    {
        if(!isPregnant)
        {
            if(currentEstrous == statsSettings.estrousCycle)
            {
                currentEstrous = 0;
                Mating();
            }
            else currentEstrous++;
        }
        // Handle Pregnancy
        else
        {
            if(currentPregnant == statsSettings.pregnantDuration)
            {
                GiveBirth();
            }
            else currentPregnant++;
        }
    }
	public void Mating()
	{
		List<Entity> entities = new List<Entity>(pack.entities);
        while(entities.Count > 0)
        {
            Entity entity = entities[rnd.Next(entities.Count)];
            if(!entity.isKid && !entity.isPregnant && CountGoutMatches(entity) >= 2 && entity.CountGoutMatches(this) >= 2)
            {
                // 1 random entity will be pregnant
				if(rnd.NextDouble() <= 0.5f) isPregnant = true;
				else entity.isPregnant = true;
				break;
            }
            else entities.Remove(entity);
        }
	}
	public virtual void GiveBirth(){}
	public void GrowUp()
	{
		currentGrow++;
		if(currentGrow == statsSettings.growDuration)
		{ 
			Scale = Vector3.One;
			currentGrow = 0;
			isKid = false;
		}
		else 
		{
			Scale = Scale + 
			(Vector3.One - new Vector3(statsSettings.kidSize, statsSettings.kidSize, statsSettings.kidSize))
			/ statsSettings.growDuration;
		}
	}

	#endregion
	
	public async void WaitFormation()
    {
		SetState(States.Idle);
        await ToSignal(GetTree().CreateTimer(waitFormationTime), "timeout");
		SetState(States.Walk);
    }
	public void ConsumeFood()
	{
		if(statsSettings.nConsume > ((EntityBase)pack.structures[0]).pack.foods)
		{
			Starve();
		}
		else 
		{
			if(currentHunger < statsSettings.maxHunger) currentHunger += 1;
			((EntityBase)pack.structures[0]).pack.foods -= statsSettings.nConsume;
		}
	}
	public void Starve()
	{
		currentHunger -= 1;
		if(currentHunger <= 0)
		{ 
			Die();
		}
	}

	public async void Die()
	{
		GetNode<Node3D>("Sprite").Visible = false;
		GetNode<CollisionShape3D>("BodyCollision").SetDeferred("disabled", true);
		GetNode<GpuParticles3D>("HurtParticle").Emitting = true;
		await ToSignal(GetTree().CreateTimer(GetNode<GpuParticles3D>("HurtParticle").Lifetime), "timeout");
		pack.entities.Remove(this);
		QueueFree();
	}
	
	public void UpdateFollowPos()
    {
        targetPos = pack.leader.GlobalPosition + followOffset;
    }
	public void SetState(States state)
	{
		currentState = state;
		//animationPlayer.Play(Enum.GetName(state));
	}
	public virtual void GetHit(Vector3 source, Node3D danger, float knockStr, int dmg)
	{
		if(invincibleTimer.IsStopped())
		{
			GetNode<AnimationPlayer>("AnimationPlayer").Play("Hurt");
			
			this.danger = danger;
			ScareOther(danger);
			SetCurrentTask(Task.Runaway);
			hp -= dmg;
			if(hp <= 0)
			{ 
				Die();
				return;
			}
			attackSourcePos = source;
			attackSourceKnock = knockStr;
			invincibleTimer.Start();
			knockbackTimer.Start();
			SetState(States.Hurt);
		}
	}
	public void ScareOther(Node3D danger)
	{
		var entities = dangersense.GetOverlappingBodies();
		if(entities.Count > 0)
		{
			foreach(var e in entities)
			{
				if(e.GetType() != GetType() || e == this) continue;
				Entity entity = (Entity)e;
				entity.danger = danger;
				entity.savedTask = entity.currentTask;
				entity.SetCurrentTask(Task.Runaway);
			}
		}
	}
	public void KnockbackTimeout()
	{
		SetState(States.Walk);
	}
	public void KnockBack()
	{
		velocity = attackSourcePos.DirectionTo(GlobalPosition) * attackSourceKnock;
		Velocity = velocity + new Vector3(0, -10, 0);
		MoveAndSlide();
	}
	public void CarryMaterial()
	{
		Material material = (Material)MaterialSceneDictionary[broughtType].Instantiate();
		material.Position = carryPoint.Position;
		material.Name = "Material";
		AddChild(material);
	}
	public void DropMaterial()
	{
		broughtType = null;
		broughtAmount = 0;
		GetNode<Material>("Material").QueueFree();
	}
	public void GiveUpPrey()
	{
		SetCurrentTask(mainTask);
	}
	public void LoseTarget()
	{
		target = null;
	}
}
