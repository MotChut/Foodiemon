using System.Collections.Generic;
using System.Linq;
using Godot;
using static Resources;

public partial class RegionSettings : Node
{
    // Pull from sheet
	public int id = 0;
	public string regionType = "";
	public string path = "";
	public int chicpeaMax = 0;
	public int floor = 0;
	public int trees = 0;
	public int chicpeaBase = 0;
	public int grassLand = 0;
    public int rawrberryMax = 0;
	public int rawrberryBase = 0;


    // Local
	public PackedScene terrain;
    public int averageDistance = 0;
    public int distanceCount = 0;
    public bool firstSpawn = false;
    List<int> weights = new List<int>();
	public List<int> rates = new List<int>();
    public List<int> upBoundaries = new List<int>();
    public List<int> entityTypesControl = new List<int>();

    public void Init()
    {
        CreateScenes();
        GetWeights();
        GetRates();
        GetUpBoundaries();
        CreateTypeControl();
        CreateAverageDistance();
    }

	void CreateScenes()
	{
		terrain = (PackedScene)ResourceLoader.Load(path);
	}

    void GetWeights()
	{
        weights = new List<int>
        {
            floor,
            trees,
            chicpeaBase,
            grassLand,
            rawrberryBase
        };
	}

	void GetRates()
	{
		int temp = 0;
		rates.Add(temp);
		foreach(int w in weights)
		{
			temp += w;
			rates.Add(temp);
		}
	}

    void GetUpBoundaries()
    {
        upBoundaries.Add(chicpeaMax);
        upBoundaries.Add(rawrberryMax);
    }

    void CreateTypeControl()
    {
        for(int i = 0; i < upBoundaries.Count; i++)
            entityTypesControl.Add(0);
    }

    void CreateAverageDistance()
    {
        averageDistance = 12;
    }

    public bool SetTypeControlByIndex(int key, int value)
    {
        entityTypesControl[key] = value;
        if(entityTypesControl[key] > upBoundaries[key]) return false;
        return true;
    }
}
