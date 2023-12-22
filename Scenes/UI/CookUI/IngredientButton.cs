using Godot;
using System;

public partial class IngredientButton : Button
{
	TextureRect textureRect;
	CookUI cookUI;

	public override void _Ready()
	{
		textureRect = GetNode<TextureRect>("Texture");
		cookUI = (CookUI)GetTree().Root.GetNode<CanvasLayer>("CookUI");

		Connect("pressed", new Callable(this, "Choose"));
	}

	public void SetTexture(Texture2D texture2D)
	{	
		
	}

	void Choose()
	{
		if(cookUI.GetCurrentIngredients() < cookUI.MAX_INGREDIENTS)
			cookUI.AssignIngredient(textureRect.Texture);
	}
}
