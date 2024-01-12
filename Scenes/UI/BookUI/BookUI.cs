using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
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
	PackedScene recipeComScene = (PackedScene)ResourceLoader.Load("res://Scenes/UI/BookUI/RecipeComponent.tscn");
	UserData userdata;
	TextureRect textureRect;
	TextureButton creatureTab, recipeTab, gearTab;
	GridContainer itemContainer;
	Label desLabel, nameLabel;
	HBoxContainer components;
	TextureButton current = null;

	public override void _Ready()
	{
		creatureTab = GetNode<TextureButton>("CreatureTab");
		recipeTab = GetNode<TextureButton>("RecipeTab");
		gearTab = GetNode<TextureButton>("GearTab");
		itemContainer = GetNode<GridContainer>("Container/BookTexture/Twosides/Left/ScrollContainer/MarginContainer/Ingredients");
		desLabel = GetNode<Label>("Container/BookTexture/Twosides/Right/VBoxContainer/DesContainer/Label");
		nameLabel = GetNode<Label>("Container/BookTexture/Twosides/Right/VBoxContainer/NameContainer/Label");
		textureRect = GetNode<TextureRect>("Container/BookTexture/Twosides/Right/VBoxContainer/ShowcaseContainer/TextureRect");
		components = GetNode<HBoxContainer>("Container/BookTexture/Twosides/Right/VBoxContainer/RecipeContainer/Recipes");

		creatureTab.FocusEntered += () => GetFocus(creatureTab);
		recipeTab.FocusEntered += () => GetFocus(recipeTab);
		gearTab.FocusEntered += () => GetFocus(gearTab);
		creatureTab.Pressed += () => TabPressed(creatureTab);
		recipeTab.Pressed += () => TabPressed(recipeTab);
		gearTab.Pressed += () => TabPressed(gearTab);

		Connect("visibility_changed", new Callable(this, "ChangeVisibleState"));
		
		userdata = UserData.GetInstance();
		TabPressed(recipeTab);

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		Scale = new Vector2(viewportSize.X / SIZEX, viewportSize.Y / SIZEY);

	}

    public override void _PhysicsProcess(double delta)
    {
        if(Input.IsActionJustPressed("escape"))
		{
			Visible = false;
			if(current == recipeTab) TabPressed(gearTab);
			GetTree().Paused = false;
		}
    }

	void ChangeVisibleState()
	{
		if(Visible)
		{
			recipeTab.ButtonPressed = true;
			TabPressed(recipeTab);
		}
	}

    void GetFocus(TextureButton btn)
	{
		btn.ZIndex = 2;
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

	void OutFocus()
	{
		if(current == null) return;
		current.GlobalPosition += new Vector2(0, TAB_OFFSET);
	}

	void TabPressed(TextureButton btn)
	{
		if(current == btn) return;
		OutFocus();
		btn.GlobalPosition += new Vector2(0, -TAB_OFFSET);
		current = btn;
		ClearRight();
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
		ButtonGroup buttonGroup = new ButtonGroup();
		foreach(var i in userdata.userDishes)
		{
			BookButton bookButton = (BookButton)buttonScene.Instantiate();
			itemContainer.AddChild(bookButton);
			bookButton.SetName(i.ToString());
			bookButton.SetTexture(DishAsset[(DishType)Enum.Parse(typeof(DishType), System.Text.RegularExpressions.Regex.Replace(i, @"\s+", ""))]);
			bookButton.SetDescription(DishDescription[(DishType)Enum.Parse(typeof(DishType), System.Text.RegularExpressions.Regex.Replace(i, @"\s+", ""))]);
			bookButton.ButtonGroup = buttonGroup;
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

	public void UpdateRecipe(string name)
	{
		// Create materials list
		Cooks recipe = CookList.Find(x => x.food == name);
		Dictionary<string, int> r = new Dictionary<string, int>();
		if (recipe.material1 != "") r.Add(recipe.material1, recipe.amount1);
		if (recipe.material2 != "") r.Add(recipe.material2, recipe.amount2);
		if (recipe.material3 != "") r.Add(recipe.material3, recipe.amount3);
		if (recipe.material4 != "") r.Add(recipe.material4, recipe.amount4);
		if (recipe.material5 != "") r.Add(recipe.material5, recipe.amount5);
		
		// Load into UI
		foreach(string material in r.Keys.ToList())
		{
			var component = recipeComScene.Instantiate();
			components.AddChild(component);
			MaterialType materialType = (MaterialType)Enum.Parse(typeof(MaterialType), material);
			component.GetNode<TextureRect>("Texture").Texture = MaterialAssets[materialType];
			component.GetNode<Label>("Label").Text = "x " + r[material].ToString();
		}
	}

	public void UpdateTexture(Texture2D texture)
	{
		textureRect.Texture = texture;
	}

	public void ClearRecipe()
	{
		foreach(var i in components.GetChildren())
		{
			i.QueueFree();
		}
	}

	void ClearRight()
	{
		textureRect.Texture = null;
		desLabel.Text = "";
		nameLabel.Text = "";
		ClearRecipe();
	}
}
