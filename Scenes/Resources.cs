using Godot;
using System;

public partial class Resources : Node
{
	public enum TerrainType {
		FLOOR,
		WATER
	}

	public const int TILE_SIZE = 1;

	public const string FLOOR_SCENE = "res://Scenes/Objects/Terrain/Floor.tscn";
	public const string WATER_SCENE = "res://Scenes/Objects/Terrain/Water.tscn";
}
