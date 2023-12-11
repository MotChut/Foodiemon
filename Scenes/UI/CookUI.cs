using Godot;
using System;
using System.Collections.Generic;

public partial class CookUI : CanvasLayer
{
	public int MAX_INGREDIENTS = 5;
	List<Button> ingredientLists = new List<Button>();
	int assignedIngredients = 0;

	public override void _Ready()
	{
		HBoxContainer hBoxContainer = GetNode<HBoxContainer>("VBox/Body/HBoxContainer/VBoxContainer/Cook/VBox/IngredientBtns/Panel/HBox");
		foreach(Button button in hBoxContainer.GetChildren())
		{
			ingredientLists.Add(button);
			button.Pressed += () => RemoveIngredient(button);
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
}
