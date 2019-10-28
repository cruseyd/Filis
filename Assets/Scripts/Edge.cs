using UnityEngine;
using System.Collections;

public class Edge{

	public bool visited = false;
	//public Vector3 position;
	public string name;

	public delegate void visitRoutine(Edge edge);
	public Node[] nodes;
	public Vertex[] vertices;
    
	public Region region1;
	public Region region2;

	public bool visible = false;
	public bool river;
	public bool worldBound;

	public Edge()
	{
		visited = false;
		nodes = new Node[2];
		vertices = new Vertex[2];
	}

	public bool external() {
		if (nodes[0] == null || nodes[1] == null) {return true;}
		else {return false;}
	}

	public Vertex downhill() {
		if (vertices[0].elevation < vertices[1].elevation) {return vertices[0];}
		else {return vertices[1];}
	}

	public Vertex uphill() {
		if (vertices[0].elevation >= vertices[1].elevation) {return vertices[0];}
		else {return vertices[1];}
	}

	public Node downhillNode() {
		if (nodes[0].elevation < nodes[1].elevation) {return nodes[0];}
		else {return nodes[1];}
	}

	public Node uphillNode() {
		if (nodes[0].elevation >= nodes[1].elevation) {return nodes[0];}
		else {return nodes[1];}
	}

	public void Visit(visitRoutine f) {
		if (!visited)
		{
			visited = true;
			f(this);
			for (int ii = 0; ii < 2; ii++)
			{
				for (int jj = 0; jj < 3; jj++)
				{
					if (vertices[ii].paths[jj] != null)
					{
						vertices[ii].paths[jj].Visit(f);
					}
				}
			}
		}
	}

	public static void Reset(Edge edge)
	{
		edge.visited = false;
	}

	public static void Draw(Edge edge)
	{
		if (edge.nodes[0] != null)
		{
			//Debug.DrawLine(edge.position, edge.nodes[0].position,Color.red,9999);
		}
		if (edge.nodes[1] != null)
		{
			//Debug.DrawLine(edge.position, edge.nodes[1].position,Color.red,9999);
		}
		if (edge.vertices[0] != null)
		{
			//Debug.DrawLine(edge.position, edge.vertices[0].position, Color.green, 9999);
		}
		if (edge.vertices[1] != null)
		{
			//Debug.DrawLine(edge.position, edge.vertices[1].position, Color.green, 9999);
		}
	}
}

