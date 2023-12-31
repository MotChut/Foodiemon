using Godot;
using static Resources;
using System;
using System.Collections.Generic;

public partial class ProceduralGeneration : Node3D
{
	[Export] int CAMERA_SPEED = 400;

	#region Constants
	const int MIN_DISTANCE = 1;
	const int MAX_DISTANCE = 3;
	const int MIN_REGION = 3;
	const int MAX_REGION = 6;
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
	[Export] int nRegion = 5;

	List<Region> regions = new List<Region>();

	#endregion

	#region Nodes
	public Node3D terrainNode;
	public Node3D blockerNode;
	public Node3D visiblerNode;
	public Node3D entityNode;
	Node3D camera;

	#endregion

	public override void _Ready()
	{
		terrainNode = GetNode<Node3D>("Terrain");
		blockerNode = GetNode<Node3D>("Blocker");
		visiblerNode = GetNode<Node3D>("Visibler");
		entityNode = GetNode<Node3D>("Entities");
		//camera = GetNode<Node3D>("Pivot");
		proGen = this;
		rnd = new Random();
		GeneratePseudoTree();
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
			Generate();
			GetTree().ReloadCurrentScene();
			
		}
    }

	#region Procedural Generation

	void GeneratePseudoTree()
	{
		List<Vector2> tree = new List<Vector2>();
	
		nRegion = rnd.Next(MIN_REGION, MAX_REGION + 1);
		Region region = new Region(RegionType.ForestRegion);
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
					Region newRegion = new Region(RegionType.ForestRegion);
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
			else if (currentPos.Y < point2.Y) currentPos.Y += 1;
			else if (currentPos.X > point2.X) currentPos.X -= 1;
			else if (currentPos.Y > point2.Y) currentPos.Y -= 1;
			
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
			region.GenerateTerrain();
		}

		GenerateBlockers();
	}

	void GenerateBlockers()
	{
		foreach(Region region in regions)
		{
			region.GenerateBlockers();
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
}
