using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    //public delegate void componentFunction(Targeter targeter, AbilityArgs args, BoardState boardState = null, bool undo = false);
    public delegate void componentFunction(HexCoords source, HexCoords target, List<HexCoords> splash,
        AbilityArgs args, BoardState boardState = null, bool undo = false);
    public static Dictionary<AbilityType, componentFunction> abilities;
    public void Awake()
    {
        if (abilities == null)
        {
            abilities = new Dictionary<AbilityType, componentFunction>();
            abilities[AbilityType.DAMAGE] = Damage;
        }
    }

    private static void Damage(HexCoords source, HexCoords target, List<HexCoords> splash,
        AbilityArgs args, BoardState boardState, bool undo)
    {
        
        switch (args.target)
        {
            case AbilityTarget.TARGET:
                {
                    if (Map.current.Occupant(target) != null)
                    {
                        if (boardState != null)
                        {
                            if (undo)
                            {
                                boardState.board[target].unit.Damage(-args.damage);
                            } else
                            {
                                boardState.board[target].unit.Damage(args.damage);
                            }
                        } else
                        {
                            Map.current.Occupant(target).GetComponent<Unit>().TakeDamage(args.damage);
                        }
                        
                    }
                    
                    break;
                }
            case AbilityTarget.SELF:
                {
                    if (boardState != null)
                    {
                        if (undo)
                        {
                            boardState.board[source].unit.Damage(-args.damage);
                        } else
                        {
                            boardState.board[source].unit.Damage(args.damage);
                        }
                    } else
                    {
                        Map.current.Occupant(source).GetComponent<Unit>().TakeDamage(args.damage);
                    }
                    
                    break;
                }
            case AbilityTarget.SPLASH:
                {
                    foreach (HexCoords tile in splash)
                    {
                        if (boardState != null && boardState.board[tile].unit != null)
                        {
                            if (undo)
                            {
                                boardState.board[tile].unit.Damage(-args.damage);
                            } else
                            {
                                boardState.board[tile].unit.Damage(args.damage);
                            }
                        } else if (boardState == null)
                        {
                            if (Map.current.Occupant(tile) != null)
                            {
                                Map.current.Occupant(tile).GetComponent<Unit>().TakeDamage(args.damage);
                            }
                        }
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }
    }
}
