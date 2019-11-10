using UnityEngine;
using System.Collections.Generic;

public enum TargeterType
{
    DEFAULT,
    MOVE,
    SINGLE,
    BREATH
}

public abstract class Targeter
{
    protected Unit _unit;
    protected int _range;
    protected HexCoords _source;
    protected HexCoords _target;
    protected List<HexCoords> _splash;

    public abstract List<HexCoords> FindSelectable(HexCoords source);
    public List<HexCoords> FindSelectable() { return FindSelectable(_unit.currentTile.coords); }
    public abstract ICommand ChooseTarget(HexTile tile, bool preview = false);
    public abstract void Show(bool flag);
    public static List<HexCoords> TilesAt(Node start, int _range)
    {
        List<HexCoords> selectable = new List<HexCoords>();
        Map.current.graph.resetNodes(); //should be whatever current map exists
        Queue<Node> process = new Queue<Node>();
        process.Enqueue(start);
        start.visited = true;
        start.distance = 0;

        while (process.Count > 0)
        {
            Node n = process.Dequeue();
            n.selectable = true;
            if (n.distance < _range)
            {
                foreach (Node node in n.neighbors)
                {
                    if (node != null
                        && !node.visited)
                    {
                        node.parent = n;
                        node.distance = n.distance + 1;
                        node.visited = true;
                        process.Enqueue(node);
                        if (node.distance == _range)
                        {
                            selectable.Add(node.coords);
                        }
                    }
                }
            }
        }
        return selectable;
    }
    public static List<HexCoords> TilesWithin(Node start, int _range)
    {
        List<HexCoords> selectable = new List<HexCoords>();
        Map.current.graph.resetNodes(); //should be whatever current map exists
        Queue<Node> process = new Queue<Node>();
        process.Enqueue(start);
        start.visited = true;
        start.distance = 0;

        while (process.Count > 0)
        {
            Node n = process.Dequeue();
            n.selectable = true;
            if (n.distance < _range)
            {
                foreach (Node node in n.neighbors)
                {
                    if (node != null
                        && !node.visited)
                    {
                        node.parent = n;
                        node.distance = n.distance + 1;
                        node.visited = true;
                        process.Enqueue(node);
                        selectable.Add(node.coords);
                    }
                }
            }
        }
        return selectable;
    }
    public List<HexCoords> FindTargets() { return FindTargets(_unit.currentTile.coords); }
    public List<HexCoords> FindTargets(HexCoords src)
    {
        List<HexCoords> range = FindSelectable(src);
        List<HexCoords> targets = new List<HexCoords>();
        foreach (HexCoords target in range)
        {
            UnitState data = CombatManager.boardState.board[target].unit;
            if (data != null) { targets.Add(target); }
        }
        return targets;
    }

}

public class MoveTargeter : Targeter
{
    private MovePath _path;

    public MoveTargeter(Unit unit)
    {
        _unit = unit;
        _source = unit.currentTile.coords;
        _range = unit.data.movement;
        _splash = new List<HexCoords>();
    }
    public override List<HexCoords> FindSelectable(HexCoords src)
    {
        _source = src;
        List<HexCoords> selectable = new List<HexCoords>();
        UnitData unit = CombatManager.boardState.board[_source].unit.data;
        Node start = Map.current.TileAt(_source).node;
        Map.current.graph.resetNodes();
        Queue<Node> process = new Queue<Node>();
        process.Enqueue(start);
        start.visited = true;
        start.distance = 0;
        FieldMap fieldMap = (FieldMap)Map.current;
        while (process.Count > 0)
        {
            Node n = process.Dequeue();
            if (n.Passable(unit) || n.distance == 0)
            {
                if (n.Walkable(unit))
                {
                    n.selectable = true;
                }
                if (n.distance < _range)
                {
                    foreach (Node node in n.neighbors)
                    {
                        if (node == null) { continue; }
                        int delta = Mathf.Abs(fieldMap.GetHeight(n.coords) - fieldMap.GetHeight((node.coords)));
                        if (!node.visited
                            && node.Passable(unit)
                            && (node.tile.terrain.movementCost + n.distance) <= _range
                            && (delta < 3))
                        {

                            node.parent = n;
                            node.distance = node.tile.terrain.movementCost + n.distance;
                            node.visited = true;
                            process.Enqueue(node);
                            if (node.Walkable(unit))
                            {
                                selectable.Add(node.coords);
                            }
                        }
                    }
                }
            }
        }
        return selectable;
    }
    public override ICommand ChooseTarget(HexTile tile, bool preview)
    {
        Show(false);
        if (tile.selectable)
        {
            _path = new MovePath(Map.current.TileAt(_source), tile);
            Show(preview);
            return new MoveCommand(_unit, _path);
        }
        else
        { return null; }
    }
    public override void Show(bool flag)
    {
        _path?.Show(flag);
    }
}

public class SingleTargeter : Targeter
{
    private Ability _ability;
    public SingleTargeter(Ability ability, HexCoords source, Unit unit)
    {
        _unit = unit;
        _ability = ability;
        _source = source;
        _range = ability.range;
        _splash = new List<HexCoords>();
    }
    public override List<HexCoords> FindSelectable(HexCoords src)
    {
        _source = src;
        return Targeter.TilesWithin(Map.current.TileAt(src).node, _range);
    }

    public override ICommand ChooseTarget(HexTile tile, bool preview)
    {
        Show(false);
        if (tile.selectable)
        {
            _target = tile.coords;
            Show(preview);
            return new AbilityCommand(_ability, _source, _target, _splash);
        }
        else
        { return null; }
    }

    public override void Show(bool flag)
    {
        if (flag)
        {
            Map.current.TileAt(_target)?.highlight(Color.red);
        } else
        {
            Map.current.TileAt(_target)?.resetColor();
        }
    }
}

public class BreathTargeter : Targeter
{
    private Ability _ability;

    public BreathTargeter(Ability ability, HexCoords source, Unit unit)
    {
        _unit = unit;
        _ability = ability;
        _source = source;
        _range = ability.range;
        _splash = new List<HexCoords>();
    }
public override List<HexCoords> FindSelectable(HexCoords src)
    {
        _source = src;
        return Targeter.TilesWithin(Map.current.TileAt(src).node, 1);
    }

    public override ICommand ChooseTarget(HexTile tile, bool preview)
    {
        Show(false);
        FieldMap.current.showSelectable();
        if (tile.selectable)
        {

            _target = tile.coords;
            _splash.Clear();
            HexCoords next = tile.coords + (tile.coords - _source);
            if (Map.current.TileAt(next) != null)
            {
                _splash.Add(next);
            }
            List<HexCoords> n1 = tile.coords.neighbors();
            List<HexCoords> n2 = next.neighbors();
            foreach(HexCoords n in n1)
            {
                if (n2.Contains(n))
                {
                    HexTile splashTile = Map.current.TileAt(n);
                    if (splashTile != null)
                    {
                        _splash.Add(n);
                    }
                }
            }

            Show(preview);
            return new AbilityCommand(_ability, _source, _target, _splash);
        }
        else
        { return null; }
    }

    public override void Show(bool flag)
    {
        if (flag)
        {
            Map.current.TileAt(_target)?.highlight(Color.red);
            foreach (HexCoords s in _splash)
            {
                Map.current.TileAt(s).highlight(Color.yellow);
            }
        } else
        {
            Map.current.TileAt(_target)?.resetColor();
            foreach (HexCoords s in _splash)
            {
                Map.current.TileAt(s).resetColor();
            }
        }
    }

}
