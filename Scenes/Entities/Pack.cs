using System.Collections.Generic;
using Godot;

public partial class Pack : Node
{
    const int nDirections = 16;
	public List<Object> structures = new List<Object>();
    public Dictionary<Entity, float> entities = new Dictionary<Entity, float>();
    public Dictionary<MapSource, float> foodSources = new Dictionary<MapSource, float>();
    public Dictionary<Vector3, float> directionPoints = new Dictionary<Vector3, float>();
    public Entity leader = null;
    
    public Pack()
    {
        GenerateDirections();
    }

    void GenerateDirections()
    {
        for(int i = 0; i < nDirections; i++)
		{
			float angle = (float)(i * Mathf.Pi / nDirections);
			directionPoints.Add(Vector3.Right.Rotated(Vector3.Up, angle), 0);
        }
    }

    public void UpdateDirectionPoint(Vector3 dir, float point)
    {
        
    }
}
