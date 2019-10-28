using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    private HealthBar _healthBar;

    private int _max;
    public int max
    {
        get { return _max; }
        set
        {
            _max = value;
            if (_healthBar != null)
            {
                _healthBar.setHealthPercent(_current, _max);
            }
        }
    }

    private int _current;
    public int current{
        get { return _current; }
        set {
            _current = value;
            if (_healthBar != null)
            {
                _healthBar.setHealthPercent(_current, _max);
            }
        }
    }

    public void setDisplay (HealthBar display)
    {

        _healthBar = display;
        if (_healthBar != null)
        {
            _healthBar.setHealthPercent(_current, _max);
        }
    }

    public static Health operator + (Health health, int value)
    {
        health.current += value;
        return health;
    }
    public static Health operator - (Health health, int value)
    {
        health.current -= value;
        return health;
    }
}
