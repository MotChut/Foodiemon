using Godot;
using System;
using System.Collections.Generic;
using static Object;
using static Resources;

public partial class Terrain : Node3D
{
	[Export] Vector3 meshSize;

	const float OBJECT_OFFSET = 0.5f;

	int chance = 5;
	Random rnd;
	Node3D objectList;
	PackedScene movableGrassScene = (PackedScene)ResourceLoader.Load(MOVABLEGRASS_SCENE);

	public override void _Ready()
	{
		objectList = GetNode<Node3D>("ObjectList");

		rnd = new Random();
		GenerateObject();
	}

	void GenerateObject()
	{
		for(int i = 0; i < TILE_SIZE; i++)
			for(int j = 0; j < TILE_SIZE; j++)
			{
				int random = rnd.Next(100);
				if (random < chance)
				{
					//if (!IsNotSurrounded(new Vector2I(i, j))) continue;
					Object grass = (Object)movableGrassScene.Instantiate();
					grass.Position = new Vector3(i - (TILE_SIZE - 2) / 2 - OBJECT_OFFSET, meshSize.Y / 2, 
												j - (TILE_SIZE - 2) / 2 - OBJECT_OFFSET);
					grass.Name = i.ToString() + "," + j.ToString();
					objectList.AddChild(grass);
				}
			}
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
