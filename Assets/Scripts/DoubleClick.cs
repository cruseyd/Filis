using UnityEngine;

public class DoubleClick : MonoBehaviour
{
    [SerializeField]
    private float delay = 0.5f;
    [SerializeField]
    private int click = 0;
    private float start = 0;

    public delegate void doubleClickHandler(Transform tf);
    public event doubleClickHandler onDoubleClick;

    public void Start()
    {
        onDoubleClick += (tf) =>
        {
            print("double click event");
        };
    }

    public void Update()
    {
        if (click == 1 && (Time.time - start) > delay)
        {
            click = 0;
        }
    }

    public void subscribe(doubleClickHandler f)
    {
        onDoubleClick += f;
    }

    public void unsubscribe(doubleClickHandler f)
    {
        onDoubleClick -= f;
    }

    private void OnMouseUp()
    {
        click++;
        if (click == 1)
        {
            start = Time.time;
        } else if (click == 2 && ((Time.time - start) < delay))
        {
            onDoubleClick(this.transform);
            click = 0;
        } else
        {
            click = 0;
        }
    }
}
