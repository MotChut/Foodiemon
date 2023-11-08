using Godot;
using System;
using static Resources;

public partial class Entity : CharacterBody3D
{
	[Export] protected float maxSpd;
	[Export] protected float accel;
	[Export] protected float friction;
	[Export] protected int maxHp;
	[Export] protected int attackPoint;
	[Export] protected int attackSpd;
	[Export] public Entities entityType;

	//protected int workPoint;

	public enum States
	{
		Idle, Walk
	}

	protected States currentState = States.Idle;
	public Vector3 lastDirection = Vector3.Zero;


    public override void _Ready()
    {
        
    }

	
}
