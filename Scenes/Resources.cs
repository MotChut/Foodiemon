using System.Collections.Generic;
using Newtonsoft.Json;
using Godot;


public partial class Resources : Node
{
	#region Constants
	public const int MIN_REGION_SIZE = 15;
	public const int MAX_REGION_SIZE = 20;
	public const int TILE_SIZE = 20;
	public const string EntitySettingsJson = "res://Jsons/EntitySettings.json";
	public const string TerrainSettingsJson = "res://Jsons/TerrainSettings.json";
	public const string RegionSettingsJson = "res://Jsons/RegionSettings.json";
	public const string StatsSettingsJson = "res://Jsons/StatsSettings.json";
	public const string ForestRegion_Scene = "res://Scenes/Environment/Terrain/ForestRegion.tscn";
	public static ProceduralGeneration proGen;

	public enum Entities
	{
		BlueChicpea, Chicpea
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
		["BerryBush"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/BerryBush.tscn"),
		["Mound"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/Mound.tscn")
	};

	#endregion

	#region Game Progress
	// Game Control
	public enum GameTime
	{
		Day, Noon, Night
	}
	public static GameTime CurrentTime = GameTime.Day;

	public static void SetCurrentTime(GameTime time)	
	{
		CurrentTime = time;
		if(time == GameTime.Day)
			TriggerPlanTask();
	}

	public static void TriggerPlanTask()
	{
		foreach(Pack pack in PackList)
		{
			pack.leader.PlanTask();
		}
	}


	// Generated Parts
	public static List<StatsSettings> StatsSettingsList = new List<StatsSettings>(){};
	public static List<EntitySettings> EntitySettingsList = new List<EntitySettings>(){};
	public static List<TerrainSettings> TerrainSettingsList = new List<TerrainSettings>(){};
	public static List<RegionSettings> RegionSettingsList = new List<RegionSettings>(){};
	public static List<Pack> PackList = new List<Pack>();
	
	
	// Loading Section
    public override void _Ready()
    {
		
        Generate();
    }

	public void Generate()
	{
		PackList = new List<Pack>();
		SetCurrentTime(GameTime.Day);
		LoadStatsSettings();
		LoadEntitySettings();
		LoadTerrainSettings();
		LoadRegionSettings();
	}

	void LoadStatsSettings()
	{
		var jsonString = FileAccess.GetFileAsString(StatsSettingsJson);
        StatsSettingsList = JsonConvert.DeserializeObject<List<StatsSettings>>(jsonString); 
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

	#endregion
}
