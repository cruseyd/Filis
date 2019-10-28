using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIMove
{
    SHORT_RANGE,
    LONG_RANGE,
    FLEE
}


[CreateAssetMenu(menuName = "AIModule")]
public class AIModule : ScriptableObject
{
    [SerializeField] private AIMove _type = AIMove.SHORT_RANGE;
    public List<ICommand> FindMoves(BoardState boardState)
    {

        List<ICommand> moves = new List<ICommand>();
        Unit unit = Unit.current;
        List<HexCoords> movement = Unit.current.moveTargeter.FindSelectable();
        HexCoords nearestUnit = boardState.NearestUnit(unit.currentTile.coords);

        switch (_type)
        {
            case AIMove.SHORT_RANGE:
                {
                    List<HexCoords> inRange = Targeter.TilesAt(Map.current.TileAt(nearestUnit).node, unit.MinRange());
                    foreach (HexCoords coords in inRange)
                    {
                        if (movement.Contains(coords))
                        {
                            moves.Add(new MoveCommand(unit, unit.currentTile.coords, coords));
                        }
                    }
                    break;
                }
                
            case AIMove.LONG_RANGE:
                {
                    List<HexCoords> inRange = Targeter.TilesAt(Map.current.TileAt(nearestUnit).node, unit.MaxRange());
                    Debug.Log(inRange.Count);
                    foreach (HexCoords coords in inRange)
                    {
                        if (movement.Contains(coords))
                        {
                            moves.Add(new MoveCommand(unit, unit.currentTile.coords, coords));
                        }
                    }
                    break;
                }
            default:
                break;

        }
        if (moves.Count == 0)
        {
            HexCoords nearest = unit.currentTile.coords;
            int dist = (nearestUnit - nearest).radius();
            foreach (HexCoords coords in movement)
            {
                int d = (nearestUnit - coords).radius();
                if (d < dist)
                {
                    dist = d;
                    nearest = coords;
                }
            }
            moves.Add(new MoveCommand(unit, unit.currentTile.coords, nearest));
        }
        return moves;
    }
    public List<ICommand> ChooseAbility(BoardState boardState, List<ICommand> movement)
    {
        float score = Mathf.NegativeInfinity;
        MoveCommand bestMove = (MoveCommand)movement[0];
        AbilityCommand bestSkill = new AbilityCommand();
        List<ICommand> bestAction = new List<ICommand>();
        
        foreach (ICommand move in movement)
        {
            move.Execute(true);
            
            foreach (Ability ability in Unit.current.data.abilities)
            {
                HexCoords position = ((MoveCommand)move).path.end.coords;
                Targeter targeter = ability.Targeter(position, Unit.current);
                List<HexCoords> targets = targeter.FindTargets(position);
                
                foreach (HexCoords t in targets)
                {
                    HexTile tile = Map.current.TileAt(t);
                    AbilityCommand testMove = (AbilityCommand)targeter.ChooseTarget(tile);

                    testMove.Execute(true);
                    
                    float testScore = boardState.TeamHealth(Unit.current.data.team);
                    testScore -= boardState.TeamHealth(Team.PLAYER);
                    if (testScore > score)
                    {
                        score = testScore;
                        bestSkill = testMove;
                        bestMove = (MoveCommand)move;
                    }
                    
                    testMove.Undo(true);
                }
            }
            
            move.Undo(true);
        }
        
        bestAction.Add(bestMove);
        bestAction.Add(bestSkill);
        return bestAction;
    }
    public IEnumerator Think(BoardState boardState)
    {
        
        yield return new WaitForSeconds(0.5f);
        List<ICommand> movement = FindMoves(boardState);
        List<ICommand> chosen = ChooseAbility(boardState, movement);
        
        foreach (ICommand c in chosen)
        {
            c.Show(true);
            yield return new WaitForSeconds(1);
            CombatManager.IssueCommand(c);
            yield return new WaitForSeconds(1);
            c.Show(false);
        }
        
        yield return new WaitForSeconds(1);
        CombatManager.Next();
        
    }

    public void ShowMoves(BoardState boardState, bool flag)
    {
        List<ICommand> moves = FindMoves(boardState);
        foreach (ICommand move in moves)
        {
            move.Show(flag);
        }
    }
}
