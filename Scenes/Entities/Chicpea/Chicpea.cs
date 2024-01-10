using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Resources;

public partial class Chicpea : Entity
{
    public Dictionary<string, List<string>> appearancesList = new Dictionary<string, List<string>>()
    {
        ["comb"] = new List<string>(){ "big", "hooded", "flowing", "flatten"},
        ["eye"] = new List<string>(){ "round", "crooked", "squint", "monolid", "doublelid"},
        ["beak"] = new List<string>(){ "thick", "pointy", "sharp"},
        ["body"] = new List<string>(){ "fat", "thin", "fit"}
    };  

    #region Init
    public override void _Ready()
    {
        base._Ready();
        rnd = new Random();
        GenerateAppearance();
        GenerateGout();
    }

    public override void GenerateAppearance()
    {
        // Generate Appearances
        List<string> keys = appearancesList.Keys.ToList();
        List<List<string>> values = appearancesList.Values.ToList();
        for(int i = 0; i < keys.Count; i++)
        {
            appearances.Add(keys[i], values[i][rnd.Next(values[i].Count)]);
        }
    }

    public override void GenerateGout()
    {
        List<string> keys = new List<string>(appearancesList.Keys.ToList());
        List<List<string>> values = new List<List<string>>(appearancesList.Values.ToList());

        for(int i = 0; i < keys.Count; i++)
        {
            int nGouts = rnd.Next(statsSettings.maxGout);
            gout.Add(keys[i], new List<string>());
            while(nGouts > 0)
            {
                string value = values[i][rnd.Next(values[i].Count)];
                gout[keys[i]].Add(value);
                values[i].Remove(value);
                nGouts -=1;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if(currentState == States.Hurt)
        {
            KnockBack();
        }
        else
        {
            if(currentTask == Task.FollowExplore) UpdateFollowPos();
            if(currentState != States.Idle)
                TaskHandler();
        }
        
            
        GetNode<Label>("InteractiveNotice/SubViewport/Label").Text = currentTask.ToString();
        if(isPregnant) GetNode<Label>("InteractiveNotice/SubViewport/Label").Modulate = new Color("#964943");
        else GetNode<Label>("InteractiveNotice/SubViewport/Label").Modulate = new Color("#ffffff");
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
            if(isLeader) WaitFormation();
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
            case Task.CollectNatureFood:
            targetPos = pack.ChooseNaturalTarget();
            break;
            case Task.CollectFood:
            targetPos = pack.ChooseResourceTarget();
            break;
            case Task.CollectMaterial:
            Vector3 t = pack.ChooseMaterialTarget();
            if(t == new Vector3(0, -100, 0))
            {
                SetCurrentTask(Task.CollectNatureFood);
                return;
            }
            targetPos = t;
            break;
            case Task.RetrieveResourceHome:
            case Task.RetrieveResource:
            case Task.GoHome:
            case Task.ForceHome:
            case Task.HomeRest:
            if(isHome && CurrentTime == GameTime.Night)
            {
                HomeRest((EntityBase)pack.structures[0]);
            }
            else
            {
                followo = null;
                targetPos = pack.structures[0].GlobalPosition;
            }
            break;
            case Task.Explore:
            pack.wanderDir = steer.Wander();
            break;
            case Task.FollowExplore:
            Random rnd = new Random();
            float xOffset = (float)(rnd.NextDouble() * (statsSettings.maxFollowDistance - statsSettings.minFollowDistance) + statsSettings.minFollowDistance);
            float zOffset = (float)(rnd.NextDouble() * (statsSettings.maxFollowDistance - statsSettings.minFollowDistance) + statsSettings.minFollowDistance);
            if(followOffset == Vector3.Zero) followOffset = new Vector3(xOffset, 0, zOffset);
            followo = pack.leader;
            break;

        }

        if(!new List<Task>(){Task.Idle, Task.Runaway}.Contains(task)) mainTask = task;
        currentTask = task;
    }

    public override void IntoGroups()
    {
        var entityList = new List<Entity>(pack.entities).OrderByDescending(i => i.speed 
                                                        * ((float)i.currentHunger / i.statsSettings.maxHunger)).ToList();
        int i = 0;
        int nExplore = entityList.Count * 1 / 2;
        foreach(var group in pack.groups)
        {
            group.Value.Clear();
        }
    
        foreach(Chicpea chicpea in entityList.Cast<Chicpea>())
        {
            // 1/2 of the packs need to go for an exploration
            if(chicpea == pack.leader)
            {
                pack.groups[Task.Explore].Add(chicpea);
                i++;
            }
            else if(i < nExplore)
            {
                pack.groups[Task.Explore].Add(chicpea);
                i++;
            }
            else
            {
                pack.groups[Task.CollectFood].Add(chicpea);
            }
        }

        SetFormation();
        PlanTasks();
    }

    public override void PlanTasks()
    {
        for(int i = 0; i < pack.groups.Count; i++)
        {
            List<Entity> list = pack.groups.Values.ToList()[i];
            Task task = pack.groups.Keys.ToList()[i];
            foreach(Chicpea chicpea in list)
            {
                chicpea.tasks = new List<Task>(3){ Task.Idle, Task.Idle, Task.Idle };
                TaskByGroup(task, chicpea);
                chicpea.UpdateCurrentTask();
            }
        }
    }

    public override void TaskByGroup(Task task, Entity chicpea)
    {
        switch(task)
        {
            case Task.Explore:
            if(chicpea.isLeader)
            {
                GiveTask(chicpea, GameTime.Day, Task.Explore);
                GiveTask(chicpea, GameTime.Noon, Task.GoHome);
                GiveTask(chicpea, GameTime.Night, Task.HomeRest);
                //chicpea.WaitFormation();
            }
            else
            {
                GiveTask(chicpea, GameTime.Day, Task.FollowExplore);
                GiveTask(chicpea, GameTime.Noon, Task.GoHome);
                GiveTask(chicpea, GameTime.Night, Task.HomeRest);
            }
            break;
            case Task.CollectFood:
            GiveTask(chicpea, GameTime.Day, Task.CollectFood);
            GiveTask(chicpea, GameTime.Noon, Task.Continue);
            GiveTask(chicpea, GameTime.Night, Task.HomeRest);
            break;
        }
    }

    public override void GiveTask(Entity chicpea, GameTime time, Task task)
    {
        chicpea.tasks[(int)time] = task;
    }

    public override void TaskHandler()
    {
        switch(currentTask)
        {
            case Task.Runaway:
            Runaway();
            break;
            case Task.Explore:
            Wander();
            break;

            case Task.FollowExplore: 
            if(!GlobalPosition.IsEqualApprox(targetPos)) 
            {
                UpdateFollowPos();
                Chase();
            }
            break;

            case Task.CollectMaterial:
            case Task.CollectFood:
            case Task.CollectMaterialHome:
            case Task.RetrieveResource:
            case Task.RetrieveResourceHome:
            case Task.GoHome:
            case Task.ForceHome:
            case Task.HomeRest:
            Chase();
            break;
            
            case Task.Idle:
            break;
        }
    }

    #endregion

    #region Signals
    public override async void SelfEntered_Area(Area3D area3D)
    {
        Node3D body = (Node3D)area3D.Owner;
		if(body is MapSource)
		{
            MapSource mapSource = (MapSource)body;
            if(new List<Task>(){Task.CollectFood, Task.CollectMaterialHome, Task.CollectMaterial}.Contains(currentTask))
			{
                isBusy = true;
                int amount = 0;
                Task task = Task.Idle;
                if(currentTask == Task.CollectFood) task = Task.RetrieveResource;
                else if(new List<Task>(){Task.CollectMaterialHome, Task.CollectMaterial}.Contains(currentTask)) task = Task.RetrieveResourceHome;

                if(mapSource.GetCurrentResources() == 0)
                {
                    if(currentTask == Task.CollectFood)
                    { 
                        SetCurrentTask(Task.GoHome);
                        return;
                    }
                    else if(currentTask == Task.CollectMaterial)
                    {
                        SetCurrentTask(Task.CollectMaterial);
                        return;
                    }
                }
				else if(mapSource.GetCurrentResources() < statsSettings.collectableMax)
					amount = statsSettings.collectableMax - mapSource.GetCurrentResources();
				else 
					amount = statsSettings.collectableMax;

                SetCurrentTask(Task.Idle);  
				mapSource.CollectResource(amount);
                if(!GlobalTransform.Origin.IsEqualApprox(new Vector3(mapSource.GlobalPosition.X, GlobalPosition.Y, mapSource.GlobalPosition.Z)))
                    LookAt(mapSource.GlobalPosition);
                await ToSignal(GetTree().CreateTimer(mapSource.collectTime), "timeout");
                broughtType = mapSource.resourceType;
                broughtAmount = amount;
                CarryMaterial();
                SetCurrentTask(task);
                isBusy = false;
			}
        }
        else if(body is EntityBase)
        {
            EntityBase entityBase = (EntityBase)body;
            Task task = Task.Idle;
            if(entityBase != pack.structures[0]) return;
            isHome = true;
            if(currentTask == Task.HomeRest)
            {
                HomeRest(entityBase);
            }
            else if(new List<Task>(){ Task.RetrieveResource, Task.RetrieveResourceHome }.Contains(currentTask))
            {
                if(currentTask == Task.RetrieveResource) task = Task.CollectFood;
                else if(currentTask == Task.RetrieveResourceHome) task = Task.CollectMaterial;

                SetCurrentTask(Task.Idle);
                if(FoodMaterialType.Contains(broughtType))
                {
                    entityBase.ReceiveFood(broughtAmount);
                }
                else
                {
                    entityBase.ReceiveMaterial(broughtType, broughtAmount);
                }
                
                if(broughtAmount > 0) DropMaterial();
                if(savedTask != Task.Idle)
                {
                    SetCurrentTask(savedTask);
                    savedTask = Task.Idle;
                }
                else SetCurrentTask(task);
            }            
            else if(currentTask == Task.ForceHome)
            {
                pack.maxExploreDistance = 0;
                SetCurrentTask(Task.Idle);
                if(CurrentTime == GameTime.Day)
                {
                    if(isLeader) SetCurrentTask(Task.Explore);
                    else SetCurrentTask(Task.FollowExplore);
                }
                else if(CurrentTime == GameTime.Noon)
                {
                    SetCurrentTask(Task.CollectMaterial);
                }
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

    public override void VisionEntered_Area(Area3D area3D)
    {
        Node3D body = (Node3D)area3D.Owner;
		if(body is MapSource)
		{
			MapSource mapSource = (MapSource)body;
			// Separate between materials and foods
			if(mapSource.isFoodSource)
			{
				if(!pack.natureFoodSources.Keys.ToList().Contains(mapSource)) // Check if the list already contains
				{
					pack.natureFoodSources.Add(mapSource, pack.structures[0].GlobalPosition.DistanceTo(mapSource.GlobalPosition));
				}
                if(CurrentTime != GameTime.Night && currentTask == Task.GoHome && mapSource.GetCurrentResources() > 0)
                {
                    followo = null;
                    targetPos = mapSource.GlobalPosition;
                    SetCurrentTask(Task.CollectMaterialHome);
                }
			}
            else // Materials
            {
                if(!pack.materialSources.Keys.ToList().Contains(mapSource)) // Check if the list already contains
				{
					pack.materialSources.Add(mapSource, pack.structures[0].GlobalPosition.DistanceTo(mapSource.GlobalPosition));
				}
                if(CurrentTime != GameTime.Night && currentTask == Task.GoHome && mapSource.GetCurrentResources() > 0)
                {
                    followo = null;
                    targetPos = mapSource.GlobalPosition;
                    SetCurrentTask(Task.CollectMaterialHome);
                }
            }
		}
        
    }

    #endregion

    void SetFormation()
    {
        Random rnd = new Random();
        
        List<Vector3> dirs = new List<Vector3>();
        for(int i = 0; i < statsSettings.nRaycasts * 2; i++)
		{
			float angle = (float)(i * Math.PI * 2 / statsSettings.nRaycasts);
			dirs.Add(Vector3.Right.Rotated(Vector3.Up, angle));
        }
        List<Entity> chicpeas = pack.groups[Task.Explore];
        for(int i = 1; i < chicpeas.Count; i++)
        {
            float xOffset = (float)(rnd.NextDouble() * (statsSettings.maxFollowDistance - statsSettings.minFollowDistance) 
                + statsSettings.minFollowDistance);
            float zOffset = (float)(rnd.NextDouble() * (statsSettings.maxFollowDistance - statsSettings.minFollowDistance) 
                + statsSettings.minFollowDistance);
            Vector3 dir = dirs[rnd.Next(dirs.Count)];
            dirs.Remove(dir);
            chicpeas[i].followOffset = dir.Normalized() * new Vector3(xOffset, 0, zOffset);
        }
    }

    public void HomeRest(EntityBase entityBase)
    {
        // Disappearing from screen
        if(broughtAmount > 0)
        {
            if(FoodMaterialType.Contains(broughtType))
            {
                entityBase.ReceiveFood(broughtAmount);
            }
            else
            {
                entityBase.ReceiveMaterial(broughtType, broughtAmount);
            }
            DropMaterial();
        }
        GetNode<CollisionShape3D>("BodyCollision").SetDeferred("disabled", true);
        ConsumeFood(); // Handle Eating
        EstrousPregnancy(); // Handle Estrous Cycle
        if(isKid) GrowUp();
        
        SetCurrentTask(Task.Idle);
        Visible = false;
    }

    public override void GiveBirth()
    {
        PackedScene packedScene = EntitySceneDictionary[Entities.Chicpea];
        Chicpea chicpea = (Chicpea)packedScene.Instantiate();
        isPregnant = false;
        GetParent().AddChild(chicpea);
        chicpea.GlobalPosition = GlobalPosition;
        chicpea.Scale = new Vector3(statsSettings.kidSize, statsSettings.kidSize, statsSettings.kidSize);
        chicpea.isKid = true;
        chicpea.Visible = false;
        chicpea.GetNode<CollisionShape3D>("BodyCollision").SetDeferred("disabled", true);
        pack.entities.Add(chicpea);
        chicpea.pack = pack;
    }
}
