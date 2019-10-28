using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[System.Serializable]
public class TooltipData
{
    [SerializeField]
    private string _type;
    public string type { get { return _type; } }

    [SerializeField]
    private string _name;
    public string name { get { return _name; } }

    [SerializeField]
    private string _description;
    public string description { get { return _description; } }

    [SerializeField]
    private List<string> _properties;
    public List<string> properties { get { return _properties; } }

    [SerializeField]
    private List<string> _values;
    public List<string> values { get { return _values; } }

}

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TooltipData data;
    public static float hoverDelay = 0.5f;
    [SerializeField]
    private float hoverStart = 0.0f;
    [SerializeField]
    private bool pointerOver = false;
    [SerializeField]
    private bool populated = false;

    public void Awake()
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerOver = true;
        hoverStart = Time.time;

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerOver = false;
        UIManager.tooltipWindow.SetActive(false);
        populated = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (data != null)
        {
            if (pointerOver && (Time.time - hoverStart) > hoverDelay)
            {
                UIManager.tooltipWindow.transform.position = Input.mousePosition;
                UIManager.tooltipWindow.SetActive(true);
                populate();
            }
            else
            {
            }
        }
    }

    void populate()
    {
        if (!populated)
        {
            TextMeshProUGUI headerText = UIManager.tooltipHeader;
            TextMeshProUGUI detailsText = UIManager.tooltipDetails;

            headerText.text = data.type + ": " + data.name;
            detailsText.text = data.description + "\n\n";
            Debug.Assert(data.properties.Count == data.values.Count);
            for (int ii = 0; ii < data.properties.Count; ii++)
            {
                detailsText.text += data.properties[ii] + ": " + data.values[ii] + "\n";
            }
            populated = true;
        }
    }
}
