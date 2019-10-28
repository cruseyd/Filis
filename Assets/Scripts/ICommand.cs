using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommand
{
    void Execute(bool theoretical = false);
    void Undo(bool theoretical = false);
    void Show(bool flag);
}

public class MoveCommand : ICommand
{

    private Unit unit;
    public MovePath path { get; }

    public MoveCommand(Unit unit, HexCoords src, HexCoords end)
    {
        this.unit = unit;
        unit.moveTargeter.FindSelectable(src);
        path = new MovePath(Map.current.TileAt(src), Map.current.TileAt(end));
    }

    public MoveCommand(Unit unit, MovePath path)
    {
        this.unit = unit;
        this.path = path;
    }

    public void Execute(bool theoretical)
    {
        if (path.start.coords == path.end.coords) { return; }
        if (theoretical)
        {
            CombatManager.boardState.MoveUnit(path.start.coords, path.end.coords);
        } else
        {
            unit.FollowPath(path);
        }
    }

    public void Undo(bool theoretical)
    {
        if (theoretical)
        {
            CombatManager.boardState.MoveUnit(path.end.coords, path.start.coords);
        }
        else
        {
            unit.FollowPath(path.Reverse());
        }
    }
    public void Show(bool flag)
    {
        if (flag) { path.Show(true); }
        else { path.Show(false); }
    }
}

public class AbilityCommand : ICommand
{
    private Ability ability;
    private HexCoords source;
    private HexCoords target;
    private List<HexCoords> splash;
    public AbilityCommand() { this.ability = null;}
    public AbilityCommand(Ability ability, HexCoords source, HexCoords target, List<HexCoords> splash)
    {
        this.ability = ability;
        this.source = source;
        this.target = target;
        this.splash = splash;
    }
    public void Execute(bool theoretical)
    {
        if (theoretical)
        {
            ability?.Activate(source, target, splash, CombatManager.boardState);
        }
        else
        {
            Debug.Log("Using " + ability);
            ability?.Activate(source, target, splash);
        }
    }

    public void Undo(bool theoretical)
    {
        
        if (theoretical)
        {
            ability?.Undo(source, target, splash, CombatManager.boardState);
        }
        else
        {
            Debug.Log("Tried to undo a non-theoretical ability");
        }
        return;
    }

    public void Show(bool flag)
    {
        
        if (ability == null) { return; }
        if (flag)
        {
            Map.current.TileAt(target)?.highlight(Color.red);
            foreach (HexCoords s in splash)
            {
                Map.current.TileAt(s).highlight(Color.yellow);
            }
            if (Map.current.Occupant(target) != null)
            {
                UIManager.setTargetUI(Map.current.TileAt(target), true);
            }
        }
        else
        {
            Map.current.TileAt(target)?.resetColor();
            foreach (HexCoords s in splash)
            {
                Map.current.TileAt(s).resetColor();
            }

            if (Map.current.Occupant(target) != null)
            {
                UIManager.setTargetUI(null);
            }
        }
        
    }
}