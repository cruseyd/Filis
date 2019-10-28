using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Region {
	
	public List<Node> nodes;
	public Node center;
	public Vector3 tectonicForce;
	private static int _numRegions = 0;
	public int numRegions { get {return _numRegions; }}
	public int regionID;

	public Region(Node _center)
	{
		Vector2 tect = Random.insideUnitCircle.normalized;
		tectonicForce = new Vector3(tect.x, 0, tect.y);
		center = _center;
		regionID = _numRegions;
		_numRegions++;
		nodes = new List<Node>();
	}


}
