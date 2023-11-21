using System.Collections.Generic;
using Godot;

public partial class Pack : Node
{
    const int nDirections = 16;
	List<Entity> entities = new List<Entity>();
    Dictionary<Object, float> foodSources = new Dictionary<Object, float>()
    {

    };
    Dictionary<Vector3, float> directionPoints = new Dictionary<Vector3, float>()
    {

    };
    
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
}
