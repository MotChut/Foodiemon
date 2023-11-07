using Godot;
using System;
using static Resources;

public partial class Entity : CharacterBody3D
{
	[Export] float maxSpd;
	[Export] float accel;
	[Export] float friction;
	[Export] int maxHp;
	[Export] int attackPoint;
	[Export] int attackSpd;
	[Export] Entities entityType;

	int workPoint;

	public enum States
	{
		
	}

    public override void _Ready()
    {
        
    }


}
