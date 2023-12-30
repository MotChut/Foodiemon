using Godot;
using static Resources;

public partial class Material : Node3D
{
	[Export] MaterialType materialType;
	[Export] public bool isFood = false;	
}
