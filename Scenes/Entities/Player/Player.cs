using Godot;
using System.Collections.Generic;
using static Resources;

public partial class Player : CharacterBody3D
{
	[Export] AudioStreamWav slashSound;
	[Export] public int maxHP = 5;
	[Export] public float maxSpd = 3.0f;
	[Export] public float dodgeSpd = 12.0f;
	[Export] public float accel = 3.0f;
	[Export] public float friction = 3.0f;
	[Export] public float attackSpeed = 0.5f;
	public const float JumpVelocity = 4.5f;
	public const float AngularAccel = 8.0f;
	public const float AttackAngularAccel = 15.0f;
	int hp;
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public bool canCook = false;
	public bool canSmith = false;
	public bool canTeleport = false;

	public Vector3 attackSourcePos;
	public float attackSourceKnock;
	MapSource currentMapSource;

	float angular = AngularAccel;
	int attack = 1;
	float knockForce = 3;

	BookUI bookUI;
	SettingUI settingUI;
	InventoryUI inventoryUI;

	Node3D sprite, areas, timers;
	AnimationPlayer effectPlayer;
	AnimationTree animationTree;
	AnimatedSprite2D effectSprite;
	Sprite3D effectNode;
	Camera3D camera;
	Area3D attackArea;
	Timer dodgeTimer, invincibleTimer, knockbackTimer;
	AudioStreamPlayer audioPlayer;

	public enum States
	{
		Idle, Walk, Attack, Dodge, Hurt
	}
	List<States> notInteruptedStates = new List<States>()
	{
		States.Attack, States.Dodge, States.Hurt
	};

	public States currentState = States.Idle;
	public Vector3 lastDirection = Vector3.Zero;


    public override void _Ready()
    {
		sprite = GetNode<Node3D>("Sprite");
		areas = GetNode<Node3D>("Areas");
		timers = GetNode<Node3D>("Timers");
		invincibleTimer = timers.GetNode<Timer>("InvincibleTimer");
		knockbackTimer = timers.GetNode<Timer>("KnockbackTimer");
		attackArea = areas.GetNode<Area3D>("AttackArea");
		effectNode = GetNode<Sprite3D>("Effects/AttackEffect");
		effectPlayer = GetNode<AnimationPlayer>("Effects/AttackEffect/SubViewport/AnimationPlayer");
		effectSprite = GetNode<AnimatedSprite2D>("Effects/AttackEffect/SubViewport/AnimatedSprite2D");
		animationTree = GetNode<AnimationTree>("AnimationTree");
		camera = GetParent().GetNode<Camera>("Camera").GetNode<Camera3D>("Camera3D");
		dodgeTimer = timers.GetNode<Timer>("DodgeTimer");
		audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

		dodgeTimer.Connect("timeout", new Callable(this, "DodgeTimeout"));
		knockbackTimer.Connect("timeout", new Callable(this, "KnockbackTimeout"));

		bookUI = GetTree().Root.GetNode<BookUI>("BookUi");
		settingUI = GetTree().Root.GetNode<SettingUI>("SettingUi");
		inventoryUI = GetTree().Root.GetNode<InventoryUI>("InventoryUi");

		hp = maxHP;
    }

