using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Resources;
using static Utils;

public partial class Terrain : Node3D
{
	[Export] Vector3 meshSize;

	const float OBJECT_OFFSET = 0.5f;

	int objectRate;
	Random rnd;
	Node3D objectList;
	public TerrainType terrainType;
	TerrainSettings terrainSettings;
	EntitySettings entitySettings;
	
	Godot.Collections.Dictionary<string, int> objectTypes = new Godot.Collections.Dictionary<string, int>(){};
	List<int> rates = new List<int>();

	public override void _Ready()
	{
		objectList = GetNode<Node3D>("ObjectList");
		rnd = new Random();
		terrainSettings = TerrainSettingsList[(int)terrainType];
		// Check if the terrain spawns entities instead of objects
		if(EntitiesTerrainType.Contains(terrainType))
		{
			entitySettings = EntitySettingsList[EntitiesTerrainType.IndexOf(terrainType)];
			GenerateEntityBase();
		}

		GenerateObject();
	}

	#region Objects

	void GenerateObject()
	{
        for (int i = 0; i < TILE_SIZE; i++)
			for(int j = 0; j < TILE_SIZE; j++)
			{
                int random = rnd.Next(100);
                if (random < terrainSettings.objectRate) // having object
				{
					PackedScene packedScene = RollObject();
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

	PackedScene RollObject()
	{
		int random = rnd.Next(terrainSettings.rates[terrainSettings.rates.Count - 1]);
		int result = 0;

		for(int i = 1; i < terrainSettings.rates.Count; i++)
		{
			if(random >= terrainSettings.rates[i - 1] && random < terrainSettings.rates[i])
			{
				result = i - 1;
				break;
			}
		}
		
		return SceneDictionary.Values.ElementAt(result);
	}


	#endregion

	void GenerateEntityBase()
	{
		GenerateHouse();
		GenerateSources();
		GenerateEntities();
	}

	void GenerateHouse()
	{
		int x =  rnd.Next(TILE_SIZE / 4, TILE_SIZE * 3 / 4);
		int z =  rnd.Next(TILE_SIZE / 4, TILE_SIZE * 3 / 4);
		PackedScene packedScene = entitySettings.houseScene;
		Object house = (Object)packedScene.Instantiate();
		house.Position = new Vector3(x - (TILE_SIZE - 2) / 2 - OBJECT_OFFSET, 0.5f, 
									z - (TILE_SIZE - 2) / 2 - OBJECT_OFFSET);
		house.Name = x.ToString() + "," + z.ToString();
		objectList.AddChild(house);
	}

	void GenerateSources()
	{
		int nFoodSource = rnd.Next(entitySettings.minFoodSource, entitySettings.maxFoodSource);
		while(nFoodSource > 0)
		{
			int x =  rnd.Next(TILE_SIZE);
			int z =  rnd.Next(TILE_SIZE);
			if (objectList.HasNode(x.ToString() + "," + z.ToString())) continue;
			else
			{
				PackedScene packedScene = entitySettings.foodSourceScene;
				Object foodSource = (Object)packedScene.Instantiate();
				foodSource.Position = new Vector3(x - (TILE_SIZE - 2) / 2 - OBJECT_OFFSET, 0.5f, 
									z - (TILE_SIZE - 2) / 2 - OBJECT_OFFSET);
				foodSource.Name = x.ToString() + "," + z.ToString();
				objectList.AddChild(foodSource);
				nFoodSource -= 1;
			}
		}
	}

	void GenerateEntities()
	{
		// Spawn Leader
		while(true)
		{
			// Check if position is available
			int x =  rnd.Next(TILE_SIZE);
			int z =  rnd.Next(TILE_SIZE);
			Node node = objectList.GetNodeOrNull(x.ToString() + "," + z.ToString());
			if(node != null) if((node as Object).isBlock) continue;
			else continue;

			// Spawn if possible
			PackedScene packedScene = entitySettings.leaderScene;
			Entity leader = (Entity)packedScene.Instantiate();
			leader.Position = new Vector3(x - (TILE_SIZE - 2) / 2 - OBJECT_OFFSET, 0.5f, 
								z - (TILE_SIZE - 2) / 2 - OBJECT_OFFSET);
			//leader.Name = x.ToString() + "," + z.ToString();
			objectList.AddChild(leader);
			break;
		}
		
		int nEntities = rnd.Next(entitySettings.minEntities, entitySettings.maxEntities);
		while(nEntities > 0)
		{
			// Check if position is available
			int x =  rnd.Next(TILE_SIZE);
			int z =  rnd.Next(TILE_SIZE);
			Node node = objectList.GetNodeOrNull(x.ToString() + "," + z.ToString());
			if(node != null) if((node as Object).isBlock) continue;
			else continue;
	
			// Spawn if possible
			PackedScene packedScene = entitySettings.entityScene;
			Entity entity = (Entity)packedScene.Instantiate();
			entity.Position = new Vector3(x - (TILE_SIZE - 2) / 2 - OBJECT_OFFSET, 0.5f, 
								z - (TILE_SIZE - 2) / 2 - OBJECT_OFFSET);
			//entity.Name = x.ToString() + "," + z.ToString();
			objectList.AddChild(entity);
			nEntities -= 1;
		}
	}

	bool IsNotSurrounded(Vector2I o)
	{
		for(int i = o.X - 1; i < o.X + 2; i++)
			for(int j = o.Y - 1; j < o.Y + 2; j++)
			{
				if (objectList.HasNode(i.ToString() + "," + j.ToString())) return false;
			}
		return true;
	}
}