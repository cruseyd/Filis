using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderHeight : MonoBehaviour
{

    [Range(-1,1)]
    [SerializeField] private float depthOffset = 0.0f;
    [SerializeField] private float heightOffset = 0.0f;
    [SerializeField] private Transform anchor;

    void Start()
    {
        RenderAt(transform);
    }
    void Update()
    {
        RenderAt(anchor);
    }
    public void SetHeight(float h)
    {
        heightOffset = h;
        RenderAt(anchor);
    }
    public void RenderAt(Transform tileLocation)
    {
        float z = tileLocation.position.y + depthOffset * 0.25f;
        transform.position = new Vector3(
            tileLocation.position.x,
            tileLocation.position.y + heightOffset,
            z
            );
    }

    private void OnDrawGizmos()
    {
        Vector3 floorLine = new Vector3(transform.position.x, transform.position.y - heightOffset + depthOffset * 0.25f, transform.position.z);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(floorLine + Vector3.left * 0.5f,
            floorLine + Vector3.right * 0.5f);
    }
}