    public override void _PhysicsProcess(double delta)
	{ 
		GetNode<Label>("InteractiveNotice/SubViewport/Label").Text = currentState.ToString();
		#region Interact
		if(Input.IsActionJustPressed("escape"))
		{
			if(!settingUI.Visible)
			{
				GetTree().Root.GetNode<PlayerHUD>("PlayerHud").Visible = false;
				settingUI.Visible = true;
				GetTree().Paused = true;
			}
		}
		if(Input.IsActionJustPressed("home"))
		{
			GetTree().Root.GetNode<PlayerHUD>("PlayerHud").Visible = false;
			Resources.Generate();
			NextPath = Home_Path;
			GetTree().ChangeSceneToFile(LoadingScene_Path);
			return;
		}
		if(Input.IsActionJustPressed("interact"))
		{
			if(currentMapSource != null)
			{
				currentMapSource.CollectResource(1);

				if(currentMapSource.isFoodSource) userdata.userIngredients[currentMapSource.resourceType] += 1;
				else userdata.userMaterials[currentMapSource.resourceType] += 1;
			}
			if(canTeleport)
			{
				GetTree().Root.GetNode<AudioController>("AudioController").Stop();
				GetTree().Root.GetNode<PlayerHUD>("PlayerHud").Visible = false;
				canTeleport = false;
				NextPath = ProceduralGeneration_Path;
				GetTree().ChangeSceneToFile(LoadingScene_Path);
				return;
			}
			else if(canSmith)
			{
				GetTree().Root.GetNode<PlayerHUD>("PlayerHud").Visible = false;
				canSmith = false;
				ForgeUI forgeUI = (ForgeUI)((PackedScene)ResourceLoader.Load(ForgeScene_Path)).Instantiate();
				GetParent().AddChild(forgeUI);
				GetTree().Paused = true;
				canSmith = true;
			}
			else if(canCook)
			{
				GetTree().Root.GetNode<PlayerHUD>("PlayerHud").Visible = false;
				canCook = false;
				CookUI cookUI = (CookUI)((PackedScene)ResourceLoader.Load(CookScene_Path)).Instantiate();
				GetParent().AddChild(cookUI);
				GetTree().Paused = true;
				canCook = true;
			}
		}
		if(Input.IsActionJustPressed("book"))
		{
			if(!bookUI.Visible)
			{
				GetTree().Root.GetNode<PlayerHUD>("PlayerHud").Visible = false;
				bookUI.Visible = true;
				GetTree().Paused = true;
			}
		}
		if(Input.IsActionJustPressed("inventory"))
		{
			if(!inventoryUI.Visible)
			{
				GetTree().Root.GetNode<PlayerHUD>("PlayerHud").Visible = false;
				inventoryUI.Visible = true;
				GetTree().Paused = true;
			}
		}
		if(Input.IsActionJustPressed("item3"))
		{
			if(hp >= maxHP) return;
			if(userdata.userEquipment.Count >= 3)
			{
				Heal();
				GetTree().Root.GetNode<PlayerHUD>("PlayerHud").Reorder(2);
				userdata.userEquipment.RemoveAt(2);
			}
		}
		if(Input.IsActionJustPressed("item2"))
		{
			if(hp >= maxHP) return;
			if(userdata.userEquipment.Count >= 2)
			{
				Heal();
				GetTree().Root.GetNode<PlayerHUD>("PlayerHud").Reorder(1);
				userdata.userEquipment.RemoveAt(1);
			}
		}
		if(Input.IsActionJustPressed("item1"))
		{
			if(hp >= maxHP) return;
			if(userdata.userEquipment.Count >= 1)
			{
				Heal();
				GetTree().Root.GetNode<PlayerHUD>("PlayerHud").Reorder(0);
				userdata.userEquipment.RemoveAt(0);
			}
		}
	
		#endregion

		#region Movement
		if(currentState == States.Hurt)
        {
            KnockBack();
        }
		else if(!notInteruptedStates.Contains(currentState) && Input.IsActionJustPressed("dodge"))
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
		effectSprite.Play("Attack");
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
			effectPlayer.Play("Attack");
			PlayAudio(slashSound);
			DealDamage();
			angular = AngularAccel;
		}
		
	}

	public void Heal()
	{
		hp += 1;
		GetTree().Root.GetNode<PlayerHUD>("PlayerHud").UpdateHealth(hp);
	}

	async void DealDamage()
	{
		var targets = attackArea.GetOverlappingBodies();
		foreach(var target in targets)
		{
			if(target is Entity)
			{
				Entity e = target as Entity;
				e.GetHit(GlobalPosition, this, knockForce, attack);
				e.HurtSound();
			}
		}
		await ToSignal(GetTree().CreateTimer(attackSpeed), "timeout");
		SetState(States.Idle);
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

	public void GetHit(Vector3 source, Node3D danger, float knockStr, int dmg)
	{
		if(invincibleTimer.IsStopped())
		{
			animationTree.Set("parameters/HurtDodge/transition_request", "hurt");
			animationTree.Set("parameters/OneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);

			hp -= dmg;
			GetTree().Root.GetNode<PlayerHUD>("PlayerHud").UpdateHealth(hp);
			if(hp <= 0)
			{ 
				//Die();
				return;
			}
			attackSourcePos = source;
			attackSourceKnock = knockStr;
			invincibleTimer.Start();
			knockbackTimer.Start();
			SetState(States.Hurt);
		}
	}

	public void KnockBack()
	{
		Vector3 velocity = attackSourcePos.DirectionTo(GlobalPosition) * attackSourceKnock;
		Velocity = velocity + new Vector3(0, -10, 0);
		MoveAndSlide();
	}

	public void KnockbackTimeout()
	{
		SetState(States.Idle);
	}

	public void SetCurrentMapSource(MapSource mapSource)
	{
		currentMapSource = mapSource;
	}

	public MapSource GetCurrentMapSource()
	{
		return currentMapSource;
	}

	void PlayAudio(AudioStreamWav sound)
	{
		audioPlayer.Stream = sound;
		audioPlayer.Play();
	}
}
