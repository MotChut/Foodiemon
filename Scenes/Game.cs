using Godot;

public partial class Game : Node3D
{
	Player player;

    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        
    }

    public override void _Process(double delta)
	{
	}

    public Camera3D GetCamera()
    {
        return GetNode<Camera3D>("Camera/Camera3D");
    }
}
