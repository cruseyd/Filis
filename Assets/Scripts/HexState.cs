using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HexState
{

    private HexTerrain _terrain;
    public HexTerrain terrain { get { return _terrain; } }

    private float _elevation;
    public float elevation { get { return _elevation; } }

    private UnitState _unit = null;
    public UnitState unit { get { return _unit; } set { _unit = value; } }

    // terrain features (plants, etc.)
    // terrain status (e.g. fire, poison)

    public HexState(HexTile tile)
    {
        _terrain = tile.terrain;
        _elevation = tile.elevation;
        _unit = null;
        if (tile.occupant != null)
        {
            _unit = new UnitState(tile.occupant.GetComponent<Unit>());
        }
    }
    public HexState(HexState state)
    {
        _terrain = state._terrain;
        _elevation = state._elevation;
        _unit = new UnitState(state._unit);
    }
    // can unit stand on this tile
    public bool Walkable(UnitState unit) { return (_unit == null); }
    // can unit walk through (but not necessarily stand on) this tile
    public bool Passable(UnitState unit) { return true; }

}
