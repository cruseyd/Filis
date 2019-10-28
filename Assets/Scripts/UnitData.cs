using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    PLAYER,
    AGGRESSIVE,
    NEUTRAL
}

[System.Serializable]
public class UnitData
{
    public Species species;
    public int movement;
    public int speed;
    public int maxHealth;
    public Ability[] abilities;
    public Team team;
}
