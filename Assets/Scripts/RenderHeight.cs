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
        float y = sprite.position.y - heightOffset + depthOffset * 0.25f;
        sprite.position = new Vector3(
            sprite.position.x,
            sprite.position.y,
            y
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
