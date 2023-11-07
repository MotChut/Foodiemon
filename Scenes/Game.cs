using Godot;
using System;

public partial class Game : Node3D
{
	Player player;
	[Export] Material grassMaterial;

    public override void _Ready()
    {
        player = GetNode<Player>("Player");
    }

    public override void _Process(double delta)
	{
		(grassMaterial as ShaderMaterial).SetShaderParameter("character_position", player.GlobalTransform.Origin);
	}
}
