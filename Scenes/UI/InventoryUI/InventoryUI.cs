using Godot;
using System;
using System.Collections.Generic;
using static Resources;

public partial class InventoryUI : CanvasLayer
{
	const int SIZEX = 1280;
	const int SIZEY = 720;

	PackedScene inventoryButtonScene = (PackedScene)ResourceLoader.Load("res://Scenes/UI/InventoryUI/InventoryButton.tscn");

	public int MAX_EQUIPMENTS = 3;
	int nEquipments = 0;
	List<Button> equipmentButtons = new List<Button>();
	List<InventoryButton> inventoryButtons = new List<InventoryButton>();
	GridContainer gridContainer;
	GridContainer inventoryContainer;
	public override void _Ready()
	{
		gridContainer = GetNode<GridContainer>("Container/HBoxContainer/Left/TextureRect/MarginContainer/ScrollContainer/MarginContainer/Inventory");
		inventoryContainer = GetNode<GridContainer>("Container/HBoxContainer/Right/TextureRect/MarginContainer/ScrollContainer/MarginContainer/Inventory");
		foreach(Button button in gridContainer.GetChildren())
		{
			equipmentButtons.Add(button);
			button.Pressed += () => RemoveEquipment(button);
		}

		Connect("visibility_changed", new Callable(this, "ChangeVisible"));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public async override void _PhysicsProcess(double delta)
	{
		if(Input.IsActionJustPressed("inventory") && Visible)
		{
			Hide();
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			GetTree().Paused = false;
			GetTree().Root.GetNode<PlayerHUD>("PlayerHud").Visible = true;
		}
	}

	void ChangeVisible()
	{
		if(Visible)
		{
			Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
			Scale = new Vector2(viewportSize.X / SIZEX, viewportSize.Y / SIZEY);
			LoadUserInventory();
			LoadUserEquipment();
		}
		else
		{
			RemoveUserInventory();
		}
	}

	public int GetCurrentEquipments()
	{
		return nEquipments;
	}

	void LoadUserEquipment()
	{
		int index = 0;
		foreach(var i in userdata.userEquipment)
		{
			gridContainer.GetChild<Button>(index++).GetNode<TextureRect>("TextureRect").Texture = DishAsset[i];
			nEquipments++;
		}
	}

	void LoadUserInventory()
	{
		foreach(var i in userdata.userInventory)
		{
			InventoryButton inventoryButton = (InventoryButton)inventoryButtonScene.Instantiate();
			inventoryContainer.AddChild(inventoryButton);
			inventoryButtons.Add(inventoryButton);
			inventoryButton.SetTexture(DishAsset[i]);
			inventoryButton.SetType(i);
		}
	}

	void RemoveUserInventory()
	{
		foreach(var i in inventoryContainer.GetChildren())
		{
			i.QueueFree();
		}
		foreach(var i in gridContainer.GetChildren())
		{
			i.GetNode<TextureRect>("TextureRect").Texture = null;
		}
		inventoryButtons.Clear();
		nEquipments = 0;
	}

	public void AssignEquipment(Texture2D texture, InventoryButton inventoryButton)
	{
		userdata.userInventory.RemoveAt(inventoryButtons.IndexOf(inventoryButton));
		inventoryButton.QueueFree();
		equipmentButtons[nEquipments].GetNode<TextureRect>("TextureRect").Texture = texture;
		PlayerHUD playerHUD = GetTree().Root.GetNode<PlayerHUD>("PlayerHud");
		switch(nEquipments)
		{
			case 0:
			playerHUD.SetItem1(texture);
			break;
			case 1:
			playerHUD.SetItem2(texture);
			break;
			case 2:
			playerHUD.SetItem3(texture);
			break;
		}
		userdata.userEquipment.Add(inventoryButton.GetDishType());
		nEquipments++;
	}

	void RemoveEquipment(Button button)
	{
		if(button.GetNode<TextureRect>("TextureRect").Texture == null) return;
		
		InventoryButton inventoryButton = (InventoryButton)inventoryButtonScene.Instantiate();
		inventoryContainer.AddChild(inventoryButton);
		inventoryButton.SetTexture(button.GetNode<TextureRect>("TextureRect").Texture);
		inventoryButton.SetType(userdata.userEquipment[button.GetIndex()]);
		inventoryButtons.Add(inventoryButton);
		userdata.userInventory.Add(userdata.userEquipment[button.GetIndex()]);
		
		button.GetNode<TextureRect>("TextureRect").Texture = null;
		ReorderEquipment(equipmentButtons.IndexOf(button));
		userdata.userEquipment.RemoveAt(equipmentButtons.IndexOf(button));
		GetTree().Root.GetNode<PlayerHUD>("PlayerHud").Reorder(equipmentButtons.IndexOf(button));
		nEquipments--;
	}

	void ReorderEquipment(int index)
	{
		for(int i = index + 1; i < MAX_EQUIPMENTS; i++)
		{
			if(equipmentButtons[i].GetNode<TextureRect>("TextureRect").Texture != null)
			{
				equipmentButtons[i-1].GetNode<TextureRect>("TextureRect").Texture = 
					equipmentButtons[i].GetNode<TextureRect>("TextureRect").Texture;
				equipmentButtons[i].GetNode<TextureRect>("TextureRect").Texture = null;
			}
		}
	}
}
