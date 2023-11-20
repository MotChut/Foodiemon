using System.Collections.Generic;
using Newtonsoft.Json;
using Godot;
using System.Reflection;
using System.Linq;
using System;

public partial class Resources : Node
{
	
	public const int TILE_SIZE = 10;
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
	
	public static Dictionary<string, PackedScene> SceneDictionary = new Dictionary<string, PackedScene>()
	{
		["MovableGrass"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/MovableGrass.tscn"),
		["Tree"] =(PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/Tree.tscn"),
		["BerryBush"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/BerryBush.tscn")
	};
	public static List<EntitySettings> EntitySettingsList = new List<EntitySettings>(){};
	public static List<TerrainSettings> TerrainSettingsList = new List<TerrainSettings>(){};
	public static List<int> TerrainObjectRates = new List<int>(){};


	// Loading Section

    public override void _Ready()
    {
        LoadEntitySettings();
		LoadTerrainSettings();
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
			terrainSettings.GetWeights();
			terrainSettings.GetRate();
		}
    }

	void GenerateObjectRate()
	{
		// foreach(var terrain in TerrainSettings)
		// {
		// 	int temp = 0;
		// 	rates.Add(temp);
		// 	foreach(var obj in objectTypes)
		// 	{
		// 		temp += obj.Value;
		// 		rates.Add(temp);
		// 	}
		// }
	}
}
