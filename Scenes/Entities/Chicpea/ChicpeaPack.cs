using static Entity;
using System.Collections.Generic;

public partial class ChicpeaPack : Pack
{
    public ChicpeaPack()
    {
        groups.Add(Task.Explore, new List<Entity>());
        groups.Add(Task.CollectFood, new List<Entity>());
    }
}