using Godot;
using System;
using static Resources;

public partial class RawrBerry : Entity
{
    #region Init
    AnimationPlayer effectPlayer;
	AnimationTree animationTree;
	AnimatedSprite2D effectSprite;
	Sprite3D effectNode;

    public override void _Ready()
    {
        base._Ready();
        effectNode = GetNode<Sprite3D>("Effects/AttackEffect");
		effectPlayer = GetNode<AnimationPlayer>("Effects/AttackEffect/SubViewport/AnimationPlayer");
		effectSprite = GetNode<AnimatedSprite2D>("Effects/AttackEffect/SubViewport/AnimatedSprite2D");
		animationTree = GetNode<AnimationTree>("AnimationTree");

        rnd = new Random();
        TaskByGroup(Task.Explore, this);
    }

    public override void _PhysicsProcess(double delta)
    {
        GetNode<Label>("InteractiveNotice/SubViewport/Label").Text = currentTask.ToString();
        if(currentState == States.Hurt)
        {
            KnockBack();
        }
        if(isAttacking)
        {
            return;
        }
        else
        {
            if(currentState != States.Idle)
                TaskHandler();
        }
    }

    #endregion

     #region Tasks Related
    public override async void UpdateCurrentTask()
    {
        if(tasks[(int)CurrentTime] == Task.Continue) return;
        
        if(isBusy) savedTask = tasks[(int)CurrentTime];
        else SetCurrentTask(tasks[(int)CurrentTime]);

        if(CurrentTime == GameTime.Day)
        {
            Visible = true;
            await ToSignal(GetTree().CreateTimer(1), "timeout");
            GetNode<CollisionShape3D>("BodyCollision").SetDeferred("disabled", false);
        }
        else if(CurrentTime == GameTime.Night && isHome)
        {
            HomeRest((EntityBase)pack.structures[0]);
        }
    }

    public override void SetCurrentTask(Task task)
    {
        currentTask = Task.Idle;
        
        switch(task)
        {   
            case Task.GoHome:
            case Task.ForceHome:
            case Task.HomeRest:
            if(isHome && CurrentTime == GameTime.Night)
            {
                HomeRest((EntityBase)pack.structures[0]);
            }
            else
            {
                target = null;
                targetPos = pack.structures[0].GlobalPosition;
            }
            break;
            case Task.Explore:
            pack.wanderDir = steer.Wander();
            break;

        }

        currentTask = task;
    }

    public override void TaskByGroup(Task task, Entity rawrberry)
    {
        GiveTask(rawrberry, GameTime.Day, Task.Explore);
        GiveTask(rawrberry, GameTime.Noon, Task.Continue);
        GiveTask(rawrberry, GameTime.Night, Task.HomeRest);
    }

    public override void GiveTask(Entity rawrberry, GameTime time, Task task)
    {
        rawrberry.tasks[(int)time] = task;
    }

    public override void TaskHandler()
    {
        switch(currentTask)
        {
            case Task.Hunt:
            if(target != null) KeepTrackOfTarget();
		    if(targetPos != Vector3.Zero) Chase();
            break;
            case Task.Runaway:
            Runaway();
            break;
            case Task.Explore:
            Wander();
            break;
            case Task.HomeRest:
            case Task.ForceHome:
            case Task.GoHome:
            Chase();
            break;
            
            case Task.Idle:
            break;
        }
    }

    #endregion
	
    #region Signals
    public override void SelfEntered_Area(Area3D area3D)
    {
        Node3D body = (Node3D)area3D.Owner;
		if(body is EntityBase)
        {
            EntityBase entityBase = (EntityBase)body;
            if(entityBase != pack.structures[0]) return;
            isHome = true;
            if(currentTask == Task.HomeRest)
            {
                HomeRest(entityBase);
            }
            if(currentTask == Task.ForceHome)
            {
                pack.maxExploreDistance = 0;
                SetCurrentTask(Task.Idle);
                SetCurrentTask(Task.Explore);
            }
        }
    }

    public override void SelfExited_Area(Area3D area3D)
    {
        Node3D body = (Node3D)area3D.Owner;
        if(body is EntityBase)
        {
            isHome = false;
        }
    }

    public override void VisionEntered_Body(Node3D body)
    {	
        base.VisionEntered_Body(body);
        if(body is Player)
        {
            if(target != null) return;
            mainTask = currentTask;
            target = body;
            SetCurrentTask(Task.Hunt);
        }
        else if(body is Entity)
        {
            Entity entity = (Entity)body;
            if(entity.entityType == Entities.Rawrberry || target != null) return;
            target = body;
            SetCurrentTask(Task.Hunt);
        }
    }

    #endregion

    public void HomeRest(EntityBase entityBase)
    {
        GetNode<CollisionShape3D>("BodyCollision").SetDeferred("disabled", true);
        
        SetCurrentTask(Task.Idle);
        Visible = false;
    }

    void PlayEffect()
	{
		effectSprite.Play("Attack");
	}

    public override void AttackArea_Body(Node3D body)
    {
        if(canAttack)
        {
            canAttack = false;
            isAttacking = true;
            GetNode<AnimationPlayer>("AnimationPlayer").Play("AttackReady");
        }
    }

    async void DealDamage()
    {            
        effectPlayer.Play("Attack");
        var targets = attackArea.GetOverlappingBodies();
		foreach(var target in targets)
		{
			if(target is Entity)
			{
				Entity e = target as Entity;
				e.GetHit(GlobalPosition, this, statsSettings.force, statsSettings.attackPoint);
			}
            if(target is Player)
			{
				Player e = target as Player;
				e.GetHit(GlobalPosition, this, statsSettings.force, statsSettings.attackPoint);
			}
		}

        await ToSignal(GetTree().CreateTimer(statsSettings.attackSpd), "timeout");
        isAttacking = false;
        canAttack = true;
    }

    public override void GetHit(Vector3 source, Node3D danger, float knockStr, int dmg)
	{
		if(invincibleTimer.IsStopped())
		{
			if(!GetNode<AnimationPlayer>("AnimationPlayer").IsPlaying()) GetNode<AnimationPlayer>("AnimationPlayer").Play("Hurt");
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
}
