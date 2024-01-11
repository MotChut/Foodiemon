using Godot;
using System;
using System.Collections.Generic;

public partial class BookButton : Button
{
	BookUI bookUI;
	TextureRect textureRect;
	string name;
	string description;
	public override void _Ready()
	{
		textureRect = GetNode<TextureRect>("Texture");

		bookUI = (BookUI)GetTree().Root.GetNode<CanvasLayer>("BookUI");
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
	}
}
