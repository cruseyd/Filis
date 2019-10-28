using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AbilityArgs
{
    public AbilityTarget target;
    public int damage;
    public int duration;
}

[System.Serializable]
public class AbilityComponent
{
    [SerializeField]
    private AbilityType _type;
    public AbilityType type { get { return _type; } }

    [SerializeField]
    private AbilityArgs _args;
    public AbilityArgs args { get { return _args; } }
}

public enum AbilityType
{
    DEFAULT,
    DAMAGE,
    HIGHLIGHT
}

public enum AbilityTarget
{
    DEFAULT,
    SELF,
    TARGET,
    SPLASH
}


[CreateAssetMenu(menuName = "Ability")]
public class Ability : ScriptableObject
{
    [SerializeField]
    private int _range;
    public int range { get { return _range; } }

    [SerializeField]
    private TargeterType _targeterType;
    public TargeterType targeterType { get { return _targeterType; } }

    [SerializeField]
    private List<AbilityComponent> _components;

    [SerializeField]
    private TooltipData _tooltipData;
    public TooltipData tooltipData { get { return _tooltipData; } }

    //public Targeter targeter;

    /*
    public void Define()
    {
        if (targeter != null) { return; }
        switch (targeterType)
        {
            case TargeterType.SINGLE: targeter = new SingleTargeter(this); break;
            case TargeterType.BREATH: targeter = new BreathTargeter(this); break;
            default: Debug.Log("Unrecognized TargeterType: " + targeterType); break;
        }
    }
    
    public void Ready(Unit unit)
    {
        Define();
        targeter.Define(unit, range);
        targeter.FindSelectable();
    }
    
    public List<HexCoords> FindTargets()
    {
        List<HexCoords> range = targeter.FindSelectable();
        List<HexCoords> targets = new List<HexCoords>();
        foreach (HexCoords target in range)
        {
            HexTile tile = Map.current.TileAt(target);
            if (tile.occupant != null) { targets.Add(target); }
        }
        return targets;
    }
    
    public AbilityCommand ChooseTarget(HexTile tile)
    {
        return (AbilityCommand)targeter.ChooseTarget(tile);
    }
    */
    public Targeter Targeter(HexCoords source, Unit unit)
    {
        switch (targeterType)
        {
            case TargeterType.SINGLE: return new SingleTargeter(this, source, unit); 
            case TargeterType.BREATH: return new  BreathTargeter(this, source, unit);
            default: Debug.Log("Unrecognized TargeterType: " + targeterType); break;
        }
        return null;
    }

    public void Activate(HexCoords source, HexCoords target, List<HexCoords> splash, BoardState boardState = null)
    {
        foreach (AbilityComponent comp in _components)
        {
            AbilityManager.abilities[comp.type](source, target, splash, comp.args, boardState);
        }
    }

    public void Undo(HexCoords source, HexCoords target, List<HexCoords> splash, BoardState boardState)
    {
        foreach (AbilityComponent comp in _components)
        {
            AbilityManager.abilities[comp.type](source, target, splash, comp.args, boardState,true);
        }
    }
}
