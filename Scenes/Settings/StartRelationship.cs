using System.Collections.Generic;
using System.Linq;
using Godot;
using static Resources;

public partial class StartRelationship : Node
{
	public int id = 0;
	public string entity = "";
	public string player = "";
	public string chicpea = "";
	public string rawrberry = "";
    public List<Relationship> playerRelationship = new List<Relationship>();
    public List<Relationship> chicpeaRelationship = new List<Relationship>();
    public List<Relationship> rawrberryRelationship = new List<Relationship>();

    public void Init()
    {
        GetStartRelationship(player, playerRelationship);
        GetStartRelationship(chicpea, chicpeaRelationship);
        GetStartRelationship(rawrberry, rawrberryRelationship);
    }

    void GetStartRelationship(string s, List<Relationship> list)
    {
        List<string> relationships = s.Split(";").ToList();
        foreach(string r in relationships)
        {
            list.Add((Relationship)Relationship.Parse(typeof(Relationship), r));
        }
    }
}
