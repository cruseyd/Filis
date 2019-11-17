using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public abstract class CombatPhase {

	public static IdleCombatPhase idle;
	public static MoveCombatPhase move;
    public static AbilityCombatPhase ability;

	public abstract void enter(CombatManager mgr);
	public abstract void processInput(CombatManager mgr);
	public abstract void confirm (CombatManager mgr);
	public abstract void exit(CombatManager mgr);
}
[System.Serializable]
public class IdleCombatPhase : CombatPhase {
  
	public override void enter(CombatManager mgr)
	{
        Map.current.graph.resetNodes();
        
	}
	public override void processInput(CombatManager mgr)
	{
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            mgr.UndoMove();
        }
    }
	public override void confirm (CombatManager mgr){}
	public override void exit(CombatManager mgr)
	{
	}
}

[System.Serializable]
public class MoveCombatPhase : CombatPhase {

    private ICommand moveCommand;
    private MoveTargeter targeter;
	public override void enter(CombatManager mgr)
	{
        targeter = Unit.current.moveTargeter;
        targeter.FindSelectable();
        FieldMap.current.showSelectable();
	}
	public override void processInput(CombatManager mgr)
	{
		if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
		{
            HexTile hitTile = UIManager.GetClickedTile();
            if (hitTile == null) { return; }
            if (hitTile.selectable)
            {
                moveCommand = targeter.ChooseTarget(hitTile, true);
                UIManager.setHexCursor(hitTile);
                FieldMap.current.showSelectable();
                UIManager.button(ButtonName.CONFIRM).gameObject.SetActive(true);
            }
            else
            {
                moveCommand?.Show(false);
            }
        }
	}
	public override void confirm (CombatManager mgr){
        CombatManager.IssueCommand(moveCommand);
        UIManager.button(ButtonName.CONFIRM).gameObject.SetActive(false);
        UIManager.button(ButtonName.MOVE).interactable = false;
        CombatManager.phase = idle;
	}
	public override void exit(CombatManager mgr)
	{
        Map.current.graph.resetNodes();
        UIManager.hexCursor.SetActive(false);
	}
}

[System.Serializable]
public class AbilityCombatPhase : CombatPhase
{
    string[] layerMask = {"Tile"};
    private AbilityCommand command;
    private Targeter targeter;
    public override void enter(CombatManager mgr)
    {
        Ability a = Unit.current.currentAbility;
        targeter = a.Targeter(Unit.current.currentTile.coords, Unit.current);
        targeter.FindSelectable();
        FieldMap.current.showSelectable();
    }

    public override void processInput(CombatManager mgr)
    {
        //Targeter targeter = CombatManager.currentUnit.currentAbility.targeter;
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HexTile hitTile = UIManager.GetClickedTile();
            if (hitTile == null) { return; }
            if (hitTile.selectable)
            {
                command = (AbilityCommand)targeter.ChooseTarget(hitTile, true);
                UIManager.setHexCursor(hitTile, true);
                UIManager.button(ButtonName.CONFIRM).gameObject.SetActive(true);
            }
        }
    }

    public override void confirm(CombatManager mgr)
    {
        CombatManager.IssueCommand(command);
        CombatManager.phase = idle;
    }

    public override void exit(CombatManager mgr)
    {
        CombatManager.instance.StartCoroutine(userAbilityDelay(1));
        UIManager.hexCursor.SetActive(false);
    }

    private IEnumerator userAbilityDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UIManager.setTargetUI(null);

    }
}
