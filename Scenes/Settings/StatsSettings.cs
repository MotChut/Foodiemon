using Godot;

public partial class StatsSettings : Node
{
	public int id;
	public string entityType;
	public int nRaycasts = 32;
	public float raycastLength = 1.5f;
	public float rotAngle = 0.03f;
	public int collectableMax;
	public float minFollowDistance;
	public float maxFollowDistance;
	public float minSpd;
	public float maxSpd;
	public float accel;
	public float friction;
	public int maxHp;
	public int attackPoint;
	public int attackSpd;
	public int maxHunger;
	public int nConsume;
	public int maxGout;
	public int estrousCycle;
	public int estrousDuration;
	public int pregnantDuration;
	public int growDuration;
	public float kidSize;
}
