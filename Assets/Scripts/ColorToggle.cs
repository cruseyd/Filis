using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorToggle : MonoBehaviour
{
    private Toggle toggle;

    private void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        ColorBlock cb = toggle.colors;
        if (isOn)
        {
            cb.normalColor = Color.gray;
            cb.highlightedColor = Color.gray;
        }
        else
        {
            cb.normalColor = Color.white;
            cb.highlightedColor = Color.white;
        }
        toggle.colors = cb;
    }
}
