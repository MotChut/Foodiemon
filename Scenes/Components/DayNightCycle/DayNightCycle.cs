using Godot;
using static Resources;

public partial class DayNightCycle : Node3D
{
	AnimationPlayer animationPlayer;
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.SpeedScale = SPEED_SCALE;
		animationPlayer.Advance(50 / SPEED_SCALE);
    }

    public void SetToDay()
	{
		SetCurrentTime(GameTime.Day);
	}
	public void SetToNoon()
	{
		SetCurrentTime(GameTime.Noon);
	}
	public void SetToNight()
	{
		SetCurrentTime(GameTime.Night);
	}
}
