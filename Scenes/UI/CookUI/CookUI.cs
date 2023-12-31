using Godot;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class CookUI : CanvasLayer
{
	PackedScene ingredientButtonScene = (PackedScene)ResourceLoader.Load("res://Scenes/UI/CookUI/IngredientButton.tscn");
	const int SIZEX = 1280;
	const int SIZEY = 720;
	public int MAX_INGREDIENTS = 5;
	List<Button> ingredientLists = new List<Button>();
	int assignedIngredients = 0;
	BoxContainer container;
	GridContainer ingredients;
	Label ingredientDescription;

	public override void _Ready()
	{
		container = GetNode<BoxContainer>("Container/VBox");
		HBoxContainer hBoxContainer = GetNode<HBoxContainer>("Container/VBox/Body/HBoxContainer/Left/VBoxContainer/IngredientBtns/HBox");
		ingredients = GetNode<GridContainer>("Container/VBox/Body/HBoxContainer/MarginContainer/Right/VBox/MarginContainer/ScrollContainer/MarginContainer/Ingredients");
		ingredientDescription = GetNode<Label>("Container/VBox/Body/HBoxContainer/MarginContainer/Right/VBox/MarginContainer2/Description");
		foreach(Button button in hBoxContainer.GetChildren())
		{
			ingredientLists.Add(button);
			button.Pressed += () => RemoveIngredient(button);
		}

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		Scale = new Vector2(viewportSize.X / SIZEX, viewportSize.Y / SIZEY);
		LoadUserIngredients();
	}

	void LoadUserIngredients()
	{
		UserData userData = UserData.GetInstance();
		userData.GenerateUserData();
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

	public void AssignIngredient(Texture2D texture)
	{
		ingredientLists[assignedIngredients].GetNode<TextureRect>("TextureRect").Texture = texture;
		assignedIngredients++;
	}

	void RemoveIngredient(Button button)
	{
		if(button.GetNode<TextureRect>("TextureRect").Texture == null) return;
		button.GetNode<TextureRect>("TextureRect").Texture = null;
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
}
