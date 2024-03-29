using System.Collections.Generic;
using Newtonsoft.Json;
using Godot;
using System.Linq;


public partial class Resources : Node
{
	public static int NUMBER_OF_DAYS = 0;

	#region Constants
	public static string NextPath = "";
	public const float SPEED_SCALE = 1f;
	public const int MIN_REGION_SIZE = 7;
	public const int MAX_REGION_SIZE = 14;
	public const int TILE_SIZE = 20;
	public const string EntitySettingsJson = "res://Jsons/EntitySettings.json";
	public const string TerrainSettingsJson = "res://Jsons/TerrainSettings.json";
	public const string RegionSettingsJson = "res://Jsons/RegionSettings.json";
	public const string StatsSettingsJson = "res://Jsons/StatsSettings.json";
	public const string CraftsJson = "res://Jsons/Crafts.json";
	public const string CooksJson = "res://Jsons/Cooks.json";
	public const string StartRelationshipJson = "res://Jsons/StartRelationship.json";
	public const string PossibleRelationshipJson = "res://Jsons/PossibleRelationship.json";
	public const string ForestRegion_Scene = "res://Scenes/Environment/Terrain/ForestRegion.tscn";
	public const string ProceduralGeneration_Path = "res://Scenes/Map/ProceduralGeneration.tscn";
	public const string TitleScreen_Path = "res://Scenes/UI/TitleScreen/TitleScreen.tscn";
	public const string Home_Path = "res://Scenes/Map/Home/Home.tscn";
	public const string LoadingScene_Path = "res://Scenes/UI/Loading/Loading.tscn";
	public const string CookScene_Path = "res://Scenes/UI/CookUI/CookUI.tscn";
	public const string ForgeScene_Path = "res://Scenes/UI/ForgeUI/ForgeUI.tscn";
	public static ProceduralGeneration proGen;
	public static UserData userdata;

	public enum Relationship
	{
		Ignore, Eat, Afraid, StayOutOfWay, AggressiveRival, Attack, 
		Uncomfortable, Antagonize, PlayWith, SocialDependent, Pack
	}

	public enum Entities
	{
		BlueChicpea, Chicpea, Rawrberry, Potatoon, Flowee
	}

	public enum RegionType {
		ForestRegion, DesertRegion
	}
	
	public enum TerrainType {
		Floor, Trees, ChicpeaBase, GrassLand, RawrberryBase, 
		Desert, Potatoon, Flowee
	}

	public enum MaterialType 
	{
		Berry, Twig, Flint, CutGrass, ChicNugget, ChicEgg, ChicLeg, SrawrBerry, RawrMeat,
		RawrHorn
	}

	public enum CraftType
	{
		BasicSpear
	}

	public enum DishType
	{
		ChickenBerrySauce, Salad, BerrySoup
	}

	public static List<TerrainType> EntitiesTerrainType = new List<TerrainType>()
	{
		TerrainType.ChicpeaBase, TerrainType.RawrberryBase, TerrainType.Potatoon, TerrainType.Flowee
	};
	
	public static List<MaterialType?> FoodMaterialType = new List<MaterialType?>()
	{
		MaterialType.Berry, MaterialType.ChicNugget, MaterialType.ChicEgg, MaterialType.ChicLeg,
		MaterialType.RawrMeat, MaterialType.SrawrBerry
	};

	public static List<MaterialType?> CraftMaterialType = new List<MaterialType?>()
	{
		MaterialType.Twig, MaterialType.Flint, MaterialType.CutGrass, MaterialType.RawrHorn
	};

	public static Dictionary<MaterialType?, Texture2D> MaterialAssets = new Dictionary<MaterialType?, Texture2D>()
	{
		[MaterialType.Berry] = (Texture2D)GD.Load("res://Assets/FoodIngredients/Berry.png"),
		[MaterialType.ChicNugget] = (Texture2D)GD.Load("res://Assets/FoodIngredients/ChicNugget.png"),
		[MaterialType.ChicLeg] = (Texture2D)GD.Load("res://Assets/FoodIngredients/ChicLeg.png"),
		[MaterialType.ChicEgg] = (Texture2D)GD.Load("res://Assets/FoodIngredients/ChicEgg.png"),
		[MaterialType.Twig] = (Texture2D)GD.Load("res://Assets/Material/Twig.png"),
		[MaterialType.Flint] = (Texture2D)GD.Load("res://Assets/Material/Flint.png"),
		[MaterialType.CutGrass] = (Texture2D)GD.Load("res://Assets/Material/Cut grass.png"),
		[MaterialType.RawrHorn] = (Texture2D)GD.Load("res://Assets/Material/Rawr horn.png"),
		[MaterialType.RawrMeat] = (Texture2D)GD.Load("res://Assets/FoodIngredients/Rawrmeat.png"),
		[MaterialType.SrawrBerry] = (Texture2D)GD.Load("res://Assets/FoodIngredients/Srawr berry.png"),
	};

