using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Pack : Node
{
    const int nDirections = 32;
	public List<Object> structures = new List<Object>();
    public Dictionary<Entity, float> entities = new Dictionary<Entity, float>();
    public Dictionary<MapSource, float> foodSources = new Dictionary<MapSource, float>();
    public Dictionary<Vector3, float> directionPoints = new Dictionary<Vector3, float>();
    public List<int> directionRates = new List<int>();
    public Entity leader = null;
    
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
}
