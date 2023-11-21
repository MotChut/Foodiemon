using System.Collections.Generic;
using Newtonsoft.Json;
using Godot;
using System.Reflection;
using System.Linq;
using System;

public partial class Resources : Node
{
	public const int MIN_REGION_SIZE = 15;
	public const int MAX_REGION_SIZE = 20;
	public const int TILE_SIZE = 20;
	public const string EntitySettingsJson = "res://Jsons/EntitySettings.json";
	public const string TerrainSettingsJson = "res://Jsons/TerrainSettings.json";
	public const string RegionSettingsJson = "res://Jsons/RegionSettings.json";
	public const string ForestRegion_Scene = "res://Scenes/Environment/Terrain/ForestRegion.tscn";
	public static ProceduralGeneration proGen;

	public enum Entities
	{
		Player, Chicpea, Dukapa
	}

	public enum RegionType {
		ForestRegion
	}
	
	public enum TerrainType {
		Floor, Trees, ChicpeaBase
	}

	public static List<TerrainType> EntitiesTerrainType = new List<TerrainType>()
	{
		TerrainType.ChicpeaBase
	};

	public static Dictionary<string, PackedScene> ObjectSceneDictionary = new Dictionary<string, PackedScene>()
	{
		["MovableGrass"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/MovableGrass.tscn"),
		["Tree"] =(PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/Tree.tscn"),
		["BerryBush"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/BerryBush.tscn")
	};
	public static List<EntitySettings> EntitySettingsList = new List<EntitySettings>(){};
	public static List<TerrainSettings> TerrainSettingsList = new List<TerrainSettings>(){};
	public static List<RegionSettings> RegionSettingsList = new List<RegionSettings>(){};

	// Loading Section

    public override void _Ready()
    {
        Generate();
    }

	public void Generate()
	{
		LoadEntitySettings();
		LoadTerrainSettings();
		LoadRegionSettings();
	}

	void LoadEntitySettings()
    {
        var jsonString = FileAccess.GetFileAsString(EntitySettingsJson);
        EntitySettingsList = JsonConvert.DeserializeObject<List<EntitySettings>>(jsonString); 

		foreach(EntitySettings entitySettings in EntitySettingsList)
		{
			entitySettings.CreateScenes();
		}
    }

	void LoadTerrainSettings()
    {
        var jsonString = FileAccess.GetFileAsString(TerrainSettingsJson);
        TerrainSettingsList = JsonConvert.DeserializeObject<List<TerrainSettings>>(jsonString); 

		foreach(TerrainSettings terrainSettings in TerrainSettingsList)
		{
			terrainSettings.Init();
		}
    }

	void LoadRegionSettings()
    {
        var jsonString = FileAccess.GetFileAsString(RegionSettingsJson);
        RegionSettingsList = JsonConvert.DeserializeObject<List<RegionSettings>>(jsonString); 

		foreach(RegionSettings regionSettings in RegionSettingsList)
		{
			regionSettings.Init();
		}
    }
}
