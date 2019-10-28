using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Vertex {

	public bool visited = false;
	//public Vector3 position;
	public delegate void visitRoutine(Vertex vertex);
	public Node[] touches;
	public Edge[] paths;
	public Vertex[] neighbors;

	public float elevation;
	public bool riverStart;
	public bool riverEnd;

	public Vertex() {
		visited = false;
		touches = new Node[3];
		paths = new Edge[3];
		neighbors = new Vertex[3];
	}

	public Edge downhill() {
		float h = elevation;
		Edge ret = null;
		for (int ii = 0; ii < 3; ii++)
		{
			if (neighbors[ii].elevation < h)
			{
				h = neighbors[ii].elevation;
				ret = paths[ii];
			}
		}
		return ret;
	}

	public Edge randomDownhill() {
		List<Edge> edges = new List<Edge>();
		for (int ii = 0; ii < 3; ii++)
		{
			if (neighbors[ii] != null && neighbors[ii].elevation < elevation)
			{
				edges.Add(paths[ii]);
			}
		}
		if (edges.Count == 0) {return null;}
		else {
			return edges[Random.Range(0,edges.Count)];
		}
	}

	public bool seaLevel() {
		for (int ii = 0; ii < 3; ii ++)
		{
			if (touches[ii] != null && touches[ii].elevation < 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool coastal() {
		if (seaLevel()) {return true;}
		for (int ii = 0; ii < 3; ii++)
		{
			if (touches[ii] != null && touches[ii].water) {return true;}
		}
		return false;
	}

	public bool external() {
		foreach (Node n in touches)
		{
			if (n == null){return true;}
		}
		return false;
	}

	public void Visit(visitRoutine f)
	{
		if(!visited)
		{
			visited = true;
			f(this);
			for (int ii = 0; ii < 3; ii++)
			{
				if (neighbors[ii] != null)
				{
					neighbors[ii].Visit(f);
				}
			}
		}
	}

	public static void Draw(Vertex vertex)
	{
		for (int ii = 0; ii < 3; ii++)
		{
			if (vertex.neighbors[ii] != null)
			{
                /*
				Debug.DrawLine(vertex.position,
					vertex.neighbors[ii].position,
					Color.cyan, 9999);
                    */
			}
		}
	}

	public static void Reset(Vertex vertex)
	{
		vertex.visited = false;
	}
}


