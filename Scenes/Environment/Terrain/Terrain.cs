using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Resources;

public partial class Terrain : Node3D
{
	[Export] Vector3 meshSize;

	const float OBJECT_OFFSET = 0.5f;

	int objectRate;
	Random rnd;
	Node3D objectList;
	PackedScene movableGrassScene = (PackedScene)ResourceLoader.Load(MOVABLEGRASS_SCENE);
	public TerrainType terrainType;
	
	Godot.Collections.Dictionary<string, int> objectTypes = new Godot.Collections.Dictionary<string, int>(){};
	List<int> rates = new List<int>();

	public override void _Ready()
	{
		objectList = GetNode<Node3D>("ObjectList");
		rnd = new Random();
		GenerateObjectWeight();
		rates = GenerateObjectRate();
		GenerateObject();
	}

	void GenerateObjectWeight()
	{
		switch(terrainType)
		{
			case TerrainType.Floor:
			objectRate = 5;
			objectTypes.Add(MOVABLEGRASS_SCENE, 1);
			//objectTypes.Add(TerrainType.ChicpeaBase, 10);
			break;
			case TerrainType.Trees:
			objectRate = 5;
			objectTypes.Add(TREE_SCENE, 1);
			break;
		}
	}

	List<int> GenerateObjectRate()
	{
		List<int> rates = new List<int>();
		int temp = 0;
		rates.Add(temp);
		foreach(var obj in objectTypes)
		{
			temp += obj.Value;
			rates.Add(temp);
		}
		return rates;
	}

	void GenerateObject()
	{
        for (int i = 0; i < TILE_SIZE; i++)
			for(int j = 0; j < TILE_SIZE; j++)
			{
                int random = rnd.Next(100);
                if (random < objectRate) // having object
				{
					PackedScene packedScene = (PackedScene)ResourceLoader.Load(RollObject());
					Object obj = (Object)packedScene.Instantiate();
					if(obj.isBlock)
						if(!IsNotSurrounded(new Vector2I(i, j))) continue;
					obj.Position = new Vector3(i - (TILE_SIZE - 2) / 2 - OBJECT_OFFSET, 0.5f, 
												j - (TILE_SIZE - 2) / 2 - OBJECT_OFFSET);
					obj.Name = i.ToString() + "," + j.ToString();
					objectList.AddChild(obj);
					continue;
				}
			}
	}

	string RollObject()
	{
		GD.Print(rates[rates.Count - 1]);
		int random = rnd.Next(rates[rates.Count - 1]);
		int result = 0;

		for(int i = 1; i < rates.Count; i++)
		{
			if(random >= rates[i - 1] && random < rates[i])
			{
				result = i - 1;
				break;
			}
		}
		
		return objectTypes.Keys.ElementAt(result);
	}

	void GenerateEntity()
	{
		
	}

	bool IsNotSurrounded(Vector2I o)
	{
		for(int i = (int)(o.X - 1); i < o.X + 2; i++)
			for(int j = (int)(o.Y - 1); j < o.Y + 2; j++)
			{
				if (objectList.HasNode(i.ToString() + "," + j.ToString())) return false;
			}
		return true;
	}
}