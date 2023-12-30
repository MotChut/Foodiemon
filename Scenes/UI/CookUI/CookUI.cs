using Godot;
using System.Collections.Generic;
using System.Drawing;

public partial class CookUI : CanvasLayer
{
	const int SIZEX = 1280;
	const int SIZEY = 720;
	public int MAX_INGREDIENTS = 5;
	List<Button> ingredientLists = new List<Button>();
	int assignedIngredients = 0;
	BoxContainer container;

	public override void _Ready()
	{
		container = GetNode<BoxContainer>("Container/VBox");
		HBoxContainer hBoxContainer = GetNode<HBoxContainer>("Container/VBox/Body/HBoxContainer/Left/VBoxContainer/IngredientBtns/HBox");
		foreach(Button button in hBoxContainer.GetChildren())
		{
			ingredientLists.Add(button);
			button.Pressed += () => RemoveIngredient(button);
		}

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		Scale = new Vector2(viewportSize.X / SIZEX, viewportSize.Y / SIZEY);
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
}
