using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainType
{
    DEFAULT,
    F_GRASS
}

public abstract class HexTerrain : ScriptableObject
{
    public int movementCost = 1;
    public Color color;
    public Sprite tileSprite;
    public Sprite foreground;
    public Sprite background;
}
