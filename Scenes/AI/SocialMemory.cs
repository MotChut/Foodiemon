using Godot;
using System;
using System.Collections.Generic;
using static Resources;

public partial class SocialMemory : Node
{
	Dictionary<Entities, AIPoints> socialMemory = new Dictionary<Entities, AIPoints>();
	[Export] Entities entityType;

    public override void _Ready()
    {
        foreach(Entities e in Enum.GetValues(typeof(Entities)))
		{
			if (e != entityType)
				socialMemory.Add(e, new AIPoints(0, 0));
		}
    }
}
