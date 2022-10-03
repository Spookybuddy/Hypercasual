using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Implode());
    }

    IEnumerator Implode()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}