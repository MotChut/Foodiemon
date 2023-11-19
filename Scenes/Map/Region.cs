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

	List<Vector2> terrains = new List<Vector2>();
	List<Neighbor> neighbors = new List<Neighbor>();
	Region nextRegion;
	RegionType regionType;
	ProceduralGeneration proGen;
	PackedScene packedScene;

	public Region(ProceduralGeneration proGen, RegionType regionType)
	{
		this.proGen = proGen;
		this.regionType = regionType;
		CreatePackedScene();
		GenerateTerrainWeight();
	}

	void CreatePackedScene()
	{
		switch(regionType)
		{
			case RegionType.ForestRegion:
			packedScene = (PackedScene)ResourceLoader.Load(ForestRegion_Scene);
			break;
		}
	}

	void GenerateTerrainWeight()
	{
		switch(regionType)
		{
			case RegionType.ForestRegion:
			terrainWeights.Add(TerrainType.Floor, 2);
			terrainWeights.Add(TerrainType.Trees, 1);
			//terrainWeights.Add(TerrainType.ChicpeaBase, 10);
			break;
		}
	}

	List<int> GenerateTerrainRate()
	{
		List<int> rates = new List<int>();
		int temp = 0;
		rates.Add(temp);
		foreach(var terrain in terrainWeights)
		{
			temp += terrain.Value;
			rates.Add(temp);
		}
		return rates;
	}

	public void GenerateTerrain()
	{
		foreach(Vector2 cell in terrains)
		{
			Terrain terrain = (Terrain)packedScene.Instantiate();
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
		List<int> rates = GenerateTerrainRate();
		Random rnd = new Random();
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
		
		return terrainWeights.Keys.ElementAt(result);
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
