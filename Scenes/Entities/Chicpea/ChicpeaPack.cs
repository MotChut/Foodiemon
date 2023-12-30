using static Entity;
using static Resources;
using System.Collections.Generic;
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
}