using Godot;
using System;
using System.Collections.Generic;
using static Resources;

public partial class RawrBerry : Entity
{
    #region Init

    public override void _Ready()
    {
        base._Ready();

        rnd = new Random();
        TaskByGroup(Task.Explore, this);
    }

    public override void _PhysicsProcess(double delta)
    {
        healthContainer.Visible = GetTree().Root.GetNode<SettingUI>("SettingUi").showHP;

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
        GiveTask(rawrberry, GameTime.Noon, Task.Explore);
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
            animationTree.Set("parameters/Transition/transition_request", "Chase");
            if(target != null) KeepTrackOfTarget();
		    if(targetPos != Vector3.Zero) Chase();
            break;
            case Task.Runaway:
            Runaway();
            break;
            case Task.Explore:
            animationTree.Set("parameters/Transition/transition_request", "Walk");
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

    public override void AttackArea_Body(Node3D body)
    {
        if(canAttack)
        {
            canAttack = false;
            isAttacking = true;
            animationTree.Set("parameters/OneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
        }
    }

    async void DealDamage()
    {            
        var targets = attackArea.GetOverlappingBodies();
		foreach(var target in targets)
		{
			if(target is Entity && target != this)
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
        isAttacking = false;
        await ToSignal(GetTree().CreateTimer(statsSettings.attackSpd), "timeout");
        canAttack = true;
    }

    public override void GetHit(Vector3 source, Node3D danger, float knockStr, int dmg)
	{
		if(invincibleTimer.IsStopped())
		{
			animationTree.Set("parameters/HurtShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
			hp -= dmg;
            healthBar.Value = hp;
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

    public override void GiveMaterials()
    {
        List<MaterialType?> dropList = new List<MaterialType?>(){ MaterialType.RawrMeat, MaterialType.SrawrBerry};
        MaterialType? materialType =  dropList[rnd.Next(dropList.Count)];
        int amount = rnd.Next(3) + 1;
        if(userdata.userIngredients.ContainsKey(materialType))
            userdata.userIngredients[materialType] += amount;
        else
            userdata.userIngredients.Add(materialType, amount);
    }
}
