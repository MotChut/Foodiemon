using Godot;
using System;
using System.Collections.Generic;
using static Resources;

public partial class Potatoon : Entity
{
	#region Init

    public override void _Ready()
    {
        base._Ready();

        mainTask = Task.Explore;
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
    }

    public override void SetCurrentTask(Task task)
    {
        currentTask = Task.Idle;
        
        switch(task)
        {   
            case Task.Explore:
            pack.wanderDir = steer.Wander();
            break;

        }

        currentTask = task;
    }

    public override void TaskByGroup(Task task, Entity potatoon)
    {
        GiveTask(potatoon, GameTime.Day, Task.Explore);
        GiveTask(potatoon, GameTime.Noon, Task.Explore);
        GiveTask(potatoon, GameTime.Night, Task.Explore);
    }

    public override void GiveTask(Entity potatoon, GameTime time, Task task)
    {
        potatoon.tasks[(int)time] = task;
    }

    public override void TaskHandler()
    {
        switch(currentTask)
        {
            case Task.Runaway:
            Runaway();
            break;
            case Task.Explore:
            animationTree.Set("parameters/Transition/transition_request", "Walk");
            Wander();
            break;
            
            case Task.Idle:
            break;
        }
    }

    #endregion
	
    #region Signals
    public override void SelfEntered_Area(Area3D area3D)
    {

    }

    public override void SelfExited_Area(Area3D area3D)
    {

    }

    public override void VisionEntered_Body(Node3D body)
    {	
    }

    #endregion

    public override void PlayHurt()
    {
        animationTree.Set("parameters/Transition/transition_request", "Run");
		animationTree.Set("parameters/OneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }
}
