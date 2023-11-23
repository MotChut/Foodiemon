using Godot;
using System;
using System.Collections.Generic;
using static Resources;

public partial class Chicpea : Entity
{
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
            chicpea.tasks = new List<Task>(3){ Task.Idle, Task.Idle, Task.Idle };
            GiveTask((Chicpea)chicpea, GameTime.Day, Task.Explore);
            GiveTask((Chicpea)chicpea, GameTime.Noon, Task.GoHome);
            GiveTask((Chicpea)chicpea, GameTime.Night, Task.HomeRest);
            chicpea.UpdateCurrentTask();
        }
    }

    public override void GiveTask(Chicpea chicpea, GameTime time, Task task)
    {
        chicpea.tasks[(int)time] = task;
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
            case Task.GoHome:
            case Task.HomeRest:
            case Task.ForceHome:
            targetPos = pack.structures[0].GlobalPosition;
            Chase();
            break;
            case Task.Idle:
            break;
        }
    }
}
