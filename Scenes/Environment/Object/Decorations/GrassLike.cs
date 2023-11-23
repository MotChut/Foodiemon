using Godot;

public partial class GrassLike : Object
{
	const float regainFormSpd = 10;
	bool isWithin = false;

	Node3D entityWithin;
	Area3D detectArea;
	
	
	public override void _Ready()
	{
		detectArea = GetNode<Area3D>("DetectArea");
		detectArea.Connect("body_entered", new Callable(this, "DetectArea_Enter"));
		detectArea.Connect("body_exited", new Callable(this, "DetectArea_Exit"));
	}

	public override void _Process(double delta)
	{
		if(isWithin)
		{
			Vector3 toTarget = entityWithin.GlobalTransform.Origin - GlobalTransform.Origin;
			toTarget = new Vector3(-toTarget.X, toTarget.Y -0.3f, -toTarget.Z);
			if((GlobalTransform.Origin - toTarget.Normalized()).Dot(Vector3.Up) > 0.001f) return;
			GlobalTransform = GlobalTransform.LookingAt(GlobalTransform.Origin - toTarget.Normalized(), Vector3.Up);
		}
		else
		{
			RotationDegrees = RotationDegrees.MoveToward(Vector3.Zero, regainFormSpd);
		}
	}

	void DetectArea_Enter(Node3D body)
	{
		isWithin = true;
		entityWithin = body;
	}

	void DetectArea_Exit(Node3D body)
	{
		isWithin = false;
		entityWithin = null;
	}
}
