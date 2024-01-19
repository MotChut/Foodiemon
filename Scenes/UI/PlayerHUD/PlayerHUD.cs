using Godot;
using System;

public partial class PlayerHUD : CanvasLayer
{
	const int SIZEX = 1280;
	const int SIZEY = 720;

	VBoxContainer healthContainer;
	TextureRect item1, item2, item3;
	
	public override void _Process(double delta)
	{
		healthContainer = GetNode<VBoxContainer>("PlayerHP/Health");
		item1 = GetNode<TextureRect>("ItemBox/Item1/TextureRect");
		item2 = GetNode<TextureRect>("ItemBox/Item2/TextureRect");
		item3 = GetNode<TextureRect>("ItemBox/Item3/TextureRect");
		GetNode<Label>("FPS").Text = "Fps: " + Engine.GetFramesPerSecond().ToString();

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		Scale = new Vector2(viewportSize.X / SIZEX, viewportSize.Y / SIZEY);
	}

	public void UpdateHealth(int value)
	{
		switch(value)
		{
			case 4:
			healthContainer.GetNode<TextureRect>("3").Show();
			healthContainer.GetNode<TextureRect>("2").Show();
			healthContainer.GetNode<TextureRect>("1").Show();
			break;
			case 3:
			healthContainer.GetNode<TextureRect>("3").Hide();
			healthContainer.GetNode<TextureRect>("2").Show();
			healthContainer.GetNode<TextureRect>("1").Show();
			break;
			case 2:
			healthContainer.GetNode<TextureRect>("3").Hide();
			healthContainer.GetNode<TextureRect>("2").Hide();
			healthContainer.GetNode<TextureRect>("1").Show();
			break;
			case 1:
			healthContainer.GetNode<TextureRect>("3").Hide();
			healthContainer.GetNode<TextureRect>("2").Hide();
			healthContainer.GetNode<TextureRect>("1").Hide();
			break;
		}
	}

	public void SetItem1(Texture2D texture)
	{
		item1.Texture = texture;
	}

	public void SetItem2(Texture2D texture)
	{
		item2.Texture = texture;
	}

	public void SetItem3(Texture2D texture)
	{
		item3.Texture = texture;
	}

	public void RemoveItem1()
	{
		item1.Texture = null;
	}

	public void RemoveItem2()
	{
		item2.Texture = null;
	}

	public void RemoveItem3()
	{
		item3.Texture = null;
	}

	public void Reorder(int index)
	{
		if(index == 0)
		{
			item1.Texture = item2.Texture;
			item2.Texture = item3.Texture;
			item3.Texture = null;
		}
		else if(index == 1)
		{
			item2.Texture = item3.Texture;
			item3.Texture = null;
		}
		else
		{
			item3.Texture = null;
		}
	}
}
