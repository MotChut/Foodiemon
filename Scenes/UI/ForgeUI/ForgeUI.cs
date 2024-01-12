using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Resources;

public partial class ForgeUI : CanvasLayer
{
	[Export] const float TweenTime = 0.25f;
	[Export] const float CraftTime = 3f;
	Texture2D failTexture = (Texture2D)GD.Load("res://Assets/Material/Fail spear.png");
	PackedScene materialButtonScene = (PackedScene)ResourceLoader.Load("res://Scenes/UI/ForgeUI/MaterialButton.tscn");
	const int SIZEX = 1280;
	const int SIZEY = 720;
	bool canCraft = true;
	bool clickRequired = false;
	public int MAX_INGREDIENTS = 5;
	List<MaterialType?> materialTypes = new List<MaterialType?>();
	List<Button> materialLists = new List<Button>();
	List<MaterialButton> materialBtns = new List<MaterialButton>();
	int assignedMaterials = 0;
	BoxContainer container;
	GridContainer ingredients;
	Label ingredientDescription, newItemName;
	AnimationPlayer animationPlayer;
	TextureButton craftBtn;
	TextureRect craftTexture, itemTexture;
	HBoxContainer hBoxContainer;
	Control newItemUI;
	UserData userdata;

	public override void _Ready()
	{
		container = GetNode<BoxContainer>("Container/VBox");
		hBoxContainer = GetNode<HBoxContainer>("Container/VBox/Body/HBoxContainer/Left/VBoxContainer/IngredientBtns/HBox");
		ingredients = GetNode<GridContainer>("Container/VBox/Body/HBoxContainer/MarginContainer/Right/VBox/MarginContainer/ScrollContainer/MarginContainer/Ingredients");
		ingredientDescription = GetNode<Label>("Container/VBox/Body/HBoxContainer/MarginContainer/Right/VBox/MarginContainer2/Description");
		animationPlayer = GetNode<AnimationPlayer>("Container/VBox/Body/HBoxContainer/Left/VBoxContainer/Pot/AnimationPlayer");
		craftBtn = GetNode<TextureButton>("Container/VBox/Footer/CraftBtn");
		craftTexture = GetNode<TextureRect>("Container/VBox/Body/HBoxContainer/Left/VBoxContainer/Pot/PotTex");
		newItemUI = GetNode<Control>("NewItem");
		newItemName = newItemUI.GetNode<Label>("VBoxContainer/NameLabel");
		itemTexture = GetNode<TextureRect>("NewItem/VBoxContainer/ItemBG/Item");
		
		foreach(Button button in hBoxContainer.GetChildren())
		{
			materialLists.Add(button);
			button.Pressed += () => RemoveIngredient(button);
		}

		craftBtn.Connect("pressed", new Callable(this, "StartToCraft"));

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		Scale = new Vector2(viewportSize.X / SIZEX, viewportSize.Y / SIZEY);

		userdata = UserData.GetInstance();
		LoadUserIngredients();
	}

    public override void _PhysicsProcess(double delta)
    {
        if(Input.IsActionJustPressed("escape"))
		{
			GetTree().Paused = false;
			QueueFree();
		}
		if(Input.IsActionJustPressed("attack") && clickRequired)
		{
			clickRequired = false;
			newItemUI.Visible = false;
		}
    }

    void LoadUserIngredients()
	{
		UserData userData = UserData.GetInstance();
		foreach(var i in userData.userMaterials.Keys.ToList())
		{
			if(userData.userMaterials[i] > 0)
			{
				MaterialButton ingredientButton = (MaterialButton)materialButtonScene.Instantiate();
				ingredients.AddChild(ingredientButton);
				ingredientButton.materialType = i;
				ingredientButton.SetTexture(MaterialAssets[i]);
				ingredientButton.SetAmount(userData.userMaterials[i]);
				ingredientButton.SetDescription(MaterialDescriptions[i]);
			}
		}
	}

	public int GetCurrentIngredients()
	{
		return assignedMaterials;
	}

