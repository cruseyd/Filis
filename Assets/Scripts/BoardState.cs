using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardState
{
    public Dictionary<HexCoords, HexState> board;
    public List<HexCoords> unitLocations;
    private HexGraph graph;
    public HexCoords position;
    public BoardState(FieldMap map, Unit current)
    {
        unitLocations = new List<HexCoords>();
        position = new HexCoords(0, 0, 0);
        if (current)
        {
            position = current.currentTile.coords;
        }
        board = new Dictionary<HexCoords, HexState>();
        graph = Map.current.graph;
        graph.resetNodes();
        graph.Center().Visit(
            (n) =>
            {
                board[n.coords] = new HexState(n.tile);
                if (n.tile.occupant != null)
                {
                    unitLocations.Add(n.coords);
                }
            });
        graph.resetNodes();
    }

    public void Show(bool flag)
    {
        Debug.Log("showing board state");
        graph.resetNodes();
        graph.Center().Visit(
            (n) =>
            {
                if (board[n.coords].unit != null)
                {
                    if (flag)
                    {
                        n.tile.highlight(Color.red);
                    } else { n.tile.resetColor(); }
                    
                }
            }
            );
    }
    public void MoveUnit(HexCoords start, HexCoords end)
    {
        Debug.Assert(board[start].unit != null);
        board[end].unit = board[start].unit;
        board[start].unit = null;
        int index = unitLocations.IndexOf(start);
        unitLocations.RemoveAt(index);
        unitLocations.Add(end);
    }
    public HexCoords NearestUnit(HexCoords origin)
    {
        HexCoords nearest = new HexCoords(0, 0, 0);
        int distance = 9999;
        foreach (HexCoords c in unitLocations)
        {
            if (c != origin)
            {
                float r = (c - origin).radius();
                if (r < distance)
                {
                    nearest = c;
                }
            }
        }
        return nearest;
    }
    public int TeamHealth(Team team)
    {
        int total = 0;
        foreach (HexCoords c in unitLocations)
        {
            UnitState unit = board[c].unit;
            if (unit.data.team == team)
            {
                total += board[c].unit.currentHealth;
            }
        }
        return total;
    }
    public int NumUnits(Team team)
    {
        int total = 0;
        foreach (HexCoords c in unitLocations)
        {
            UnitState unit = board[c].unit;
            if (unit.data.team == team && !unit.dead)
            {
                total++;
            }
        }
        return total;
    }
}
