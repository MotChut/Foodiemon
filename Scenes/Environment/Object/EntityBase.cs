using System;
using System.Linq;
using Godot;
using static Entity;
using static Resources;
public partial class EntityBase : Object
{
	[Export] public string[] entityTypes;
	Area3D detectArea;

	public override void _Ready()
	{
		detectArea = GetNode<Area3D>("MeshInstance3D/DetectArea");
		detectArea.Connect("body_entered", new Callable(this, "DetectArea_Enter"));
		detectArea.Connect("body_exited", new Callable(this, "DetectArea_Exit"));
	}

	void DetectArea_Enter(Node3D body)
	{
		Entity entity = (Entity)body;

		// if is the same type
		if(entityTypes.Contains(
		Enum.GetName(typeof(Entities), entity.entityType)))
		{
			switch(entity.currentTask)
			{
				case Task.ForceHome:
				entity.pack.UpdateDirectionPoint(entity.wanderDir, 0);
				break;
			}
		}
	}

	void DetectArea_Exit(Node3D body)
	{
		
	}
}
