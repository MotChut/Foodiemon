using Godot;

public partial class EntitySettings : Node
{
	public int id = 0;
	public string terrainType = "";
	public string house = "";
	public string foodSource = "";
	public int minFoodSource = 0;
	public int maxFoodSource = 0;
	public string leader = "";
	public string entity = "";
	public int minEntities = 0;
	public int maxEntities = 0;
	public PackedScene houseScene, foodSourceScene, leaderScene, entityScene;

	public void CreateScenes()
	{
		houseScene = (PackedScene)ResourceLoader.Load(house);
		foodSourceScene = (PackedScene)ResourceLoader.Load(foodSource);
		if(leader != "") leaderScene = (PackedScene)ResourceLoader.Load(leader);
		entityScene = (PackedScene)ResourceLoader.Load(entity);
	}
}
