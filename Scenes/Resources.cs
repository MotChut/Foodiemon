using System;
using Godot;

public partial class Resources : Node
{
	public enum Entities
	{
		Player, 
		Chicpea,
		Dukapa
	}

	public enum RegionType {
		ForestRegion
	}
	
	public enum TerrainType {
		Floor, // Plain, only has passable objects
		Trees, // Trees,...
		ChicpeaBase // Base of Chicpeas
	}

	public const int TILE_SIZE = 10;

	public const string ForestRegion_Scene = "res://Scenes/Environment/Terrain/ForestRegion.tscn";
	public const string TREE_SCENE = "res://Scenes/Environment/Object/Tree.tscn";
	public const string MOVABLEGRASS_SCENE = "res://Scenes/Environment/Object/MovableGrass.tscn";
}
