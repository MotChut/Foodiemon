using Godot;
using System;
using System.Collections.Generic;
using static Resources;

public partial class BookButton : Button
{
	BookUI bookUI;
	TextureRect textureRect;
	string name;
	string description;
	
	public override void _Ready()
	{
		textureRect = GetNode<TextureRect>("Texture");

		bookUI = (BookUI)GetTree().Root.GetNode<CanvasLayer>("BookUi");
		Connect("pressed", new Callable(this, "Choose"));
	}

	public void SetTexture(Texture2D texture2D)
	{	
		textureRect.Texture = texture2D;
	}

	public void SetName(string s)
	{
		name = s;
	}

	public void SetDescription(string s)
	{
		description = s;
	}

	public void SetRecipe(List<Material?> materials)
	{

	}

	public void Choose()
	{
		bookUI.UpdateName(name);
		bookUI.UpdateDescription(description);
		bookUI.UpdateTexture(textureRect.Texture);
		bookUI.ClearRecipe();

		string editedName = System.Text.RegularExpressions.Regex.Replace(name, @"\s+", "");
		if(Enum.IsDefined(typeof(DishType), 
		(DishType)Enum.Parse(typeof(DishType), editedName)))
		{
			bookUI.UpdateRecipe(name);
		}
	}
}
