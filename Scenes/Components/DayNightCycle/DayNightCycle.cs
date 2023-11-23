using Godot;
using static Resources;

public partial class DayNightCycle : Node3D
{
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
