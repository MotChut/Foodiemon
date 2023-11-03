using Godot;
using System;

public partial class AIPoints : Node
{
	int SurvivabilityPoint = 0;
    int FavorabilityPoint = 0;

    public int GetSurvivabilityPoint() { return SurvivabilityPoint; }
    public void SetSurvivabilityPoint(int value) { SurvivabilityPoint = value; }
    public int GetFavorabilityPoint() { return FavorabilityPoint; }
    public void SetFavorabilityPoint(int value) { FavorabilityPoint = value; }

    public AIPoints(int s, int f)
    {
        SurvivabilityPoint = s;
        FavorabilityPoint = f;
    }
}
