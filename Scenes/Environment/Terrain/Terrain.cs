using Godot;
using System;
using System.Linq;
using static Resources;

public partial class Terrain : Node3D
{
	[Export] Vector3 meshSize;

	int objectRate;
	Random rnd;
	Node3D objectList;
	public TerrainType terrainType;
	TerrainSettings terrainSettings;
	EntitySettings entitySettings;
	
	Godot.Collections.Dictionary<string, int> objectTypes = new Godot.Collections.Dictionary<string, int>(){};
	int[,] matrix = new int[TILE_SIZE, TILE_SIZE];

	public override void _Ready()
	{
		objectList = GetNode<Node3D>("ObjectList");
		rnd = new Random();
		terrainSettings = TerrainSettingsList[(int)terrainType];
		// Check if the terrain spawns entities instead of objects
		if(EntitiesTerrainType.Contains(terrainType))
		{
			entitySettings = EntitySettingsList[EntitiesTerrainType.IndexOf(terrainType)];
			GenerateEntityBase(terrainType);
		}

		GenerateObject();
	}

	#region Objects

	void GenerateMatrix()
	{
		for(int i = 0; i < TILE_SIZE; i++)
			for(int j = 0; j < TILE_SIZE; j++)
				matrix[i, j] = 0;
	}

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
					// Check if the position is suitable
					if(!IsAvailableCell(new Vector2I(i, j), new Vector2(obj.objectSize.X, obj.objectSize.Z))) continue;
					if(obj.isBlock)
						if(!IsNotSurrounded(new Vector2I(i, j), new Vector2(obj.objectSize.X, obj.objectSize.Z))) continue;
					obj.Position = new Vector3(i - (TILE_SIZE - 1) / 2.0f, 0.50001f, 
												j - (TILE_SIZE - 1) / 2.0f);
					obj.Name = i.ToString() + "," + j.ToString();
					if(obj is MapSource) obj.GetNode<Node3D>("Sprite").RotationDegrees = new Vector3(0, rnd.Next(360), 0);
					else obj.RotationDegrees = new Vector3(0, rnd.Next(360), 0);
					objectList.AddChild(obj);
					MarkCells(new Vector2I(i, j), new Vector2(obj.objectSize.X, obj.objectSize.Z));
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
		
