using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.Camera))]
public class CameraFollow : MonoBehaviour
{
    public AnimationCurve panCurve;
    public float panSpeed = 3.0f;
    public float zoomSpeed = 5.0f;
    public float maxSize;
    public float minSize;

    public Vector3 unitPanOffset;

    private void Update()
    {
        handleManualInput();
        handleZoom();
    }

    public void handleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKey(KeyCode.KeypadPlus) || scroll < 0.01)
        {
            float size = GetComponent<Camera>().orthographicSize;
            GetComponent<Camera>().orthographicSize =
                Mathf.Min(maxSize, size + Time.deltaTime * zoomSpeed);
        }
        if (Input.GetKey(KeyCode.KeypadMinus) || scroll > -0.01)
        {
            float size = GetComponent<Camera>().orthographicSize;
            GetComponent<Camera>().orthographicSize =
                Mathf.Max(minSize, size - Time.deltaTime * zoomSpeed);
        }

    }

    public void handleManualInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            this.transform.position = this.transform.position
                + new Vector3(0, panSpeed * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            this.transform.position = this.transform.position
                + new Vector3(-panSpeed * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            this.transform.position = this.transform.position
                + new Vector3(0, -panSpeed * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            this.transform.position = this.transform.position
                + new Vector3(panSpeed * Time.deltaTime, 0, 0);
        }
    }

    public void panToUnit(Transform target)
    {
        StopCoroutine("animatePan");
        StartCoroutine(animatePan(target));
    }

    IEnumerator animatePan(Transform target)
    {
        Vector3 start = this.transform.position;
        Vector3 end = new Vector3(target.position.x, target.position.y, start.z)
            + unitPanOffset;
        float t = 0.0f;
        while (t < 1.0)
        {
            t += panSpeed * Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, panCurve.Evaluate(t));
            yield return null;
        }
    }

}
