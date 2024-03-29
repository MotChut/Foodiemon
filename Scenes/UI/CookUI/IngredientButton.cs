using Godot;
using static Resources;

public partial class IngredientButton : Button
{
	TextureRect textureRect;
	Label amountLabel;
	CookUI cookUI;
	int amount = 0;
	string description;
	public MaterialType? materialType;

	public override void _Ready()
	{
		textureRect = GetNode<TextureRect>("Texture");
		amountLabel = GetNode<Label>("Amount");
		cookUI = (CookUI)GetTree().Root.GetNode("Home").GetNode<CanvasLayer>("CookUI");
		SetAmount(0);

		Connect("pressed", new Callable(this, "Choose"));
		Connect("mouse_entered", new Callable(this, "MouseEnter"));
		Connect("mouse_exited", new Callable(this, "MouseLeave"));
	}

	public void SetTexture(Texture2D texture2D)
	{	
		textureRect.Texture = texture2D;
	}

	public void SetAmount(int n)
	{
		amount = n;
		UpdateAmount();
	}

	public void SetDescription(string s)
	{
		description = s;
	}

	public void UpdateAmount(int n = 0)
	{
		amount += n;
		amountLabel.Text = amount.ToString();
	}

	void Choose()
	{
		if(cookUI.GetCurrentIngredients() < cookUI.MAX_INGREDIENTS)
		{
			UpdateAmount(-1);
			cookUI.AssignIngredient(textureRect.Texture, this);
		}
	}

	void MouseEnter()
	{
		if(Disabled) return;
		Scale = new Vector2(1.2f, 1.2f);
		cookUI.UpdateIngredientDescription(description);
	}

	void MouseLeave()
	{
		Scale = new Vector2(1f, 1f);
	}
}
