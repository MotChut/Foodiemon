using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using static Resources;

public partial class Region : Node
{
	public struct Neighbor
	{
		public Region region;
		public Vector2 dir;

		public Neighbor(Region r, Vector2 d)
		{
			region = r;
			dir = d;
		}
	}

	const int MAX_NEIGHBORS = 3;
	List<Vector2> terrains = new List<Vector2>();
	List<Neighbor> neighbors = new List<Neighbor>();
	Region nextRegion;
	RegionType regionType;

	public Region(RegionType regionType)
	{
		this.regionType = regionType;
	}
	

	public void AddCell(Vector2 cell)
	{
		terrains.Add(cell);
	}

	public Vector2 GetFirstCell()
	{
		return terrains[0];
	}

	public List<Vector2> GetAllCells()
	{
		return terrains;
	}

	public bool CanBeAdded()
	{
		return neighbors.Count < MAX_NEIGHBORS;
	}

	public void AddNeighbor(Region region, Vector2 dir)
	{
		neighbors.Add(new Neighbor(region, dir));
	}

	public bool HasNeighbor()
	{
		return neighbors.Count > 0 ? true : false;
	}

	public List<Neighbor> GetNeighbors()
	{
		return neighbors;
	}

	public Vector2 GetRandomCell()
	{
		Random rnd = new Random();
		return terrains[rnd.Next(terrains.Count)];
	}

	public bool HasCell(Vector2 cell)
	{
		return terrains.Contains(cell);
	}
}
