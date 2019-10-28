using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct VisitData {
	public Region region;
	public HexCoords origin;
	public float value;
}


public class Node {

	public Node[] neighbors;
	public Edge[] borders;
	public Vertex[] corners;
	public HexCoords coords;

	public bool visited = false;
	public bool selectable = false;
	public Node parent = null;
	public int distance;

	public string name;

	public HexTile tile;
	public Region region1;
	public Region region2;
	public Region region3;

    // map generation stuff
	public float compression;
	public float elevation;
	public float rainfall = 0.0f;
	public float temperature;
	public bool water;
	public bool ocean;
	public bool swamp;

	public delegate void visitRoutine(Node node);
	public delegate void visitRoutineArgs(Node node, VisitData visitData);

	public Node(HexCoords _coords)
	{
		visited = false;
		coords = _coords;
		name = "(" + coords.a + ", " + coords.b + ", " + coords.c + ")";
		neighbors = new Node[6];
		borders = new Edge[6];
		corners = new Vertex[6];
	}

    public bool Walkable(UnitData unit) { return tile.Walkable(unit); }
    public bool Passable(UnitData unit) { return tile.Passable(unit); }

    public int numRegions() {
		int n = 0;
		if (region1 != null) {n++;}
		if (region2 != null) {n++;}
		if (region3 != null) {n++;}
		return n;
	}
	public int numNeighbors() {
		int n = 0;
		for (int ii = 0; ii < 6; ii++)
		{
			if (neighbors[n] != null) {n++;}
		}
		return n;
	}

	public bool river() {
		for (int ii = 0; ii < 6; ii++)
		{
			if (borders[ii].river){return true;}
		}
		return false;
	}

	public bool seaLevel() {
		if (elevation < 0) {return false;}
		foreach (Node n in neighbors)
		{
			if (n != null && n.elevation < 0)
			{
				return true;
			}
		}
		return false;
	}

	public Vertex downhillCorner() {
		float low = 2;
		Vertex ret = null;
		foreach (Vertex v in corners) {
			if (v.elevation < low)
			{
				low = v.elevation;
				ret = v;
			}
		}
		return ret;
	}

	public void Visit(visitRoutine f) {
		if (!visited)
		{
			visited = true;
			f(this);
			foreach (Node node in neighbors)
			{
				if (node != null)
				{
					node.Visit(f);
				}
			}
		}
	}

	public void LocalVisit(visitRoutine f, HexCoords origin, int distance) {
		if (!visited && (this.coords - origin).radius() <= distance)
		{
			visited = true;
			f(this);
			foreach (Node node in neighbors)
			{
				if (node != null)
				{
					node.LocalVisit(f, origin, distance);
				}
			}
		}
	}
	public void Visit(visitRoutineArgs f, VisitData args) {
		if (!visited)
		{
			visited = true;
			f(this, args);
			foreach (Node node in neighbors)
			{
				if (node != null)
				{
					node.Visit(f, args);
				}
			}
		}
	}

	public void LocalVisit(visitRoutineArgs f, VisitData args, HexCoords origin, int distance) {
		if (!visited && (this.coords - origin).radius() <= distance)
		{
			visited = true;
			f(this, args);
			foreach (Node node in neighbors)
			{
				if (node != null)
				{
					node.LocalVisit(f, args, origin, distance);
				}
			}
		}
	}

	public static void Reset(Node node)
	{
		node.parent = null;
		node.selectable = false;
		node.visited = false;
        if (node.tile != null)
        {
            node.tile.resetColor();
        }
	}

	public static void Draw(Node node)
	{
		for (int ii = 0; ii < 6; ii++)
		{
			if (node.neighbors[ii] != null)
			{
                /*
				Debug.DrawLine(node.position,
					node.neighbors[ii].position,
					Color.red, 999);
                    */
			}
		}
	}
}

