using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateObject : MonoBehaviour
{
    public Objects parent;
    private GameManager script;
    private Vector3 rotateAxis;
    public float spd;
    private bool speeeen;

    void Start()
    {
        script = parent.script;
        rotateAxis = parent.rotateAxis;
        StartCoroutine(waitFor());
    }

    void Update()
    {
        //Keep to parent, spin only when active
        transform.localPosition = Vector3.zero;
        if (speeeen && script.canDraw) transform.Rotate(rotateAxis * Time.deltaTime * spd, Space.Self);
    }

    private IEnumerator waitFor()
    {
        yield return new WaitForSeconds(0.75f);
        speeeen = true;
    }
}