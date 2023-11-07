using Godot;
using System;

public partial class Resources : Node
{
	public enum Entities
	{
		Player, 
		Chicpea,
		Dukapa
	}

	public enum RegionType {

	}
	
	public enum TerrainType {
		FLOOR,
		FOREST,
	}

	public const int TILE_SIZE = 10;

	public const string FLOOR_SCENE = "res://Scenes/Environment/Terrain/Floor.tscn";

	public const string TREE_SCENE = "res://Scenes/Environment/Object/Tree.tscn";
}
