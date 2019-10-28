using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HexGraph{
	
	private int _radius;
    
	public Dictionary<HexCoords, Node> nodes;
	public List<Edge> edges;
	public List<Vertex> vertices;

	public HexGraph(int radius)
	{
		_radius = radius;

		nodes = new Dictionary<HexCoords, Node>();
		edges = new List<Edge>();
		vertices = new List<Vertex>();

		for (int aa = -radius; aa <= radius; aa++)
		{
			for (int bb = -radius; bb <= radius; bb++)
			{
				for (int cc = -radius; cc <= radius; cc++)
				{
					if ((aa + bb + cc) == 0) {
						// create HexCoords
						HexCoords coords = new HexCoords(aa,bb,cc);

						// create and add a new node
						Node node = new Node(coords);
						//node.position = Position(coords);
						nodes[coords] = node;

						// process neighbors
						List<HexCoords> neighbors = coords.neighbors();
						for (int ii = 0; ii < 6; ii++)
						{
							HexCoords nCoords = neighbors[ii];
							// if there's already a node there
							if (nodes.ContainsKey(nCoords))
							{
								// set node neighbors
								node.neighbors[ii] = nodes[nCoords];
								nodes[nCoords].neighbors[(ii + 3) % 6] = node;

								// set node borders
								node.borders[ii] = nodes[nCoords].borders[(ii + 3) % 6];
								Debug.Assert(node.borders[ii].nodes[0] == nodes[nCoords]);
								Debug.Assert(node.borders[ii].nodes[1] == null);
								node.borders[ii].nodes[1] = node;
							} else {
								// otherwise, create corners and borders
								// create new borders
								Debug.Assert(node.borders[ii] == null);
								node.borders[ii] = new Edge();
								node.borders[ii].name = node.name + " | " + nCoords.name;
								//node.borders[ii].position = node.position + dir[ii]/2;
								edges.Add(node.borders[ii]);
								node.borders[ii].nodes[0] = node;
							}
						} //end neighbors loop
					} // end if coords valid
				} // cc
			} // bb
		} // aa

		foreach (Node node in nodes.Values) {
			List<HexCoords> neighbors = node.coords.neighbors();
			for (int ii = 0; ii < 6; ii++)
			{
				HexCoords c1 = neighbors[ii];
				HexCoords c2 = neighbors[(ii + 5) % 6];

				if (node.corners[ii] == null)
				{
					node.corners[ii] = new Vertex();
					vertices.Add(node.corners[ii]);
					//node.corners[ii].position = node.position + dir[ii]/2 + dirPerp[ii]/2;
					node.corners[ii].touches[ii/2] = node;

					if (node.borders[ii].vertices[0] == null)
					{
						node.borders[ii].vertices[0] = node.corners[ii];
					} else {
						Debug.Assert(node.borders[ii].vertices[1] == null);
						node.borders[ii].vertices[1] = node.corners[ii];
					}

					if (node.borders[(ii + 5) % 6].vertices[1] == null) {
						node.borders[(ii + 5) % 6].vertices[1] = node.corners[ii];
					} else {
						Debug.Assert(node.borders[(ii + 5) % 6].vertices[0] == null);
						node.borders[(ii + 5) % 6].vertices[0] = node.corners[ii];
					}

					node.corners[ii].paths[ii/2] = node.borders[ii];
					node.corners[ii].paths[((ii/2) + 1) % 3] = node.borders[(ii + 5) % 6];

					if (nodes.ContainsKey(c1)) // there is a neighboring cell in the ii direction
					{
						nodes[c1].corners[(ii + 4) % 6] = node.corners[ii];
						node.corners[ii].touches[((ii/2) + 2) % 3] = nodes[c1];
						node.corners[ii].paths[((ii/2) + 2) % 3] = nodes[c1].borders[(ii + 4) % 6];
						if (nodes[c1].borders[(ii + 4) % 6].vertices[0] == null) {
							nodes[c1].borders[(ii + 4) % 6].vertices[0] = node.corners[ii];
						} else {
							Debug.Assert(nodes[c1].borders[(ii + 4) % 6].vertices[1] == null);
							nodes[c1].borders[(ii + 4) % 6].vertices[1] = node.corners[ii];
						}
					} //nodes[c1]
					if (nodes.ContainsKey(c2))
					{
						nodes[c2].corners[(ii + 2) % 6] = node.corners[ii];
						node.corners[ii].touches[((ii/2) + 1) % 3] = nodes[c2];
						if (!nodes.ContainsKey(c1))
						{
							node.corners[ii].paths[((ii/2) + 2) % 3] = nodes[c2].borders[(ii + 1) % 6];
							if (nodes[c2].borders[(ii + 1) % 6].vertices[0] == null) {
								nodes[c2].borders[(ii + 1) % 6].vertices[0] = node.corners[ii];
							} else {
								Debug.Assert(nodes[c2].borders[(ii + 1) % 6].vertices[1] == null);
								nodes[c2].borders[(ii + 1) % 6].vertices[1] = node.corners[ii];
							}
						}
					}//nodes[c2]
				} // this neighbor
			} //loop over neighbors
		} // foreach node
		foreach (Node node in nodes.Values) {
			for (int ii = 0; ii < 6; ii++)
			{
				for (int jj = 0; jj < 3; jj++)
				{
					if (node.corners[ii].paths[jj] != null
						&& node.corners[ii].paths[jj].vertices[0] != node.corners[ii])
					{
						node.corners[ii].neighbors[jj] = node.corners[ii].paths[jj].vertices[0];
					} else if (node.corners[ii].paths[jj] != null
						&& node.corners[ii].paths[jj].vertices[1] != node.corners[ii])
					{
						node.corners[ii].neighbors[jj] = node.corners[ii].paths[jj].vertices[1];
					}
				}
			}
		} // foreach node
	}

	public void removeNode(Node node) {
		if (!nodes.ContainsKey(node.coords)){return;}
		for (int ii = 0; ii < 6; ii++)
		{
			for (int jj = 0; jj < 3; jj++)
			{
				if (node.corners[ii].touches[jj] == node)
				{
					node.corners[ii].touches[jj] = null;
				}
			}
			for (int jj = 0; jj < 2; jj++)
			{
				if (node.borders[ii].nodes[jj] == node)
				{
					node.borders[ii].nodes[jj] = null;
				}
			}
		}
		for (int ii = 0; ii < 6; ii++)
		{
			if (node.neighbors[ii] != null)
			{
				Debug.Assert(node.neighbors[ii].neighbors[(ii + 3) % 6] == node);
				node.neighbors[ii].neighbors[(ii + 3) % 6] = null;
			} else {
				Debug.Assert(node.borders[ii].nodes[0] == null);
				Debug.Assert(node.borders[ii].nodes[1] == null);
				removeEdge(node.borders[ii]);
			}
		}
		nodes.Remove(node.coords);
		node = null;
	}

	public void removeEdge(Edge edge) {
		if (edge.nodes[0] != null || edge.nodes[1] != null){return;}
		if (edge.vertices[0] != null)
		{
			for (int ii = 0; ii < 3; ii++)
			{
				if (edge.vertices[0].paths[ii] == edge)
				{edge.vertices[0].paths[ii] = null;}
			}
			removeVertex(edge.vertices[0]);
		}
		if (edge.vertices[1] != null)
		{
			for (int ii = 0; ii < 3; ii++)
			{
				if (edge.vertices[1].paths[ii] == edge)
				{edge.vertices[1].paths[ii] = null;}
			}
			removeVertex(edge.vertices[1]);
		}
		edges.Remove(edge);
		edge = null;
	}

	public void removeVertex(Vertex vertex)
	{
		for (int ii = 0; ii < 3; ii++)
		{
			if (vertex.touches[ii] != null) {return;}
		}
		for (int ii = 0; ii < 3; ii++)
		{
			if (vertex.paths[ii] != null)
			{
				for (int jj = 0; jj < 2; jj++)
				{
					if (vertex.paths[ii].vertices[jj] == vertex)
					{
						vertex.paths[ii].vertices[jj] = null;
					}
				}
			}
			if (vertex.neighbors[ii] != null)
			{
				for (int jj = 0; jj < 3; jj++)
				{
					if (vertex.neighbors[ii].neighbors[jj] == vertex)
					{
						vertex.neighbors[ii].neighbors[jj] = null;
					}
				}
			}
		}
		vertices.Remove(vertex);
		vertex = null;
	}

	public Node Center() {
		HexCoords coords = new HexCoords(0,0,0);
		return nodes[coords];
	}

	public void reset() {
		resetNodes ();
		resetEdges ();
		resetVertices ();
	}

	public void resetNodes() {
		foreach (Node node in nodes.Values)
		{
			Node.Reset(node);
		}
	}

	public void resetEdges() {
		foreach (Edge edge in edges)
		{
			Edge.Reset(edge);
		}
	}

	public void resetVertices() {
		foreach (Vertex vertex in vertices)
		{
			vertex.visited = false;
		}
	}

	public void ShowEdges(bool flag) {
		foreach (Edge e in edges)
		{
			e.visible = flag;
		}
	}
	public void DrawNodes() {
		Center().Visit(Node.Draw);
	}
	public void DrawEdges() {
		Center().borders[0].Visit(Edge.Draw);
	}
	public void DrawVertices() {
		Center().corners[0].Visit(Vertex.Draw);
	}
    
	public Node RandomNode()
	{
		HexCoords coords = HexCoords.random(_radius);
		return nodes[coords];
	}

    public Node NodeAt(HexCoords coords)
    {
        if (nodes.ContainsKey(coords))
        {
            return nodes[coords];
        }
        return null;
    }
}
