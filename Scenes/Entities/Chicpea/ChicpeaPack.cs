using static Entity;
using static Resources;
using System.Collections.Generic;
using System;
using Godot;


public partial class ChicpeaPack : Pack
{
    public ChicpeaPack()
    {
        groups.Add(Task.Explore, new List<Entity>());
        groups.Add(Task.CollectFood, new List<Entity>());
        materials.Add(MaterialType.Twig, 0);
        materials.Add(MaterialType.Flint, 0);
        materials.Add(MaterialType.CutGrass, 0);
    }

    public override void Craft()
    {
        Crafts crafts = CraftsList.Find(x => System.Text.RegularExpressions.Regex.Replace(x.craft, @"\s+", "") == craftType.ToString());
		Dictionary<string, int> recipe = new Dictionary<string, int>();
		if (crafts.material1 != "") recipe.Add(crafts.material1, crafts.amount1);
		if (crafts.material2 != "") recipe.Add(crafts.material2, crafts.amount2);
		if (crafts.material3 != "") recipe.Add(crafts.material3, crafts.amount3);
		if (crafts.material4 != "") recipe.Add(crafts.material4, crafts.amount4);
		if (crafts.material5 != "") recipe.Add(crafts.material5, crafts.amount5);

		while(true)
		{
			bool flag = true;
			foreach(string material in recipe.Keys)
			{
				if(materials[(MaterialType)Enum.Parse(typeof(MaterialType), material)] < recipe[material])
				{
					flag = false;
					break;
				}
			}
			if(!flag) break;
			foreach(string material in recipe.Keys)
			{
				materials[(MaterialType)Enum.Parse(typeof(MaterialType), material)] -= recipe[material];
			}
			weapons++;
		} 
    }
}