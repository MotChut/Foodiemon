using Godot;
using System;
using System.Collections.Generic;
using static Resources;

public partial class RawBerry : Entity
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
        GetNode<Label>("InteractiveNotice/SubViewport/Label").Text = currentTask.ToString();
        if(currentState == States.Hurt)
        {
            KnockBack();
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
            mainTask = currentTask;
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
}
