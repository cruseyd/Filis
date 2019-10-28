using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitState
{
    public UnitData data;
    public int currentHealth;
    public bool dead { get { return currentHealth == 0; } }
    // status effect

    public UnitState(Unit unit)
    {
        data = unit.data;
        currentHealth = unit.health.current;
    }
    
    public UnitState(UnitState state)
    {
        data = state.data;
        currentHealth = state.currentHealth;
    }

    public void Damage(int value)
    {
        currentHealth -= value;
        currentHealth = Mathf.Max(currentHealth, 0);
    }

}
