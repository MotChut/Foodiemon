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
		Forest
	}
	
	public enum TerrainType {
		Floor,
		Forest,
	}

	public const int TILE_SIZE = 10;

	public const string FLOOR_SCENE = "res://Scenes/Environment/Terrain/Floor.tscn";

	public const string TREE_SCENE = "res://Scenes/Environment/Object/Tree.tscn";
	public const string MOVABLEGRASS_SCENE = "res://Scenes/Environment/Object/MovableGrass.tscn";
}
