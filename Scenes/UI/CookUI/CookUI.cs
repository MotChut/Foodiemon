using Godot;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class CookUI : CanvasLayer
{
	[Export] const float TweenTime = 0.25f;
	[Export] const float CookTime = 3f;
	PackedScene ingredientButtonScene = (PackedScene)ResourceLoader.Load("res://Scenes/UI/CookUI/IngredientButton.tscn");
	const int SIZEX = 1280;
	const int SIZEY = 720;
	bool canCook = true;
	public int MAX_INGREDIENTS = 5;
	List<Button> ingredientLists = new List<Button>();
	List<IngredientButton> ingredientBtns = new List<IngredientButton>();
	int assignedIngredients = 0;
	BoxContainer container;
	GridContainer ingredients;
	Label ingredientDescription;
	AnimationPlayer animationPlayer;
	TextureButton cookBtn;
	TextureRect cookTexture;
	HBoxContainer hBoxContainer;

	public override void _Ready()
	{
		container = GetNode<BoxContainer>("Container/VBox");
		hBoxContainer = GetNode<HBoxContainer>("Container/VBox/Body/HBoxContainer/Left/VBoxContainer/IngredientBtns/HBox");
		ingredients = GetNode<GridContainer>("Container/VBox/Body/HBoxContainer/MarginContainer/Right/VBox/MarginContainer/ScrollContainer/MarginContainer/Ingredients");
		ingredientDescription = GetNode<Label>("Container/VBox/Body/HBoxContainer/MarginContainer/Right/VBox/MarginContainer2/Description");
		animationPlayer = GetNode<AnimationPlayer>("Container/VBox/Body/HBoxContainer/Left/VBoxContainer/Pot/AnimationPlayer");
		cookBtn = GetNode<TextureButton>("Container/VBox/Footer/CookBtn");
		cookTexture = GetNode<TextureRect>("Container/VBox/Body/HBoxContainer/Left/VBoxContainer/Pot/PotTex");
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
		}
    }

    void LoadUserIngredients()
	{
		UserData userData = UserData.GetInstance();
		foreach(var i in userData.userIngredients.Keys.ToList())
		{
			if(userData.userIngredients[i] > 0)
			{
				IngredientButton ingredientButton = (IngredientButton)ingredientButtonScene.Instantiate();
				ingredients.AddChild(ingredientButton);
				ingredientButton.SetTexture(Resources.FoodMaterialAsset[i]);
				ingredientButton.SetAmount(userData.userIngredients[i]);
				ingredientButton.SetDescription(Resources.FoodMaterialDescription[i]);
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
		assignedIngredients++;
	}

	void RemoveIngredient(Button button)
	{
		if(button.GetNode<TextureRect>("TextureRect").Texture == null) return;
		button.GetNode<TextureRect>("TextureRect").Texture = null;
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

	void StartToCook()
	{
		canCook = false;
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
		
		assignedIngredients = 0;
		await ToSignal(GetTree().CreateTimer(TweenTime), "timeout");
		Cooking();
	}

	async void Cooking()
	{
		animationPlayer.Play("Cook");
		await ToSignal(GetTree().CreateTimer(CookTime), "timeout");
		animationPlayer.Stop();
		canCook = true;
	}
}
