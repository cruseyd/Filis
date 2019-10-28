using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDelay : MonoBehaviour
{

    [SerializeField] private Renderer rend;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(ChangeColor());
        }
    }

    private IEnumerator ChangeColor()
    {
        yield return new WaitForSeconds(1);
        rend.material.color = Color.red;
        
    }
}
