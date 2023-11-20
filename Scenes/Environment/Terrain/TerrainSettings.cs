using System.Collections.Generic;
using Godot;

public partial class TerrainSettings : Node
{
	public int id = 0;
	public string terrainType = "";
	public int objectRate = 0;
	public int movableGrass = 0;
	public int tree = 0;
	public int berryBush = 0;
	public List<int> weights = new List<int>();
	public List<int> rates = new List<int>();

	public void GetWeights()
	{
		weights.Add(movableGrass);
		weights.Add(tree);
		weights.Add(berryBush);
	}

	public void GetRate()
	{
		int temp = 0;
		rates.Add(temp);
		foreach(int w in weights)
		{
			temp += w;
			rates.Add(temp);
		}
	}
}
