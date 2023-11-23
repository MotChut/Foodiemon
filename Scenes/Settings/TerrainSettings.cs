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
	public int mound = 0;
	public List<int> rates = new List<int>();

	public void Init()
	{
		GetRate();
	}

	List<int> GetWeights()
	{
		List<int> weights = new List<int>
        {
            movableGrass, tree, berryBush, mound
        };
		return weights;
	}

	void GetRate()
	{
		List<int> weights = GetWeights();
		int temp = 0;
		rates.Add(temp);
		foreach(int w in weights)
		{
			temp += w;
			rates.Add(temp);
		}
	}
}
