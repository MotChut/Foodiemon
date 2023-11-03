using Godot;
using System;

public partial class Entity : CharacterBody3D
{
	[Export] float MAX_SPEED;
	[Export] float ACCEL;
	[Export] int MAX_HP;

    public override void _Ready()
    {
        
    }
}
