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
	public int rock1 = 0;
	public int grassBush = 0;
	public int flint = 0;
	public int grassSource = 0;
	public int tree2 = 0;
	public int dune = 0;
	public int grass2 = 0;
	public List<int> rates = new List<int>();

	public void Init()
	{
		GetRate();
	}

	List<int> GetWeights()
	{
		List<int> weights = new List<int>
        {
            movableGrass, tree, berryBush, rock1, grassBush, flint, grassSource,
			tree2, dune, grass2
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
