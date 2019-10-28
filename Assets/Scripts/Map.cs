using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Map : MonoBehaviour
{
    public static Map current;

    public GameObject hexPrefab;
    public HexGraph graph;

    protected static Vector2 ab;
    protected static Vector2 ba;
    protected static Vector2 bc;
    protected static Vector2 cb;
    protected static Vector2 ca;
    protected static Vector2 ac;

    protected static Vector2[] dir;

    public abstract void Generate();
    public abstract void SpawnTiles();
    public Transform Occupant(HexCoords coords) { return TileAt(coords).occupant; }
    public abstract HexTile TileAt(HexCoords coords);
    public static Vector2 Position(HexCoords coords)
    {
        if (dir == null)
        {
            dir = new Vector2[6];
            for (int ii = 0; ii < 6; ii++)
            {
                dir[ii] = new Vector2(Mathf.Cos(ii * Mathf.PI / 3),
                                     -Mathf.Sin(ii * Mathf.PI / 3)
                                     * Mathf.Sqrt(2.0f) / 2); //projection
            }
        }

        bc = dir[0];
        ba = dir[1];
        ca = dir[2];
        cb = dir[3];
        ab = dir[4];
        ac = dir[5];

        int a = coords.a;
        int b = coords.b;
        int c = coords.c;
        int absA = Mathf.Abs(a);
        int absB = Mathf.Abs(b);
        int absC = Mathf.Abs(c);
        if (absA >= absB && absA >= absC)
        {
            return b * ba + c * ca;
        }
        else if (absB >= absA && absB >= absC)
        {
            return a * ab + c * cb;
        }
        else
        {
            return a * ac + b * bc;
        }
    }
}


