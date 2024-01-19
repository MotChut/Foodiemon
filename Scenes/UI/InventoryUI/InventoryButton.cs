using Godot;
using static Resources;

public partial class InventoryButton : Button
{
	int amount = 0;

	TextureRect textureRect;
	DishType dishType;
	InventoryUI inventoryUI;
	
	public override void _Ready()
	{
		textureRect = GetNode<TextureRect>("TextureRect");
		inventoryUI = (InventoryUI)GetTree().Root.GetNode("InventoryUi");

		Connect("pressed", new Callable(this, "Choose"));
	}

	public DishType GetDishType()
	{
		return dishType;
	}

	public void SetType(DishType type)
	{
		dishType = type;
	}
	
	public void SetTexture(Texture2D texture2D)
	{	
		textureRect.Texture = texture2D;
	}

	void Choose()
	{
		if(inventoryUI.GetCurrentEquipments() < inventoryUI.MAX_EQUIPMENTS)
		{
			inventoryUI.AssignEquipment(textureRect.Texture, this);
		}
	}
}