		return ObjectSceneDictionary.Values.ElementAt(result);
	}


	#endregion

	void GenerateEntityBase(TerrainType type)
	{
		Pack pack = null;
		switch(type)
		{
			case TerrainType.ChicpeaBase:
			pack = new ChicpeaPack();
			break;
		}
		
		PackList.Add(pack);
		GenerateHouse(pack);
		GenerateSources(pack);
		GenerateEntities(pack);
	}

	void GenerateHouse(Pack pack) // Generate first so required no checking
	{
		int x =  rnd.Next(TILE_SIZE / 3, TILE_SIZE * 2 / 3);
		int z =  rnd.Next(TILE_SIZE / 3, TILE_SIZE * 2 / 3);
		PackedScene packedScene = entitySettings.houseScene;
		EntityBase house = (EntityBase)packedScene.Instantiate();
		house.Position = new Vector3(x - (TILE_SIZE - 1) / 2.0f, 0.5f, 
									z - (TILE_SIZE - 1) / 2.0f);
		house.Name = x.ToString() + "," + z.ToString();
		house.pack = pack;
		objectList.AddChild(house);
		MarkCells(new Vector2I(x, z), new Vector2(house.objectSize.X, house.objectSize.Z), house.extend);
		pack.structures.Add(house);
	}

	void GenerateSources(Pack pack) // Generate second so required no break when not finding possible cell
	{
		int nFoodSource = rnd.Next(entitySettings.minFoodSource, entitySettings.maxFoodSource + 1);
		while(nFoodSource > 0)
		{
			int x =  rnd.Next(TILE_SIZE);
			int z =  rnd.Next(TILE_SIZE);

			PackedScene packedScene = entitySettings.foodSourceScene;
			Object foodSource = (Object)packedScene.Instantiate();
			if(!IsAvailableCell(new Vector2I(x, z), new Vector2(foodSource.objectSize.X, foodSource.objectSize.Z))) continue;
			foodSource.Position = new Vector3(x - (TILE_SIZE - 1) / 2.0f, 0.5f, 
								z - (TILE_SIZE - 1) / 2.0f);
			foodSource.Name = x.ToString() + "," + z.ToString();
			foodSource.GetNode<Sprite3D>("InteractiveNotice").RotationDegrees = new Vector3(0, rnd.Next(360), 0);
			objectList.AddChild(foodSource);
			MarkCells(new Vector2I(x, z), new Vector2(foodSource.objectSize.X, foodSource.objectSize.Z));
			pack.foodSources.Add((MapSource)foodSource);
			nFoodSource -= 1;
		}
	}

	void GenerateEntities(Pack pack)
	{
		// Spawn Leader
		while(true)
		{
			// Check if position is available
			int x =  rnd.Next(TILE_SIZE);
			int z =  rnd.Next(TILE_SIZE);
			// Node node = objectList.GetNodeOrNull(x.ToString() + "," + z.ToString());
			// if(node != null) if((node as Object).isBlock) continue;
			// else continue;
			
			// Spawn if possible
			PackedScene packedScene = entitySettings.leaderScene;
			Entity leader = (Entity)packedScene.Instantiate();

			if(!IsAvailableCell(new Vector2I(x, z), new Vector2(leader.objectSize.X, leader.objectSize.Z))) continue;

			//leader.Name = x.ToString() + "," + z.ToString();
			proGen.entityNode.AddChild(leader);
			leader.GlobalPosition = GlobalPosition + new Vector3(x - (TILE_SIZE - 1) / 2.0f, 0.5f, 
								z - (TILE_SIZE - 1) / 2.0f);
			MarkCells(new Vector2I(x, z), new Vector2(leader.objectSize.X, leader.objectSize.Z));
			pack.entities.Add(leader);
			pack.leader = leader;
			leader.pack = pack;
			break;
		}
		
		int nEntities = rnd.Next(entitySettings.minEntities, entitySettings.maxEntities + 1);
		while(nEntities > 0)
		{
			// Check if position is available
			int x =  rnd.Next(TILE_SIZE);
			int z =  rnd.Next(TILE_SIZE);
			// Node node = objectList.GetNodeOrNull(x.ToString() + "," + z.ToString());
			// if(node != null) if((node as Object).isBlock) continue;
			// else continue;
	
			// Spawn if possible
			PackedScene packedScene = entitySettings.entityScene;
			Entity entity = (Entity)packedScene.Instantiate();

			if(!IsAvailableCell(new Vector2I(x, z), new Vector2(entity.objectSize.X, entity.objectSize.Z))) continue;

			//entity.Name = x.ToString() + "," + z.ToString();
			proGen.entityNode.AddChild(entity);
			entity.GlobalPosition = GlobalPosition + new Vector3(x - (TILE_SIZE - 1) / 2.0f, 0.5f, 
								z - (TILE_SIZE - 1) / 2.0f);
			MarkCells(new Vector2I(x, z), new Vector2(entity.objectSize.X, entity.objectSize.Z));
			pack.entities.Add(entity);
			entity.pack = pack;
			nEntities -= 1;
		}
	}

	bool IsNotSurrounded(Vector2I o, Vector2 size)
	{
		for(int i = o.X - 1; i < o.X + 2 + size.X; i++)
			for(int j = o.Y - 1; j < o.Y + 2 + size.Y; j++)
			{
				if (i >= TILE_SIZE - 1 || j >= TILE_SIZE - 1 
				|| i < 0 || j < 0
				|| matrix[i, j] == 1) return false;
			}
		return true;
	}

	bool IsAvailableCell(Vector2I pos, Vector2I size)
	{
		bool result = true;
		for(int i = pos.X; i < pos.X + size.X; i++)
			for(int j = pos.Y; j < pos.Y + size.Y; j++)
				if(matrix[i, j] == 1) result = false;
		return result;
	}

	bool IsAvailableCell(Vector2I pos, Vector2 size)
	{	
		for(int x = pos.X; x < pos.X + size.X; x++)
			for(int z = pos.Y; z < pos.Y + size.Y; z++)
				if(x >= TILE_SIZE - 1 || z >= TILE_SIZE - 1 || matrix[x, z] == 1)
					return false;
		return true;
	}

	void MarkCells(Vector2I pos, Vector2 size, int extend = 0)
	{
		for(int x = pos.X - extend; x < pos.X + size.X + extend; x++)
			for(int z = pos.Y - extend; z < pos.Y + size.Y + extend; z++)
				matrix[x, z] = 1;
	}
}