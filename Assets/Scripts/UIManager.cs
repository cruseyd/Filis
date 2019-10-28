using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ButtonName
{
    MOVE,
    NEXT,
    CONFIRM,
}

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager instance { get { return _instance; } }

    [SerializeField]
    private int _numAbilities = 4;
    public static int numAbilities { get { return _instance._numAbilities; } }

    [SerializeField] private GameObject _moveButton;
    [SerializeField] private GameObject _endButton;
    [SerializeField] private GameObject _confirmButton;
    [SerializeField] private GameObject[] _abilityButtons;
    [SerializeField] private GameObject _unitCursor;

    [SerializeField] public Vector3 _unitCursorOffset;
    [SerializeField] public GameObject _hexCursor;
    public static GameObject hexCursor { get { return instance._hexCursor; } }

    
    // Unit UI
    [SerializeField] private GameObject _unitDetails;
    [SerializeField] private Image _unitDetailsSprite;
    [SerializeField] private TextMeshProUGUI _unitDetailsName;
    [SerializeField] private HealthBar _unitHealthBar;

    // Target UI
    [SerializeField] private GameObject _targetDetails;
    [SerializeField] private Image _targetDetailsSprite;
    [SerializeField] private TextMeshProUGUI _targetDetailsName;
    [SerializeField] private HealthBar _targetHealthBar;

    [SerializeField] private GameObject _tooltipWindow;
    public static GameObject tooltipWindow { get { return _instance._tooltipWindow; } }
    [SerializeField] private TextMeshProUGUI _tooltipHeader;
    public static TextMeshProUGUI tooltipHeader { get { return _instance._tooltipHeader; } }
    [SerializeField] private TextMeshProUGUI _tooltipDetails;
    public static TextMeshProUGUI tooltipDetails { get { return _instance._tooltipDetails; } }


    private void Awake()
    {
        if (instance != null) { Destroy(this); }
        else { _instance = this; }
    }

    public static Button button(ButtonName name)
    {
        switch (name)
        {
            case ButtonName.MOVE:
                {
                    return instance._moveButton.GetComponent<Button>();
                }
            case ButtonName.NEXT:
                {
                    return instance._endButton.GetComponent<Button>();
                }
            case ButtonName.CONFIRM:
                {
                    return instance._confirmButton.GetComponent<Button>();
                }
        }
        return null;
    }
    public static Button ability(int index)
    {
        return instance._abilityButtons[index].GetComponent<Button>();
    }

    public static void setUnitCursor(Unit unit)
    {
        if (unit == null)
        {
            instance._unitCursor.SetActive(false);
        }
        instance._unitCursor.transform.position =
            unit.transform.position + instance._unitCursorOffset;
        instance._unitCursor.transform.SetParent(unit.transform);
        instance._unitCursor.SetActive(true);
    }

    public static void setUnitUI(Unit unit, bool animate = false)
    {
        setUnitCursor(unit);
        UIManager.button(ButtonName.MOVE).interactable = true;
        UIManager.button(ButtonName.NEXT).interactable = true;

        for (int ii = 0; ii < UIManager.numAbilities; ii++)
        {
            if (unit.data.abilities[ii] == null)
            {
                UIManager.ability(ii).interactable = false;
            } else
            {
                UIManager.ability(ii).GetComponent<Tooltip>().data = unit.data.abilities[ii].tooltipData;
            }
        }
        instance._unitDetailsSprite.sprite = unit.data.species.icon;
        instance._unitDetailsName.text = unit.data.species.name;

        unit.GetComponent<Health>().setDisplay(instance._unitHealthBar);
        if (animate)
        {
            Vector3 end = instance._unitDetails.transform.localPosition;
            Vector3 start = new Vector3(-1000, end.y, end.z);
            _instance.StartCoroutine(slide(instance._unitDetails.transform, start, end, 0.1f));
        }

    }

    public static void setTargetUI(HexTile tile, bool animate = false)
    {
        if (tile == null || tile.occupant == null)
        {
            instance._targetDetails.SetActive(false);
        } else
        {
            instance._targetDetails.SetActive(true);
            Unit unit = tile.occupant.GetComponent<Unit>();
            instance._targetDetailsSprite.sprite = unit.data.species.icon;
            instance._targetDetailsName.text = unit.data.species.name;

            unit.GetComponent<Health>().setDisplay(instance._targetHealthBar);
            if (animate)
            {
                Vector3 end = instance._targetDetails.transform.localPosition;
                Vector3 start = new Vector3(1000, end.y, end.z);
                _instance.StartCoroutine(slide(instance._targetDetails.transform, start, end, 0.1f));
            }
            
        }
    }

    public static void setHexCursor(HexTile tile, bool animate = false)
    {
        hexCursor.SetActive(true);
        hexCursor.GetComponentInChildren<Renderer>().sortingOrder = -tile.coords.a;
        if (animate)
        {
            
            _instance.StartCoroutine(slide(hexCursor.transform,
                hexCursor.transform.position,
                tile.transform.position, 0.1f));
        }
        else
        {
            hexCursor.transform.position = tile.transform.position;
        }
        
    }

    public static IEnumerator slide(Transform tf, Vector3 start, Vector3 end, float speed)
    {
        float elapsed = 0.0f;
        Renderer rend = tf.GetComponentInChildren<Renderer>();
        int order = 0;
        if (rend != null) { order = rend.sortingOrder; }
        while (elapsed < speed)
        {
            if (rend != null) { rend.sortingOrder = 100; }
            elapsed += Time.deltaTime;
            tf.localPosition = Vector3.Lerp(start, end, elapsed / speed);
            yield return null;
        }
        if (rend != null) { rend.sortingOrder = order; }
        
    }

}
