using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct HexCoords {

	private int _a;
	private int _b;
	private int _c;
	public string name;

	public int a {get {return _a;}}
	public int b {get {return _b;}}
	public int c {get {return _c;}}

    public static HexCoords left { get { return new HexCoords(0, -1, 1); } }
    public static HexCoords right { get { return new HexCoords(0, 1, -1); } }
    public static HexCoords upLeft { get { return new HexCoords(1, -1, 0); } }
    public static HexCoords upRight { get { return new HexCoords(1, 0, -1); } }
    public static HexCoords downLeft { get { return new HexCoords(-1, 0, 1); } }
    public static HexCoords downRight { get { return new HexCoords(-1, 1, 0); } }
    public HexCoords(int a, int b, int c)
	{
		Debug.Assert(a + b + c == 0);
		_a = a; _b = b; _c = c;
		name = "(" + a + ", " + b + ", " + c + ")";
	}

	public List<HexCoords> neighbors() {
		List<HexCoords> n = new List<HexCoords>();
		n.Add(new HexCoords(a,     b + 1, c - 1));
		n.Add(new HexCoords(a - 1, b + 1, c    ));
		n.Add(new HexCoords(a - 1, b,     c + 1));
		n.Add(new HexCoords(a,     b - 1, c + 1));
		n.Add(new HexCoords(a + 1, b - 1, c    ));
		n.Add(new HexCoords(a + 1, b,     c - 1));
		return n;
	}

	public int radius() {
		return Mathf.Max(Mathf.Abs(a), Mathf.Abs(b), Mathf.Abs(c));
	}

	public bool Equals(HexCoords other)
	{
		return ((a == other.a) && (b == other.b) && (c == other.c));
	}

	public static HexCoords operator + (HexCoords x, HexCoords y)
	{
		return new HexCoords(x.a + y.a, x.b + y.b, x.c + y.c);
	}
	public static HexCoords operator - (HexCoords x, HexCoords y)
	{
		return new HexCoords(x.a - y.a, x.b - y.b, x.c - y.c);
	}
	public static HexCoords operator*(HexCoords x, int s)
	{
		return new HexCoords(x.a*s, x.b*s, x.c*s);
	}
	public static HexCoords operator*(int s, HexCoords x)
	{
		return new HexCoords(x.a*s, x.b*s, x.c*s);
	}
    public static bool operator==(HexCoords x, HexCoords y)
    {
        return (x.a == y.a && x.b == y.b && x.c == y.c);
    }
    public static bool operator!=(HexCoords x, HexCoords y)
    {
        return (x.a != y.a || x.b != y.b || x.c != y.c);
    }


	public static HexCoords random(int radius)
	{
		while(true) {
			int a = Random.Range(-radius,radius);
			int b = Random.Range(-radius,radius);
			int c = -(a+b);
			HexCoords ret = new HexCoords(a,b,c);
			if (ret.radius() <= radius){return ret;}
		}
	}
}

