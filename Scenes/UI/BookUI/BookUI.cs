using Godot;
using System;
using System.Collections.Generic;
using static Resources;

public partial class BookUI : CanvasLayer
{
	private BookUI()
    {
    }
    private static BookUI _instance;
    public static BookUI GetInstance()
    {
        if (_instance == null)
        {
            _instance = new BookUI();
        }
        return _instance;
    }

	const int SIZEX = 1280;
	const int SIZEY = 720;
	const int TAB_OFFSET = 15;

	PackedScene buttonScene = (PackedScene)ResourceLoader.Load("res://Scenes/UI/BookUI/BookButton.tscn");
	UserData userdata;
	TextureRect textureRect;
	TextureButton creatureTab, recipeTab, gearTab;
	GridContainer itemContainer;
	Label desLabel, nameLabel;

	public override void _Ready()
	{
		creatureTab = GetNode<TextureButton>("CreatureTab");
		recipeTab = GetNode<TextureButton>("RecipeTab");
		gearTab = GetNode<TextureButton>("GearTab");
		itemContainer = GetNode<GridContainer>("Container/BookTexture/Twosides/Left/ScrollContainer/MarginContainer/Ingredients");
		desLabel = GetNode<Label>("Container/BookTexture/Twosides/Right/VBoxContainer/DesContainer/Label");
		nameLabel = GetNode<Label>("Container/BookTexture/Twosides/Right/VBoxContainer/NameContainer/Label");
		textureRect = GetNode<TextureRect>("Container/BookTexture/Twosides/Right/VBoxContainer/ShowcaseContainer/TextureRect");

		creatureTab.FocusEntered += () => GetFocus(creatureTab);
		recipeTab.FocusEntered += () => GetFocus(recipeTab);
		gearTab.FocusEntered += () => GetFocus(gearTab);
		creatureTab.FocusExited += () => OutFocus(creatureTab);
		recipeTab.FocusExited += () => OutFocus(recipeTab);
		gearTab.FocusExited += () => OutFocus(gearTab);
		creatureTab.Pressed += () => TabPressed(creatureTab);
		recipeTab.Pressed += () => TabPressed(recipeTab);
		gearTab.Pressed += () => TabPressed(gearTab);

		creatureTab.GrabFocus();

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		Scale = new Vector2(viewportSize.X / SIZEX, viewportSize.Y / SIZEY);

		userdata = UserData.GetInstance();
	}

    public override void _PhysicsProcess(double delta)
    {
        if(Input.IsActionJustPressed("escape"))
		{
			Visible = false;
			GetTree().Paused = false;
		}
    }

    void GetFocus(TextureButton btn)
	{
		btn.ZIndex = 2;
		btn.GlobalPosition += new Vector2(0, -TAB_OFFSET);
		if(btn == gearTab)
		{
			recipeTab.ZIndex = 0;
			creatureTab.ZIndex = 1;
		}
		if(btn == creatureTab)
		{
			recipeTab.ZIndex = 0;
			gearTab.ZIndex = 0;
		}
		if(btn == recipeTab)
		{
			creatureTab.ZIndex = 1;
			gearTab.ZIndex = 0;
		}
	}

	void OutFocus(TextureButton btn)
	{
		btn.GlobalPosition += new Vector2(0, TAB_OFFSET);
	}

	void TabPressed(TextureButton btn)
	{
		ClearItems();
		if(btn == gearTab)
		{
			
		}
		if(btn == creatureTab)
		{
			LoadCreatures();
		}
		if(btn == recipeTab)
		{
			LoadRecipes();
		}
	}

	void ClearItems()
	{
		foreach(var i in itemContainer.GetChildren())
		{
			i.QueueFree();
		}
	}

	void LoadGears()
	{

	}

	void LoadCreatures()
	{

	}

	void LoadRecipes()
	{
		foreach(var i in userdata.userDishes)
		{
			BookButton bookButton = (BookButton)buttonScene.Instantiate();
			itemContainer.AddChild(bookButton);
			bookButton.SetName(i.ToString());
			bookButton.SetTexture(DishAsset[i]);
			bookButton.SetDescription(DishDescription[i]);
		}
	}

	public void UpdateName(string name)
	{
		nameLabel.Text = name;
	}

	public void UpdateDescription(string des)
	{
		desLabel.Text = des;
	}

	

	public void UpdateTexture(Texture2D texture)
	{
		textureRect.Texture = texture;
	}
}
