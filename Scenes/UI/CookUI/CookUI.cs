using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Resources;

public partial class CookUI : CanvasLayer
{
	[Export] const float TweenTime = 0.25f;
	[Export] const float CookTime = 3f;
	Texture2D failTexture = (Texture2D)GD.Load("res://Assets/Dish/Fail dish.png");
	PackedScene ingredientButtonScene = (PackedScene)ResourceLoader.Load("res://Scenes/UI/CookUI/IngredientButton.tscn");
	const int SIZEX = 1280;
	const int SIZEY = 720;
	bool canCook = true;
	bool clickRequired = false;
	public int MAX_INGREDIENTS = 5;
	List<MaterialType?> materialTypes = new List<MaterialType?>();
	List<Button> ingredientLists = new List<Button>();
	List<IngredientButton> ingredientBtns = new List<IngredientButton>();
	int assignedIngredients = 0;
	BoxContainer container;
	GridContainer ingredients;
	Label ingredientDescription, newDishName;
	AnimationPlayer animationPlayer;
	TextureButton cookBtn;
	TextureRect cookTexture, dishTexture;
	HBoxContainer hBoxContainer;
	Control newDishUI;

	public override void _Ready()
	{
		container = GetNode<BoxContainer>("Container/VBox");
		hBoxContainer = GetNode<HBoxContainer>("Container/VBox/Body/HBoxContainer/Left/VBoxContainer/IngredientBtns/HBox");
		ingredients = GetNode<GridContainer>("Container/VBox/Body/HBoxContainer/MarginContainer/Right/VBox/MarginContainer/ScrollContainer/MarginContainer/Ingredients");
		ingredientDescription = GetNode<Label>("Container/VBox/Body/HBoxContainer/MarginContainer/Right/VBox/MarginContainer2/Description");
		animationPlayer = GetNode<AnimationPlayer>("Container/VBox/Body/HBoxContainer/Left/VBoxContainer/Pot/AnimationPlayer");
		cookBtn = GetNode<TextureButton>("Container/VBox/Footer/CookBtn");
		cookTexture = GetNode<TextureRect>("Container/VBox/Body/HBoxContainer/Left/VBoxContainer/Pot/PotTex");
		newDishUI = GetNode<Control>("NewDish");
		newDishName = newDishUI.GetNode<Label>("VBoxContainer/NameLabel");
		dishTexture = GetNode<TextureRect>("NewDish/VBoxContainer/DishBG/Dish");
		
		foreach(Button button in hBoxContainer.GetChildren())
		{
			ingredientLists.Add(button);
			button.Pressed += () => RemoveIngredient(button);
		}

		cookBtn.Connect("pressed", new Callable(this, "StartToCook"));

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		Scale = new Vector2(viewportSize.X / SIZEX, viewportSize.Y / SIZEY);

		LoadUserIngredients();
	}

    public override void _PhysicsProcess(double delta)
    {
        if(Input.IsActionJustPressed("escape"))
		{
			GetTree().Paused = false;
			QueueFree();
			GetTree().Root.GetNode<PlayerHUD>("PlayerHud").Visible = true;
		}
		if(Input.IsActionJustPressed("attack") && clickRequired)
		{
			clickRequired = false;
			newDishUI.Visible = false;
		}
    }

    void LoadUserIngredients()
	{
		foreach(var i in userdata.userIngredients.Keys.ToList())
		{
			if(userdata.userIngredients[i] > 0)
			{
				IngredientButton ingredientButton = (IngredientButton)ingredientButtonScene.Instantiate();
				ingredients.AddChild(ingredientButton);
				ingredientButton.materialType = i;
				ingredientButton.SetTexture(MaterialAssets[i]);
				ingredientButton.SetAmount(userdata.userIngredients[i]);
				ingredientButton.SetDescription(MaterialDescriptions[i]);
			}
		}
	}

	public int GetCurrentIngredients()
	{
		return assignedIngredients;
	}

	public void AssignIngredient(Texture2D texture, IngredientButton ingredientButton)
	{
		ingredientBtns.Add(ingredientButton);
		ingredientLists[assignedIngredients].GetNode<TextureRect>("TextureRect").Texture = texture;
		materialTypes.Add(ingredientButton.materialType);
		assignedIngredients++;
	}

	void RemoveIngredient(Button button)
	{
		if(button.GetNode<TextureRect>("TextureRect").Texture == null) return;
		button.GetNode<TextureRect>("TextureRect").Texture = null;
		materialTypes.RemoveAt(ingredientLists.IndexOf(button));
		ingredientBtns[ingredientLists.IndexOf(button)].UpdateAmount(1);
		ingredientBtns.RemoveAt(ingredientLists.IndexOf(button));
		ReorderIngredient(ingredientLists.IndexOf(button));
		assignedIngredients--;
	}

