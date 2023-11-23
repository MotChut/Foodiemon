using Godot;
using System;

public partial class MapSource : Object
{
	[Export] int minResources;
	[Export] int maxResources;
	int resources;
	bool canInteract = false;

	Random rnd;
	SubViewport subViewport;
	Sprite3D interactiveNotice;
	Area3D detectArea;

    public override void _Ready()
    {
		interactiveNotice = GetNode<Sprite3D>("InteractiveNotice");
		subViewport = GetNode<SubViewport>("InteractiveNotice/SubViewport");
		detectArea = GetNode<Area3D>("DetectArea");

		subViewport.Size =(Vector2I)GetNode<Label>("InteractiveNotice/SubViewport/Label").GetRect().Size;
		subViewport.Size = new Vector2I(subViewport.Size.X, subViewport.Size.Y * 6);

		detectArea.Connect("body_entered", new Callable(this, "DetectArea_Enter"));
		detectArea.Connect("body_exited", new Callable(this, "DetectArea_Exit"));

        rnd = new Random();
		RegenerateResource();
    }

	void RegenerateResource()
	{
		resources = rnd.Next(minResources, maxResources + 1);
	}

	public int CollectResource(int value)
	{
		int amount;
		if (resources == 0) amount = 0;
		else if (resources < value)
		{
			amount = resources;
			resources = 0;
		}
		else
		{
			amount = value;
			resources -= value;
		}
		return amount;
	}

	void DetectArea_Enter(Node3D body)
	{
		if(body is Player)
		{
			canInteract = true;
			interactiveNotice.Show();
		}
	}

	void DetectArea_Exit(Node3D body)
	{
		if(body is Player)
		{
			canInteract = false;
			interactiveNotice.Hide();
		}
	}
}
