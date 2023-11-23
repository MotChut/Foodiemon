using Godot;
using System;
using System.Collections.Generic;
using static Resources;

public partial class Chicpea : Entity
{
	[Export] bool isLeader = false;

    public override void _Ready()
    {
        base._Ready();
        rnd = new Random();
    }

    public override void _PhysicsProcess(double delta)
    {
        TaskHandler();
    }

    public override void SetPack(Pack pack)
    {
        this.pack = pack;
    }

    public override void PlanTask()
    {
        foreach(Entity chicpea in new List<Entity>(pack.entities.Keys))
        {
            switch(CurrentTime)
            {
                case GameTime.Day:
                GiveTask((Chicpea)chicpea, Task.Explore);
                break;
                case GameTime.Noon:
                GiveTask((Chicpea)chicpea, Task.Explore);
                break;
            }
            
        }
    }

    public override void GiveTask(Chicpea chicpea, Task task)
    {
        chicpea.currentTask = task;
    }

    public override void ReceiveTask()
    {

    }

    public override void TaskHandler()
    {
        switch(currentTask)
        {
            case Task.Explore:
            if(wanderDir == Vector3.Zero) wanderDir = steer.Wander();
            Wander();
            break;
        }
    }
}
