using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Unit : MonoBehaviour
{

    public static Unit current;

    [SerializeField] private Vector3 posOffset = Vector3.zero;
    [SerializeField] private DoubleClick doubleClick;
    [SerializeField] private float moveAnimationSpeed = 5.0f;
    
    private HexTile _currentTile;
    public HexTile currentTile { get { return _currentTile; } }

    private Ability _currentAbility;
    public Ability currentAbility { get { return _currentAbility; } }

    private UnitData _data;
    public UnitData data { get { return _data; } }

    private AIModule _controller;
    public AIModule controller { get { return _controller; } }
    public Ability ability(int index) { return data.abilities[index]; }

    public Health health;
    private float moveAnimationStart;
    private int currentSpeed = 0;
    private Stack<HexTile> moveStack;
    public MoveTargeter moveTargeter;
    public static Unit Spawn(UnitData data, HexTile tile, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);

        Unit unit = obj.GetComponent<Unit>();
        unit.moveStack = new Stack<HexTile>();
        unit.SetPosition(tile);
        unit._data = data;
        unit.health = obj.GetComponent<Health>();
        unit.health.current = data.maxHealth;
        unit.health.max = data.maxHealth;
        unit.moveTargeter = new MoveTargeter(unit);

        if (data.team != Team.PLAYER) { unit._controller = data.species.behavior; }
        else { unit._controller = null; }

        obj.GetComponentInChildren<SpriteRenderer>().sprite = data.species.sprite;
        obj.GetComponentInChildren<SpriteRenderer>().sortingOrder = 1 - tile.coords.a;

        obj.GetComponent<DoubleClick>().subscribe(
            Camera.main.GetComponent<CameraFollow>().panToUnit);


        return unit;
    }
    public void EndTurn()
    {
        health.setDisplay(null);
    }
    public void BeginTurn()
    {
        Camera.main.GetComponent<CameraFollow>().panToUnit(this.transform);
        if (controller != null)
        {
            StartCoroutine(controller.Think(CombatManager.boardState));
        }
    }
    public void SetPosition(HexTile tile)
    {
        _currentTile = tile;
        tile.occupant = this.transform;
        transform.position = currentTile.transform.position + posOffset;
        
    }
    public bool SetAbility(int index)
    {
        _currentAbility = data.abilities[index];
        if (_currentAbility == null) { return false; }
        else { return true; }
    }
    public bool IncrementSpeed()
    {
        currentSpeed += data.speed;
        if (currentSpeed >= CombatManager.speedThreshold)
        {
            return true;
        }
        return false;
    }
    
    public void TakeDamage(int damage)
    {
        Debug.Log(data.species.name + " took " + damage + " damage.");
        health -= damage;
        if (health.current == 0)
        {
            Die();
        }
    }
    public void Die()
    {
        Destroy(this.gameObject);
    }
    public void FollowPath(MovePath path)
    {
        moveTargeter.FindSelectable();
        Debug.Assert(path.start == currentTile);
        if (path.end != currentTile)
        {
            currentTile.occupant = null;
            StartCoroutine(animateMove(path));
        }
    }

    public int MinRange()
    {
        int min = 9999;
        foreach (Ability ability in data.abilities)
        {
            if (ability != null) { min = Mathf.Min(min, ability.range); }
        }
        return min;
    }

    public int MaxRange()
    {
        int max = 0;
        foreach (Ability ability in data.abilities)
        {
            if (ability != null) { max = Mathf.Max(max, ability.range); }
        }
        return max;
    }

    IEnumerator animateMove(MovePath path)
    {
        Transform img = transform.Find("img");
        while (!path.last)
        {
            HexTile target = path.next;
            Vector2 start = transform.position;
            Vector2 end = target.transform.position + posOffset;
            if (end.x - start.x > 0)
            {
                img.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                img.localScale = new Vector3(1, 1, 1);
            }
            moveAnimationStart = Time.time;
            float t = 0;
            while (t < 1)
            {
                t += moveAnimationSpeed * Time.deltaTime;
                if (t > 0.5f && currentTile != target)
                {
                    _currentTile = target;
                    img.GetComponent<SpriteRenderer>().sortingOrder = 1 - target.coords.a;
                }
                transform.position = Vector2.Lerp(start, end, t);
                yield return null;
            }
        }
        SetPosition(path.end);
        CombatManager.UpdateBoardState();
    }
}
