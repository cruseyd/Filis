using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class CombatManager : MonoBehaviour {

	public static CombatManager instance;

	public static int speedThreshold = 20;
	public List<UnitData> playerUnitData; //temporary
	public GameObject unitPrefab; //temporary

	private List<Unit> _playerUnits;
    private List<Unit> _enemyUnits;
    
	private Queue<Unit> _readyUnits;
	
	private CombatPhase _phase;
    public static CombatPhase phase {
        get { return instance._phase; }
        set
        {
            if (instance._phase != null)
            {
                instance._phase.exit(instance);
            }
            instance._phase = value;

            instance._phase.enter(instance);
        }
    }
    private Stack<ICommand> _actionStack;
    //private Unit _currentUnit = null;
    //public static Unit currentUnit {get {return instance._currentUnit;}}
    //private FieldMap _map;
    //public static FieldMap map {get {return instance._map;}}
    private BoardState _boardState;
    public static BoardState boardState { get { return instance._boardState; } }

	void Awake () {
		if (instance == null) {instance = this;}
		else if (instance != this) {Destroy(this.gameObject);}

		if (CombatPhase.idle == null) {
			CombatPhase.idle = new IdleCombatPhase ();
		}
		if (CombatPhase.move == null) {
			CombatPhase.move = new MoveCombatPhase ();
		}
        if (CombatPhase.ability == null)
        {
            CombatPhase.ability = new AbilityCombatPhase();
        }
    }

	void Start () {
		//_map = FieldMap.instance;
		_playerUnits = new List<Unit>();
		_readyUnits = new Queue<Unit>();
        _actionStack = new Stack<ICommand>();
		// temporary code to add some units for testing
		foreach (UnitData data in playerUnitData)
		{
			Node node = Map.current.graph.RandomNode();
			if (node.tile.occupant == null)
			{
				Unit unit = Unit.Spawn(data,  node.tile, unitPrefab);
				_playerUnits.Add(unit.GetComponent<Unit>());
			}
		}
        UpdateBoardState();
        _boardState.Show(true);
		Next();
	}

	void Update () {
        string[] layerMask = { "Tile" };
        if (Input.GetMouseButtonDown(1))
        {
            phase = CombatPhase.idle;
            UIManager.setTargetUI(null);
            UIManager.setHexCursor(Unit.current.currentTile);
        }
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hit =
                Physics2D.RaycastAll(mousePos, Vector2.zero, 9999, LayerMask.GetMask(layerMask));
            if (hit.Length > 0)
            {
                HexTile tile = hit[0].transform.GetComponent<HexTile>();
                for (int ii = 1; ii < hit.Length; ii++)
                {
                    if (hit[ii].transform.position.y < tile.transform.position.y) { tile = hit[ii].transform.GetComponent<HexTile>(); }
                }
                //HexTile tile = hit.transform.GetComponent<HexTile>();
                UIManager.setHexCursor(tile, true);
                if (tile != Unit.current.currentTile)
                {
                    UIManager.setTargetUI(tile, true);
                } else
                {
                    UIManager.setTargetUI(null);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _boardState.Show(true);
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _boardState.Show(false);
        }
		_phase.processInput (this);
	}

    public static void UpdateBoardState()
    {
        instance._boardState = new BoardState(FieldMap.current, Unit.current);
    }

    
	public static void Next() {
        // end turn of the current unit
        phase = CombatPhase.idle;
		if (Unit.current != null) {
			Unit.current.EndTurn();
		}
        instance._actionStack.Clear();
        // increment speed of remaining units, add to ready queue
		while (instance._readyUnits.Count == 0)
		{
			foreach (Unit unit in instance._playerUnits)
			{
				if (unit.IncrementSpeed ()) {
					instance._readyUnits.Enqueue (unit);
				}
			}
		}
        // grab the next ready unit
        Unit.current = instance._readyUnits.Dequeue();
        UIManager.setUnitUI(Unit.current, true);
        UIManager.setTargetUI(null);
        Unit.current.BeginTurn();
    }

    public static void IssueCommand(ICommand command)
    {
        if (command != null)
        {
            command.Execute();
            if (command.GetType() == typeof(MoveCommand))
            {
                instance._actionStack.Push(command);
            } else
            {
                instance._actionStack.Clear();
            }
        }
        UpdateBoardState();
    }

    public void UndoMove()
    {
        if (_actionStack.Count == 0) { return; }
        if (_actionStack.Peek().GetType() == typeof(MoveCommand))
        {
            _actionStack.Pop().Undo();
            UIManager.button(ButtonName.MOVE).interactable = true;
        }
        UpdateBoardState();
    }

    // UI Buttons
    public void ready_1() {
        if (Unit.current.SetAbility(0))
        {
            phase = CombatPhase.ability;
        } else
        {
            phase = CombatPhase.idle;
        }
    }
    public void ready_2() {
        if (Unit.current.SetAbility(1))
        {
            phase = CombatPhase.ability;
        }
        else
        {
            phase = CombatPhase.idle;
        }

    }
    public void ready_3() {
        if (Unit.current.SetAbility(2))
        {
            phase = CombatPhase.ability;
        }
        else
        {
            phase = CombatPhase.idle;
        }
    }
    public void ready_4() {
        if (Unit.current.SetAbility(3))
        {
            phase = CombatPhase.ability;
        }
        else
        {
            phase = CombatPhase.idle;
        }
    }

    public void NextButton() { Next(); }
    public void ConfirmButton()
    {
        phase.confirm(this);
        phase = CombatPhase.idle;
    }

    public void MoveButton() { phase = CombatPhase.move; }
}
