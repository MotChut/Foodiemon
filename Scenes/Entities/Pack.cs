using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static Entity;

public partial class Pack : Node
{
	bool justSpawn = true;
	public float maxExploreDistance = 0;
    const int nDirections = 16;
	public List<Object> structures = new List<Object>();
    public List<Entity> entities = new List<Entity>();
    public List<Entity> isHomeEnetities = new List<Entity>();
    public Dictionary<Task, List<Entity>> groups = new Dictionary<Task, List<Entity>>();
    public List<MapSource> foodSources = new List<MapSource>();
	public List<MapSource> materialSources = new List<MapSource>();
    public Dictionary<MapSource, float> natureFoodSources = new Dictionary<MapSource, float>();
    public Dictionary<Vector3, float> directionPoints = new Dictionary<Vector3, float>();
    public List<int> directionRates = new List<int>();
    public Entity leader = null;
	public Entity exploreLeader;
	public Vector3 wanderDir = Vector3.Zero;

	public int foods = 0;

    
    public Pack()
    {
        GenerateDirections();
        GetRate();
    }

    void GenerateDirections()
    {
        for(int i = 0; i < nDirections; i++)
		{
			float angle = (float)(i * Mathf.Pi / nDirections);
			directionPoints.Add(Vector3.Right.Rotated(Vector3.Up, angle), 1);
        }
    }

    public void UpdateDirectionPoint(Vector3 dir, float point)
    {
        directionPoints[dir] = point;
        directionRates = new List<int>();
        GetRate();
    }

	void GetRate()
	{
		int temp = 0;
		directionRates.Add(temp);
		foreach(int w in new List<float>(directionPoints.Values))
		{
			temp += w;
			directionRates.Add(temp);
		}
	}

    public Vector3 ChooseWanderDirection()
	{
        Random rnd = new Random();
		int random = rnd.Next(directionRates[directionRates.Count - 1]);
		int result = 0;

		for(int i = 1; i < directionRates.Count; i++)
		{
			if(random >= directionRates[i - 1] && random < directionRates[i])
			{
				result = i - 1;
				break;
			}
		}
		
		return directionPoints.Keys.ToList<Vector3>()[result];
	}

	public Vector3 ChooseResourceTarget()
	{
		List<MapSource> availableSources = new List<MapSource>();
		foreach(MapSource mapSource in foodSources)
		{
			if(mapSource.GetCurrentResources() > 0)
			{
				availableSources.Add(mapSource);
			}
		}
		Random rnd = new Random();
		return availableSources[rnd.Next(availableSources.Count)].GlobalPosition;
	}

	public void PackConsumeFoods()
	{
		if(justSpawn)
		{
			justSpawn = false;
			return;
		}
		foreach(Entity entity in entities)
        {
			if(entity.statsSettings.nConsume > ((EntityBase)entity.pack.structures[0]).foods)
			{
				entity.Starve();
			}
			else
			{
				if(entity.currentHunger < entity.statsSettings.maxHunger) entity.currentHunger += 1;
				((EntityBase)structures[0]).foods -= entity.statsSettings.nConsume;
			}
        }
	}
}