	public static Dictionary<MaterialType?, string> MaterialDescriptions = new Dictionary<MaterialType?, string>()
	{
		[MaterialType.Berry] = "This is a cool fresh berry, barely hatched from the bush!",
		[MaterialType.ChicNugget] = "This is nugget that makes u nutttttt!",
		[MaterialType.ChicLeg] = "Big juicy Chicpea Leg!",
		[MaterialType.ChicEgg] = "Delicious extra protein!",
		[MaterialType.Twig] = "Just a boring dry twig.",
		[MaterialType.Flint] = "Flint can be very useful in multiple aspects.",
		[MaterialType.CutGrass] = "1 cutgrass can do nothing but 3 cutgrass can become a rope.",
		[MaterialType.RawrHorn] = "Could be extremely expensive!",
		[MaterialType.RawrMeat] = "The flesh of the dangerous predator!",
		[MaterialType.SrawrBerry] = "Cool to eat no matter hot or cold the weather is."
	};

	public static Dictionary<DishType, Texture2D> DishAsset = new Dictionary<DishType, Texture2D>()
	{
		[DishType.ChickenBerrySauce] = (Texture2D)GD.Load("res://Assets/Dish/ChickenBerrySauce.png"),
		[DishType.Salad] = (Texture2D)GD.Load("res://Assets/Dish/Salad.png"),
		[DishType.BerrySoup] = (Texture2D)GD.Load("res://Assets/Dish/Berry soup.png")
	};

	public static Dictionary<CraftType, Texture2D> CraftAsset = new Dictionary<CraftType, Texture2D>()
	{
		[CraftType.BasicSpear] = (Texture2D)GD.Load("res://Assets/Material/Spear.png"),
	};

	public static Dictionary<CraftType, string> CraftDescription = new Dictionary<CraftType, string>()
	{
		[CraftType.BasicSpear] = "Basic spear than a Chicpea can also make it.",
	};

	public static Dictionary<Entities, Texture2D> CreatureAsset = new Dictionary<Entities, Texture2D>()
	{
		[Entities.Chicpea] = (Texture2D)GD.Load("res://Assets/2D/Chicpea.png"),
		[Entities.BlueChicpea] = (Texture2D)GD.Load("res://Assets/2D/BlueChicpea.png"),
		[Entities.Rawrberry] = (Texture2D)GD.Load("res://Assets/2D/Rawrberry.png"),
		[Entities.Potatoon] = (Texture2D)GD.Load("res://Assets/2D/Potatoon.png"),
		[Entities.Flowee] = (Texture2D)GD.Load("res://Assets/2D/Flowee.png"),
	};

	public static Dictionary<Entities, string> CreatureDescription = new Dictionary<Entities, string>()
	{
		[Entities.Chicpea] = "A creature mixed from chicken & pea.",
		[Entities.BlueChicpea] = "Chicpea leader, the one that leads the whole pack.",
		[Entities.Rawrberry] = "Not so easy-going. Like to hunt everything down, no matter chics or you.",
		[Entities.Potatoon] = "Potatatatatatooooon. A creature that likes hot sand and warm sunlight.",
		[Entities.Flowee] = "POISON ALERT! KEEP AWAY FROM IT!"
	};

	public static Dictionary<DishType, string> DishDescription = new Dictionary<DishType, string>()
	{
		[DishType.ChickenBerrySauce] = "Kill Chicpeas and appreciate their juicy meaty chunky legs.",
		[DishType.Salad] = "A combination of natural materials! (not all but kinda)",
		[DishType.BerrySoup] = "Warm soup is made using a dangerous creature - RAWRBERRY!."
	};

