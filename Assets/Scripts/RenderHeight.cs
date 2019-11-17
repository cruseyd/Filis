using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderHeight : MonoBehaviour
{

    [Range(-1,1)]
    public float depthOffset = 0.0f;

    public float heightOffset = 0.0f;

    [SerializeField] private Transform sprite;
    void Start()
    {
        RenderAt(transform);
    }

    public void RenderAt(Transform tileLocation)
    {
        float z = tileLocation.position.y + depthOffset * 0.25f;
        sprite.position = new Vector3(
            tileLocation.position.x,
            tileLocation.position.y + heightOffset,
            z
            );
    }

    private void OnDrawGizmos()
    {
        Vector3 floorLine = new Vector3(sprite.position.x, sprite.position.y - heightOffset + depthOffset * 0.25f, sprite.position.z);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(floorLine + Vector3.left * 0.5f,
            floorLine + Vector3.right * 0.5f);
    }
}
