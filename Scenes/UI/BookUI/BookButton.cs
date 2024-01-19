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
	string type;
	bool available;
	
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

	public void SetAvailable(bool b)
	{
		available = b;

		if(!available)
		{
			Modulate = new Color(0, 0, 0, 1);
			Disabled = true;
			FocusMode = FocusModeEnum.None;
		}
	}

	public void SetType(string s)
	{
		type = s;
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
		switch(type)
		{
			case "Recipe":
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
			break;
			case "Creature":
			bookUI.UpdateName(name);
			bookUI.UpdateDescription(description);
			bookUI.UpdateTexture(textureRect.Texture);
			break;
			case "Craft":
			bookUI.UpdateName(name);
			bookUI.UpdateDescription(description);
			bookUI.UpdateTexture(textureRect.Texture);
			bookUI.ClearRecipe();

			string editedName2 = System.Text.RegularExpressions.Regex.Replace(name, @"\s+", "");
			if(Enum.IsDefined(typeof(CraftType), 
			(CraftType)Enum.Parse(typeof(CraftType), editedName2)))
			{
				bookUI.UpdateMaterials(name);
			}
			break;
		}
		
	}
}
