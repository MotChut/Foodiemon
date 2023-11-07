using Godot;
using System;

public partial class MapSource : Object
{
	[Export] int minResources;
	[Export] int maxResources;
	int resources;

	Random rnd;

    public override void _Ready()
    {
        rnd = new Random();
		RegenerateResource();
    }

	void RegenerateResource()
	{
		resources = rnd.Next(minResources, maxResources + 1);
	}

	public int CollectResource(int value)
	{
		int amount;
		if (resources == 0) amount = 0;
		else if (resources < value)
		{
			amount = resources;
			resources = 0;
		}
		else
		{
			amount = value;
			resources -= value;
		}
		return amount;
	}
}
