using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Species : ScriptableObject
{
    [SerializeField]
    private string _name;
    public new string name { get { return _name; } }

    [SerializeField]
    private Sprite _sprite;
    public Sprite sprite { get { return _sprite; } }

    [SerializeField]
    private Sprite _icon;
    public Sprite icon { get { return _icon; } }

    public AIModule behavior;

}
