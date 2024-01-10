using System.Collections.Generic;
using Godot;
using static Resources;


public partial class EntityBase : Object
{
	public Pack pack;

    public override void _PhysicsProcess(double delta)
    {
		if(pack is ChicpeaPack)
			GetNode<Label>("InteractiveNotice/SubViewport/Label").Text = 
				"Food: " + pack.foods + "\n" +
				"Twigs: " + pack.materials[MaterialType.Twig] + "\n" +
				"Flints: " + pack.materials[MaterialType.Flint] + "\n" +
				"CutGrass: " + pack.materials[MaterialType.CutGrass] + "\n" + 
				"Material Sources: " + pack.materialSources.Count.ToString() +  "\n" +
				"Weapons: " + pack.weapons;
    }

	public void ReceiveFood(int amount)
	{
		pack.foods += amount;
	}
	public void ReceiveMaterial(MaterialType? type, int amount)
	{
		if(type == null) return;
		pack.materials[type] += amount;
	}
}