	public static Dictionary<string, PackedScene> ObjectSceneDictionary = new Dictionary<string, PackedScene>()
	{
		["MovableGrass"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/Decorations/MovableGrass.tscn"),
		["Tree"] =(PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/Obstacles/Tree.tscn"),
		["BerryBush"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/Sources/BerryBush.tscn"),
		["Rock1"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/Obstacles/Rock1.tscn"),
		["GrassBush"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/Sources/GrassBush.tscn"),
		["Flint"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/Sources/Flint.tscn"),
		["GrassSource"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/Sources/GrassSource.tscn"),
		["Tree2"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/Obstacles/Tree2.tscn"),
		["Dune"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/Obstacles/Dune.tscn"),
		["Grass2"] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Object/Decorations/Grass2.tscn"),
	};

	public static Dictionary<MaterialType?, PackedScene> MaterialSceneDictionary = new Dictionary<MaterialType?, PackedScene>()
	{
		[MaterialType.Berry] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Material/Berry.tscn"),
		[MaterialType.Twig] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Material/Twig.tscn"),
		[MaterialType.Flint] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Material/Flint.tscn"),
		[MaterialType.CutGrass] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Material/CutGrass.tscn")
	};

	public static Dictionary<CraftType, PackedScene> CraftSceneDictionary = new Dictionary<CraftType, PackedScene>()
	{
		//[CraftType.Berry] = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/Material/Berry.tscn")
	};

	public static Dictionary<Entities, PackedScene> EntitySceneDictionary = new Dictionary<Entities, PackedScene>()
	{
		[Entities.Chicpea] = (PackedScene)ResourceLoader.Load("res://Scenes/Entities/Chicpea/Chicpea.tscn")
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
		if(CurrentTime == GameTime.Night && time == GameTime.Day) NUMBER_OF_DAYS += 1;

		CurrentTime = time;
		
		if(time == GameTime.Day)
		{
			TriggerPlanTask();
		}
			
		UpdateCurrentTask();
	}

	static void TriggerPlanTask()
	{
		foreach(Pack pack in PackList)
		{
			if(pack.entities.Count <= 0) continue;
			pack.Craft();
			pack.PackConsumeFoods();
			if(pack.leader != null) pack.leader.IntoGroups();
		}
	}

	static void UpdateCurrentTask()
	{
		foreach(Pack pack in PackList)
		{
			foreach(Entity entity in pack.entities.ToList())
			{
				if(entity == null) continue;
				entity.UpdateCurrentTask();
			}
		}
	}

	// Generated Parts
	public static List<StatsSettings> StatsSettingsList = new List<StatsSettings>(){};
	public static List<EntitySettings> EntitySettingsList = new List<EntitySettings>(){};
	public static List<TerrainSettings> TerrainSettingsList = new List<TerrainSettings>(){};
	public static List<RegionSettings> RegionSettingsList = new List<RegionSettings>(){};
	public static List<Crafts> CraftsList = new List<Crafts>(){};

	public static List<StartRelationship> StartRelationships = new List<StartRelationship>(){};
	public static List<PossibleRelationship> PossibleRelationships = new List<PossibleRelationship>(){};

	public static List<Cooks> CookList = new List<Cooks>(){};

	public static List<Pack> PackList = new List<Pack>();
	
	
	// Loading Section
    public override void _Ready()
    {
        Generate();
    }

	public static void Generate()
	{
		StatsSettingsList = new List<StatsSettings>(){};
		EntitySettingsList = new List<EntitySettings>(){};
		TerrainSettingsList = new List<TerrainSettings>(){};
		RegionSettingsList = new List<RegionSettings>(){};
		CraftsList = new List<Crafts>(){};
		CookList = new List<Cooks>(){};
		PackList = new List<Pack>();
		LoadCraftsList();
		LoadCookList();
		LoadStartRelationship();
		LoadPossibleRelationship();
		LoadStatsSettings();
		LoadEntitySettings();
		LoadTerrainSettings();
		LoadRegionSettings();
		userdata = UserData.GetInstance();
	}

	static void LoadStatsSettings()
	{
		var jsonString = FileAccess.GetFileAsString(StatsSettingsJson);
        StatsSettingsList = JsonConvert.DeserializeObject<List<StatsSettings>>(jsonString); 
	}

	static void LoadEntitySettings()
    {
        var jsonString = FileAccess.GetFileAsString(EntitySettingsJson);
        EntitySettingsList = JsonConvert.DeserializeObject<List<EntitySettings>>(jsonString); 

		foreach(EntitySettings entitySettings in EntitySettingsList)
		{
			entitySettings.CreateScenes();
		}
    }

	static void LoadTerrainSettings()
    {
        var jsonString = FileAccess.GetFileAsString(TerrainSettingsJson);
        TerrainSettingsList = JsonConvert.DeserializeObject<List<TerrainSettings>>(jsonString); 

		foreach(TerrainSettings terrainSettings in TerrainSettingsList)
		{
			terrainSettings.Init();
		}
    }

	static void LoadRegionSettings()
    {
        var jsonString = FileAccess.GetFileAsString(RegionSettingsJson);
        RegionSettingsList = JsonConvert.DeserializeObject<List<RegionSettings>>(jsonString); 

		foreach(RegionSettings regionSettings in RegionSettingsList)
		{
			regionSettings.Init();
		}
    }

	static void LoadCraftsList()
    {
        var jsonString = FileAccess.GetFileAsString(CraftsJson);
        CraftsList = JsonConvert.DeserializeObject<List<Crafts>>(jsonString); 
    }

	static void LoadStartRelationship()
    {
        var jsonString = FileAccess.GetFileAsString(StartRelationshipJson);
        StartRelationships = JsonConvert.DeserializeObject<List<StartRelationship>>(jsonString);
		foreach(StartRelationship startRelationship in StartRelationships)
		{
			startRelationship.Init();
		}
    }

	static void LoadPossibleRelationship()
    {
        var jsonString = FileAccess.GetFileAsString(PossibleRelationshipJson);
        PossibleRelationships = JsonConvert.DeserializeObject<List<PossibleRelationship>>(jsonString); 
		foreach(PossibleRelationship possibleRelationship in PossibleRelationships)
		{
			possibleRelationship.Init();
		}
    }

	static void LoadCookList()
    {
        var jsonString = FileAccess.GetFileAsString(CooksJson);
        CookList = JsonConvert.DeserializeObject<List<Cooks>>(jsonString); 
    }

	#endregion
}
