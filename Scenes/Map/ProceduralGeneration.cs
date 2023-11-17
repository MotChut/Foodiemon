using Godot;
using static Resources;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;

public partial class ProceduralGeneration : Node3D
{
	[Export] int CAMERA_SPEED = 400;

	#region Constants
	const int MIN_REGION_SIZE = 100;
	const int MAX_REGION_SIZE = 200;
	const int MIN_DISTANCE = 1;
	const int MAX_DISTANCE = 4;
	List<Vector2> availableDirection4 = new List<Vector2>() 
	{
		new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1)
	};

	List<Vector2> availableDirection8 = new List<Vector2>() 
	{
		new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1),
		new Vector2(-1, -1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(1, 1)
	};

	#endregion

	#region Variables
	// int noiseDensity = 42;
	// int iterations = 8;
	Random rnd;
	[Export] int nRegion = 7;

	List<Region> regions = new List<Region>();

	#endregion

	#region Nodes
	PackedScene floorScene = (PackedScene)ResourceLoader.Load(FLOOR_SCENE);
	Node3D terrainNode;
	Node3D visiblerNode;
	Node3D camera;

	#endregion

	public override void _Ready()
	{
		terrainNode = GetNode<Node3D>("Terrain");
		visiblerNode = GetNode<Node3D>("Visibler");
		//camera = GetNode<Node3D>("Pivot");

		rnd = new Random();
		GeneratePseudoTree();
		// GenerateNoiseGrid();
		// CellularAutomata();
		// GenerateTerrain();
	}

    public override void _PhysicsProcess(double delta)
    {
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if(direction != Vector3.Zero)
		{
			//camera.Position += new Vector3((float)(direction.X * CAMERA_SPEED * delta), 0, (float)(direction.Z * CAMERA_SPEED * delta)).Rotated(Vector3.Up, camera.Rotation.Y);
		}

		if(Input.IsActionJustPressed("reset"))
		{
			GetTree().ReloadCurrentScene();
		}
    }

	#region Procedural Generation

	void GeneratePseudoTree()
	{
		List<Vector2> tree = new List<Vector2>();
	
		Region region = new Region(RegionType.Forest);
		tree.Add(Vector2.Zero);
		regions.Add(region);

		for(int r = 0; r < nRegion - 1; r++)
		{
			while(true)
			{
				Vector2 selectedCell = tree[rnd.Next(tree.Count)];
				if (!regions[tree.IndexOf(selectedCell)].CanBeAdded()) continue;

				Vector2 newCell = GetPossibleCell(availableDirection8, selectedCell);
				if(newCell != new Vector2(-1, -1))
				{
					tree.Add(newCell);
					Region newRegion = new Region(RegionType.Forest);
					regions[tree.IndexOf(selectedCell)].AddNeighbor(newRegion, newCell - selectedCell);
					regions.Add(newRegion);
					break;
				}
			}
		}
		GeneratePseudoMap();
	}
	
	void GeneratePseudoMap()
	{
		for(int i = 0; i < nRegion; i++)
		{
			int regionSize = rnd.Next(MIN_REGION_SIZE, MAX_REGION_SIZE);
			Vector2 firstCell;

			// Generate the root of pseudo tree
			if(i == 0)
			{
				firstCell = new Vector2(0, 0);
				regions[i].AddCell(firstCell);
				GeneratePseudoRegion(regions.IndexOf(regions[i]), regionSize);
				ExpandRegionNeighbor(i);
			}
			else if(regions[i].HasNeighbor())
			{
				ExpandRegionNeighbor(i);
			}
		}

		GenerateMap();	
	}

	void GeneratePseudoRegion(int index, int regionSize)
	{
		for(int i = 0; i < regionSize; i++)
		{
			Vector2 newCell = GetPossibleCell(availableDirection4, regions[index].GetRandomCell());
			if(newCell != new Vector2(-1, -1))
			{
				regions[index].AddCell(newCell);
			}
		}
	}

	void ExpandRegionNeighbor(int index)
	{
		List<Region.Neighbor> neighbors = regions[index].GetNeighbors();
		for(int i = 0; i < neighbors.Count; i++)
		{
			int regionSize = rnd.Next(MIN_REGION_SIZE, MAX_REGION_SIZE);
			Vector2 firstCell = FindNeighborFirstCell(regions[index].GetFirstCell(), neighbors[i].dir);
			regions[regions.IndexOf(neighbors[i].region)].AddCell(firstCell); // Add neighbor's first cell
			ConnectTwoPoints(regions[index].GetFirstCell(), regions[regions.IndexOf(neighbors[i].region)].GetFirstCell(), regions.IndexOf(neighbors[i].region));
			GeneratePseudoRegion(regions.IndexOf(neighbors[i].region), regionSize);
		}
	}

	void ConnectTwoPoints(Vector2 point1, Vector2 point2, int index)
	{
		Vector2 currentPos = point1;
		do
		{
			if (currentPos.X < point2.X) currentPos.X += 1;
			if (currentPos.Y < point2.Y) currentPos.Y += 1;
			if (currentPos.X > point2.X) currentPos.X -= 1;
			if (currentPos.Y > point2.Y) currentPos.Y -= 1;
			
			if (IsAvailableCell(currentPos))
			{
				regions[index].AddCell(currentPos);
			}
		} while (currentPos != point2);
	}

	Vector2 GetPossibleCell(List<Vector2> dirs, Vector2 cell)
	{
		List<Vector2> availableDirection = new List<Vector2>(dirs);
		while(availableDirection.Count != 0)
		{
			Vector2 dir = availableDirection[rnd.Next(availableDirection.Count)];
			Vector2 newCell = cell + dir;
			
			if(IsAvailableCell(newCell)) 
			{
				return newCell;
			}
			else availableDirection.Remove(dir);
		}
		return new Vector2(-1, -1);
	}

	bool IsAvailableCell(Vector2 cell)
	{
		foreach(Region region in regions)
		{
			if (region.HasCell(cell)) return false;
		}
		return true;
	}

	void GenerateMap()
	{
		foreach(Region region in regions)
		{
			List<Vector2> list = region.GetAllCells();
			foreach(Vector2 cell in list)
			{
				Terrain terrain = (Terrain)floorScene.Instantiate();
				terrain.Position = new Vector3(cell.X * TILE_SIZE, 0, cell.Y * TILE_SIZE);
				terrain.Name = cell.X.ToString() + "," + cell.Y.ToString();
				terrainNode.AddChild(terrain);

                VisibleOnScreenNotifier3D visibleOnScreenNotifier3D = new VisibleOnScreenNotifier3D
                {
                    Aabb = new Aabb(new Vector3(TILE_SIZE / -2, -0.5f, TILE_SIZE / -2), new Vector3(TILE_SIZE, 1, TILE_SIZE)),
                    Position = terrain.Position
                };
                visibleOnScreenNotifier3D.Connect("screen_entered", new Callable(terrain, "show"));
				visibleOnScreenNotifier3D.Connect("screen_exited", new Callable(terrain, "hide"));
				visiblerNode.AddChild(visibleOnScreenNotifier3D);
			}
		}
	}

	Vector2 FindNeighborFirstCell(Vector2 cell, Vector2 dir)
	{
		Vector2 currentCell = cell;
		while(true) // Find the first empty cell in the direction
		{
			currentCell += dir;
			if(IsAvailableCell(currentCell)) return currentCell + dir * rnd.Next(MIN_DISTANCE, MAX_DISTANCE);
		}
	}

	#endregion



	#region Cellular Automata
	/*
	
    void GenerateNoiseGrid()
	{		
		for (int i = 0; i < HEIGHT; i++)
			for (int j = 0; j < WIDTH; j++)
			{
				if (i == 0 || j == 0 || i == HEIGHT - 1 || j == WIDTH - 1)
					matrix[i, j] = TerrainType.WATER;
				else 
				{
					int random = (int)(GD.Randi() % 100 + 1);
					matrix[i, j] = (random > noiseDensity) ? TerrainType.FLOOR : TerrainType.WATER;
				}	
			}
	}

	void CellularAutomata()
	{
		for (int time = 0; time < iterations; time++)
		{
			SmoothMap();
		}
	}

	void SmoothMap()
	{
		for (int i = 0; i < HEIGHT; i++)
			for (int j = 0; j < WIDTH; j++)
			{
				int blockCount = GetSurroundingBlockCount(i, j);
				if (blockCount > 4) matrix[i, j] = TerrainType.WATER;
				else if (blockCount < 4) matrix[i, j] = TerrainType.FLOOR;
			}
	}

	int GetSurroundingBlockCount(int gridX, int gridY)
	{
		int blockCount = 0;
		for (int x = gridX - 1; x <= gridX + 1; x++)
			for (int y = gridY - 1; y <= gridY + 1; y++)
				if (IsWithinMatrix(x, y))
				{
					if (x != gridX || y != gridY)
						if (matrix[x, y] == TerrainType.WATER) 
							blockCount++;
				}
				else blockCount++;
		return blockCount;
	}

	bool IsWithinMatrix(int x, int y)
	{
		return x >= 0 && x < HEIGHT && y >= 0 && y < WIDTH;
	}

	void GenerateTerrain()
	{
		for (int i = 0; i < HEIGHT; i++)
			for (int j = 0; j < WIDTH; j++)
			{
				Terrain terrain = (matrix[i, j] == TerrainType.FLOOR) ? (Terrain)floorScene.Instantiate() : (Terrain)waterScene.Instantiate();
				terrain.Position = new Vector3(i * TILE_SIZE, 0, j * TILE_SIZE);
				terrainNode.AddChild(terrain);
			}
	}

	
	*/
	#endregion
}
