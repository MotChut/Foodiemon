using Godot;
using static Resources;
using System;
using System.Collections.Generic;

public partial class ProceduralGeneration : Node3D
{
	// Constants
	const int WIDTH = 150;
	const int HEIGHT = 150;

	// Variables
	int noiseDensity = 65;
	int iterations = 10;

	TerrainType[,] matrix = new TerrainType[WIDTH, HEIGHT];
	PackedScene floorScene = (PackedScene)ResourceLoader.Load(FLOOR_SCENE);
	PackedScene waterScene = (PackedScene)ResourceLoader.Load(WATER_SCENE);
	Node3D terrainNode;
	Node3D camera;

	public override void _Ready()
	{
		terrainNode = GetNode<Node3D>("Terrain");
		camera = GetNode<Node3D>("Pivot");

		GD.Randomize();
		GenerateNoiseGrid();
		CellularAutomation();
		GenerateTerrain();
	}

    public override void _PhysicsProcess(double delta)
    {
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			camera.Position += new Vector3((float)(direction.X * 70 * delta), 0, (float)(direction.Z * 70 * delta)).Rotated(Vector3.Up, camera.Rotation.Y);
		}

		if (Input.IsActionJustPressed("reset"))
		{
			foreach (var i in terrainNode.GetChildren()) i.QueueFree();
			GenerateNoiseGrid();
			CellularAutomation();
			GenerateTerrain();
		}
    }

    void GenerateNoiseGrid()
	{		
		for (int i = 0; i < HEIGHT; i++)
			for (int j = 0; j < WIDTH; j++)
			{
				int random = (int)(GD.Randi() % 100 + 1);
				if (random > noiseDensity) matrix[i, j] = TerrainType.FLOOR;
				else matrix[i, j] = TerrainType.WATER;
			}
	}

	void CellularAutomation()
	{
		for (int time = 0; time < iterations; time++)
		{
			for (int i = 0; i < HEIGHT; i++)
				for (int j = 0; j < WIDTH; j++)
				{
					int neighboredWaterCount = 0;
					for (int x = i - 1; x <= i + 1; x++)
						for (int y = j - 1; y <= j + 1; y++)
							if (IsWithinMatrix(x, y))
							{
								if (x != i || y != j)
									if (matrix[x, y] == TerrainType.WATER) 
										neighboredWaterCount++;
							}
							else neighboredWaterCount++;
					
					if (neighboredWaterCount > 4) matrix[i, j] = TerrainType.WATER;
					else matrix[i, j] = TerrainType.FLOOR;
				}
		}
	}

	bool IsWithinMatrix(int x, int y)
	{
		return (x >= 0 && x < HEIGHT) && (y >= 0 && y < WIDTH);
	}

	void GenerateTerrain()
	{
		for (int i = 0; i < HEIGHT; i++)
			for (int j = 0; j < WIDTH; j++)
			{
				if (matrix[i, j] == TerrainType.FLOOR)
				{
					Terrain floor = (Terrain)floorScene.Instantiate();
					floor.Position = new Vector3(i * TILE_SIZE, 0, j * TILE_SIZE);
					terrainNode.AddChild(floor);
				}
				else
				{
					Terrain water = (Terrain)waterScene.Instantiate();
					water.Position = new Vector3(i * TILE_SIZE, 0, j * TILE_SIZE);
					terrainNode.AddChild(water);
				}
			}
	}
}
