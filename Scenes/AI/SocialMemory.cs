using Godot;
using System;
using System.Collections.Generic;
using static Resources;

public partial class SocialMemory : Node
{
	Dictionary<Entities, AIPoints> socialMemory = new Dictionary<Entities, AIPoints>();
	[Export] Entities entityType;

	public SocialMemory()
	{
		
	}
}