	void ReorderIngredient(int index)
	{
		for(int i = index + 1; i < MAX_INGREDIENTS; i++)
		{
			if(ingredientLists[i].GetNode<TextureRect>("TextureRect").Texture != null)
			{
				ingredientLists[i-1].GetNode<TextureRect>("TextureRect").Texture = 
					ingredientLists[i].GetNode<TextureRect>("TextureRect").Texture;
				ingredientLists[i].GetNode<TextureRect>("TextureRect").Texture = null;
			}
		}
	}

	public void UpdateIngredientDescription(string s)
	{
		ingredientDescription.Text = s;
	}

	void DisableIngredients()
	{
		foreach(var button in ingredients.GetChildren())
		{
			button.SetDeferred("disabled", true);
		}
		foreach(var button in hBoxContainer.GetChildren())
		{
			button.SetDeferred("disabled", true);
		}
	}

	void EnableIngredients()
	{
		foreach(var button in ingredients.GetChildren())
		{
			button.SetDeferred("disabled", false);
		}
		foreach(var button in hBoxContainer.GetChildren())
		{
			button.SetDeferred("disabled", false);
		}
	}

	void StartToCook()
	{
		canCook = false;
		DisableIngredients();
		ConsumeIngredients();
	}

	async void ConsumeIngredients()
	{
		foreach(var ingredient in ingredientLists)
		{
			if(ingredient.GetNode<TextureRect>("TextureRect").Texture != null)
			{
				TextureRect textureRect = (TextureRect)ingredient.GetNode<TextureRect>("TextureRect").Duplicate();
				textureRect.AnchorsPreset = 0;
				textureRect.GlobalPosition = ingredient.GlobalPosition;
				AddChild(textureRect);
				ingredient.GetNode<TextureRect>("TextureRect").Texture = null;
				Tween tween = CreateTween();
				tween.TweenProperty(textureRect, "global_position", cookTexture.GlobalPosition + 
					new Vector2(cookTexture.Size.X / 3.5f, cookTexture.Size.Y / 1.5f), TweenTime * 2);
				tween.TweenCallback(new Callable(textureRect, "queue_free"));
				await ToSignal(GetTree().CreateTimer(TweenTime), "timeout");		
			}
		}
		ingredientBtns.Clear();
		assignedIngredients = 0;
		await ToSignal(GetTree().CreateTimer(TweenTime), "timeout");
		Cooking();
	}

	async void Cooking()
	{
		animationPlayer.Play("Cook");
		await ToSignal(GetTree().CreateTimer(CookTime), "timeout");
		animationPlayer.Stop();
		CreateDish();
	}

	void CreateDish()
	{
		Dictionary<MaterialType?, int> materials = new Dictionary<MaterialType?, int>();
		foreach(MaterialType? materialType in materialTypes)
		{
			userdata.userIngredients[materialType] -= 1;
			if(!materials.Keys.Contains(materialType))
			{
				materials.Add(materialType, 1);
			}
			else
			{
				materials[materialType] += 1;
			}
		}
		materialTypes.Clear();

		bool hasPossibleDish = false;
		Cooks possibleDish = null;
		foreach(Cooks recipe in CookList)
		{
			Dictionary<string, int> r = new Dictionary<string, int>();
			if (recipe.material1 != "") r.Add(recipe.material1, recipe.amount1);
			if (recipe.material2 != "") r.Add(recipe.material2, recipe.amount2);
			if (recipe.material3 != "") r.Add(recipe.material3, recipe.amount3);
			if (recipe.material4 != "") r.Add(recipe.material4, recipe.amount4);
			if (recipe.material5 != "") r.Add(recipe.material5, recipe.amount5);

			while(true)
			{
				bool flag = true;
				foreach(string material in r.Keys)
				{
					if(!materials.Keys.Contains((MaterialType)Enum.Parse(typeof(MaterialType), material)) ||
					materials[(MaterialType)Enum.Parse(typeof(MaterialType), material)] < r[material])
					{
						flag = false;
						break;
					}
				}
				if(!flag) break;
				else
				{
					possibleDish = recipe;
					hasPossibleDish = true;
					break;
				}
			} 
		}

		if(hasPossibleDish) // Has some dishes
		{
			userdata.userDishes[possibleDish.food] = true;
			string type = System.Text.RegularExpressions.Regex.Replace(possibleDish.food, @"\s+", "");
			userdata.userInventory.Add((DishType)Enum.Parse(typeof(DishType), type));
			RunDishAnimation((DishType)Enum.Parse(typeof(DishType), type), possibleDish.food);
		}
		else // No possible dishes
		{
			RunDishAnimation(DishType.ChickenBerrySauce, "");
		}
	}

	void RunDishAnimation(DishType dish, string name = "")
	{
		if(name == "")
		{
			newDishName.Text = "Failed!";
			dishTexture.Texture = failTexture;
		}
		else
		{
			newDishName.Text = name;
			dishTexture.Texture = DishAsset[dish];
		}
		newDishUI.Visible = true;
		GetNode<AnimationPlayer>("NewDish/AnimationPlayer").Play();
		clickRequired = true;
		EnableIngredients();
		canCook = true;
	}
}