	public void AssignIngredient(Texture2D texture, MaterialButton materialButton)
	{
		materialBtns.Add(materialButton);
		materialLists[assignedMaterials].GetNode<TextureRect>("TextureRect").Texture = texture;
		materialTypes.Add(materialButton.materialType);
		assignedMaterials++;
	}

	void RemoveIngredient(Button button)
	{
		if(button.GetNode<TextureRect>("TextureRect").Texture == null) return;
		button.GetNode<TextureRect>("TextureRect").Texture = null;
		materialTypes.RemoveAt(materialLists.IndexOf(button));
		materialBtns[materialLists.IndexOf(button)].UpdateAmount(1);
		materialBtns.RemoveAt(materialLists.IndexOf(button));
		ReorderIngredient(materialLists.IndexOf(button));
		assignedMaterials--;
	}

	void ReorderIngredient(int index)
	{
		for(int i = index + 1; i < MAX_INGREDIENTS; i++)
		{
			if(materialLists[i].GetNode<TextureRect>("TextureRect").Texture != null)
			{
				materialLists[i-1].GetNode<TextureRect>("TextureRect").Texture = 
					materialLists[i].GetNode<TextureRect>("TextureRect").Texture;
				materialLists[i].GetNode<TextureRect>("TextureRect").Texture = null;
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

	void StartToCraft()
	{
		canCraft = false;
		DisableIngredients();
		ConsumeIngredients();
	}

	async void ConsumeIngredients()
	{
		foreach(var ingredient in materialLists)
		{
			if(ingredient.GetNode<TextureRect>("TextureRect").Texture != null)
			{
				TextureRect textureRect = (TextureRect)ingredient.GetNode<TextureRect>("TextureRect").Duplicate();
				textureRect.AnchorsPreset = 0;
				textureRect.GlobalPosition = ingredient.GlobalPosition;
				AddChild(textureRect);
				ingredient.GetNode<TextureRect>("TextureRect").Texture = null;
				Tween tween = CreateTween();
				tween.TweenProperty(textureRect, "global_position", craftTexture.GlobalPosition + 
					new Vector2(craftTexture.Size.X / 3.5f, craftTexture.Size.Y / 1.5f), TweenTime * 2);
				tween.TweenCallback(new Callable(textureRect, "queue_free"));
				await ToSignal(GetTree().CreateTimer(TweenTime), "timeout");		
			}
		}
		materialBtns.Clear();
		assignedMaterials = 0;
		await ToSignal(GetTree().CreateTimer(TweenTime), "timeout");
		Crafting();
	}

	async void Crafting()
	{
		animationPlayer.Play("Cook");
		await ToSignal(GetTree().CreateTimer(CraftTime), "timeout");
		animationPlayer.Stop();
		CraftItem();
	}

	void CraftItem()
	{
		Dictionary<MaterialType?, int> materials = new Dictionary<MaterialType?, int>();
		foreach(MaterialType? materialType in materialTypes)
		{
			userdata.userMaterials[materialType] -= 1;
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

		bool hasPossibleItem = false;
		Crafts possibleItem = null;
		foreach(Crafts recipe in CraftsList)
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
					possibleItem = recipe;
					hasPossibleItem = true;
					break;
				}
			} 
		}

		if(hasPossibleItem) // Has some dishes
		{
			string type = System.Text.RegularExpressions.Regex.Replace(possibleItem.craft, @"\s+", "");
			RunDishAnimation((CraftType)Enum.Parse(typeof(CraftType), type), possibleItem.craft);
		}
		else // No possible dishes
		{
			RunDishAnimation(CraftType.BasicSpear);
		}
	}

	void RunDishAnimation(CraftType item, string name = "")
	{
		if(name == "")
		{
			newItemName.Text = "Failed!";
			itemTexture.Texture = failTexture;
		}
		else
		{
			newItemName.Text = name;
			itemTexture.Texture = CraftAsset[item];
			
		}
		newItemUI.Visible = true;
		GetNode<AnimationPlayer>("NewItem/AnimationPlayer").Play();
		clickRequired = true;
		EnableIngredients();
		canCraft = true;
	}
}
