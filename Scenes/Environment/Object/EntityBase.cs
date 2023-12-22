using Godot;

public partial class EntityBase : Object
{
	[Export] public string[] entityTypes;
	public int foods = 0;

    public override void _PhysicsProcess(double delta)
    {
        GetNode<Label>("InteractiveNotice/SubViewport/Label").Text = foods.ToString();
    }

	public void ReceiveFood(int amount)
	{
		foods += amount;
	}
}
