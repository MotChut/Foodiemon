using Godot;
using System;
using static Resources;

public partial class MapSource : Object
{
	[Export] float TotalRegrowTime;
	[Export] public bool isFoodSource;
	[Export] public bool canDisappear;
	[Export] public MaterialType resourceType;
	[Export] int minResources;
	[Export] int maxResources;
	[Export] public float collectTime;
	int resources;
	int currentResources;
	bool canInteract = false;

	Random rnd;
	SubViewport subViewport;
	Sprite3D interactiveNotice;
	Area3D detectArea;
	Timer regrowTimer;
	Node3D sourcesNode;

    public override void _Ready()
    {
		interactiveNotice = GetNode<Sprite3D>("InteractiveNotice");
		subViewport = interactiveNotice.GetNode<SubViewport>("SubViewport");
		detectArea = GetNode<Area3D>("DetectArea");
		regrowTimer = GetNode<Timer>("RegrowTimer");
		sourcesNode = GetNode<Node3D>("Sources");

		subViewport.Size =(Vector2I)GetNode<Label>("InteractiveNotice/SubViewport/Label").GetRect().Size;
		subViewport.Size = new Vector2I(subViewport.Size.X, subViewport.Size.Y * 6);

		detectArea.Connect("body_entered", new Callable(this, "DetectArea_Enter"));
		detectArea.Connect("body_exited", new Callable(this, "DetectArea_Exit"));
		regrowTimer.Connect("timeout", new Callable(this, "Regrow"));

        rnd = new Random();
		regrowTimer.WaitTime /= SPEED_SCALE;
		
		RegenerateResource();
    }

    public override void _PhysicsProcess(double delta)
    {
		
    }

    void RegenerateResource()
	{
		resources = rnd.Next(minResources, maxResources + 1);
		currentResources = resources;
		regrowTimer.WaitTime = TotalRegrowTime / SPEED_SCALE / resources;
	}

	public int GetCurrentResources()
	{
		return currentResources;
	}

	public void CollectResource(int value)
	{
		currentResources -= value;
		if(currentResources <= 0 && canDisappear) Visible = false;
		else if(currentResources > 0) Visible = true;
		if(regrowTimer.IsStopped()) regrowTimer.Start();
	}


	void DetectArea_Enter(Node3D body)
	{
		if(body is Player)
		{
			canInteract = true;
			interactiveNotice.Show();
			((Player)body).SetCurrentMapSource(this);
		}
	}
	void DetectArea_Exit(Node3D body)
	{
		if(body is Player)
		{
			canInteract = false;
			interactiveNotice.Hide();
			if(((Player)body).GetCurrentMapSource() == this) ((Player)body).SetCurrentMapSource(null);
		}
	}

	void Regrow()
	{
		currentResources += 1;
		if(currentResources < resources) regrowTimer.Start();
	}
}
