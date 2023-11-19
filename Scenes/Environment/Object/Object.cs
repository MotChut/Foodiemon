using Godot;
using System;

public partial class Object : Node3D
{
	const float regainFormSpd = 10;

	[Export] public Vector3 objectSize;
	[Export] public bool isInteractive;
	[Export] public bool isBlock;
	bool isWithin = false;

	Entity entityWithin;

	Area3D detectArea;

	public override void _Ready()
	{
		if(isInteractive)
		{
			detectArea = GetNode<Area3D>("DetectArea");
			detectArea.Connect("body_entered", new Callable(this, "DetectArea_Enter"));
			detectArea.Connect("body_exited", new Callable(this, "DetectArea_Exit"));
		}
	}

    public override void _PhysicsProcess(double delta)
    {
        if(isWithin)
		{
			Vector3 toTarget = entityWithin.GlobalTransform.Origin - GlobalTransform.Origin;
			toTarget = new Vector3(-toTarget.X, toTarget.Y -0.3f, -toTarget.Z);
			GlobalTransform = GlobalTransform.LookingAt(GlobalTransform.Origin - toTarget.Normalized(), Vector3.Up);
		}
		else
		{
			RotationDegrees = RotationDegrees.MoveToward(Vector3.Zero, regainFormSpd);
		}
    }

    void DetectArea_Enter(Node3D body)
	{
		if (body is Entity)
		{
			isWithin = true;
			entityWithin = body as Entity;
		}
	}

	void DetectArea_Exit(Node3D body)
	{
		if (body is Entity)
		{
			isWithin = false;
			entityWithin = null;
		}
	}
}
