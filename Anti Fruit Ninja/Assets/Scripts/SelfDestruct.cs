using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float timer;

    void Start() { StartCoroutine(Implode()); }

    IEnumerator Implode()
    {
        yield return new WaitForSeconds(timer);
        Destroy(gameObject);
    }
}