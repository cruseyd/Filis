using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePath
{
    public List<HexTile> path { get; }
    public HexTile start { get { return path[0]; } }
    public HexTile end { get { return path[path.Count - 1]; } }

    private int _current;
    public HexTile current { get { return path[_current]; } }

    public HexTile next
    {
        get
        {
            if (_current < path.Count - 1)
            {
                _current++;
                return path[_current];
            }
            else { return null; }
        }
    }
    public HexTile prev
    {
        get
        {
            if (_current > 0)
            {
                _current--;
                return path[_current];
            }
            else { return null; }
        }
    }
    public bool first { get { return _current == 0; } }
    public bool last { get { return _current == path.Count - 1; } }
    public MovePath(List<HexTile> path)
    {
        this.path = path;
    }
    public MovePath(HexTile start, HexTile end)
    {
        path = new List<HexTile>();
        path.Insert(0, end);
        Node n = end.node;
        while (n.parent != null)
        {
            path.Insert(0, n.parent.tile);
            n = n.parent;
        }
        Debug.Assert(n.tile == start);
        _current = 0;
    }
    public MovePath Reverse()
    {
        List<HexTile> newPath = new List<HexTile>();
        foreach (HexTile tile in path)
        {
            newPath.Insert(0, tile);
        }
        return new MovePath(newPath);
    }
    public void Show(bool flag)
    {
        foreach (HexTile tile in path)
        {
            if (flag) { tile.highlight(Color.cyan); }
            else { tile.resetColor(); }
        }
    }
}
