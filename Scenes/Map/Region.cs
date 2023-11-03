using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

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
	List<Vector2> cells = new List<Vector2>();
	Region nextRegion;
	List<Neighbor> neighbors = new List<Neighbor>();

	public override void _Ready()
	{
		
	}

	public void AddCell(Vector2 cell)
	{
		cells.Add(cell);
	}

	public Vector2 GetFirstCell()
	{
		return cells[0];
	}

	public List<Vector2> GetAllCells()
	{
		return cells;
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
		return cells[rnd.Next(cells.Count)];
	}

	public bool HasCell(Vector2 cell)
	{
		return cells.Contains(cell);
	}
}
