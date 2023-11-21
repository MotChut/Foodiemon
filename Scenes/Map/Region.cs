using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Resources;

public partial class Region : Node
{
	public struct Neighbor
	{
		public Region region;
		public Vector2 dir;

		public Neighbor(Region r, Vector2 d)
		{
			region = r;
			dir = d;
		}
	}
	const int MAX_NEIGHBORS = 3;

	Godot.Collections.Dictionary<TerrainType, int> terrainWeights = new Godot.Collections.Dictionary<TerrainType, int>(){};

	Random rnd;
	List<Vector2> terrains = new List<Vector2>();
	List<Neighbor> neighbors = new List<Neighbor>();
	Region nextRegion;
	RegionType regionType;
	PackedScene packedScene;
	RegionSettings regionSettings;

	public Region(RegionType regionType)
	{
		this.regionType = regionType;
		regionSettings = RegionSettingsList[(int)regionType];
		rnd = new Random();
	}

	public void GenerateTerrain()
	{
		foreach(Vector2 cell in terrains)
		{
			Terrain terrain = (Terrain)regionSettings.terrain.Instantiate();
			terrain.terrainType = RollTerrain();
			terrain.Position = new Vector3(cell.X * TILE_SIZE, 0, cell.Y * TILE_SIZE);
			terrain.Name = cell.X.ToString() + "," + cell.Y.ToString();
			proGen.terrainNode.AddChild(terrain);

			VisibleOnScreenNotifier3D visibleOnScreenNotifier3D = new VisibleOnScreenNotifier3D
			{
				Aabb = new Aabb(new Vector3(TILE_SIZE / -2, -0.5f, TILE_SIZE / -2), new Vector3(TILE_SIZE, 1, TILE_SIZE)),
				Position = terrain.Position
			};
			visibleOnScreenNotifier3D.Connect("screen_entered", new Callable(terrain, "show"));
			visibleOnScreenNotifier3D.Connect("screen_exited", new Callable(terrain, "hide"));
			proGen.visiblerNode.AddChild(visibleOnScreenNotifier3D);
		}
	}
	
	TerrainType RollTerrain()
	{
		int result = 0;
		while(true)
		{
			int random = rnd.Next(regionSettings.rates[regionSettings.rates.Count - 1]);

			for(int i = 1; i < regionSettings.rates.Count; i++)
			{
				if(random >= regionSettings.rates[i - 1] && random < regionSettings.rates[i]) // Suitable Rate
				{
					int index = EntitiesTerrainType.IndexOf((TerrainType)(i - 1));
					if(EntitiesTerrainType.Contains((TerrainType)(i - 1)))
					{
						if(regionSettings.distanceCount < regionSettings.averageDistance && regionSettings.firstSpawn) break;
						regionSettings.firstSpawn = true;
						regionSettings.distanceCount = 0;
						bool r = regionSettings.SetTypeControlByIndex(index, regionSettings.entityTypesControl[index] + 1);
						if(!r) break;
					}
					
					result = i - 1;
					break;
				}
			}

			if(result != 0) break;
		}
		regionSettings.distanceCount++;
		return (TerrainType)result;
	}

	#region Utils
	public void AddCell(Vector2 cell)
	{
		terrains.Add(cell);
	}

	public Vector2 GetFirstCell()
	{
		return terrains[0];
	}

	public List<Vector2> GetAllCells()
	{
		return terrains;
	}

	public bool CanBeAdded()
	{
		return neighbors.Count < MAX_NEIGHBORS;
	}

	public void AddNeighbor(Region region, Vector2 dir)
	{
		neighbors.Add(new Neighbor(region, dir));
	}

	public bool HasNeighbor()
	{
		return neighbors.Count > 0 ? true : false;
	}

	public List<Neighbor> GetNeighbors()
	{
		return neighbors;
	}

	public Vector2 GetRandomCell()
	{
		Random rnd = new Random();
		return terrains[rnd.Next(terrains.Count)];
	}

	public bool HasCell(Vector2 cell)
	{
		return terrains.Contains(cell);
	}

	#endregion
}
