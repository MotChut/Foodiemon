using Godot;
using System;
using static Object;
using static Resources;

public partial class Terrain : Node3D
{
	[Export] Vector3 meshSize;

	const float OBJECT_OFFSET = 0.5f;

	int chance = 1;
	Random rnd;
	PackedScene treeScene = (PackedScene)ResourceLoader.Load(TREE_SCENE);

	public override void _Ready()
	{
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
					Object tree = (Object)treeScene.Instantiate();
					tree.Position = new Vector3(i - (TILE_SIZE - 2) / 2 - OBJECT_OFFSET, meshSize.Y / 2 + OBJECT_OFFSET, 
												j - (TILE_SIZE - 2) / 2 - OBJECT_OFFSET);
					AddChild(tree);
				}
			}
	}
}
