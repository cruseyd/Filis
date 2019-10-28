using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public enum NumberDisplayType
{
    NONE,
    PERCENT,
    VALUE
}

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Image image;
    [SerializeField]
    private TextMeshProUGUI text;
    [SerializeField]
    private float updateSpeed = 0.2f;
    [SerializeField]
    private NumberDisplayType displayType;

    public int maxHP;
    public int currentHP;

    public void Awake()
    {
        setHealthPercent(currentHP, maxHP);
    }

    public void setHealthPercent(float value, float maxValue)
    {
        StartCoroutine(animateBar(value, maxValue));
    }

    private IEnumerator animateBar(float value, float maxValue)
    {
        float percent = Mathf.Clamp01(value / maxValue);
        float elapsed = 0.0f;
        float start = image.fillAmount;
        while (elapsed < updateSpeed)
        {
            elapsed += Time.deltaTime;
            float currentPercent = Mathf.Lerp(start, percent, elapsed / updateSpeed);
            image.fillAmount = currentPercent;
            switch (displayType)
            {
                case NumberDisplayType.NONE:
                    {
                        text.text = "";
                        break;
                    }
                case NumberDisplayType.PERCENT:
                    {
                        int pctNum = Mathf.RoundToInt(currentPercent * 100);
                        text.text = pctNum.ToString() + " %";
                        break;
                    }
                case NumberDisplayType.VALUE:
                    {
                        int currentValue = Mathf.RoundToInt(currentPercent * maxValue);
                        text.text = currentValue.ToString() + " / " + maxValue.ToString();
                        break;
                    }
                default:
                    {
                        text.text = "";
                        break;
                    }
            }
            yield return null;
        }
        image.fillAmount = percent;
    }


}
