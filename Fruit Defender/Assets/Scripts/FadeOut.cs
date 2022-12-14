using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOut : MonoBehaviour
{
    private bool outIn;
    public float countdown;
    private bool speedUp;
    public float timer;
    public Material mat;

    void Update()
    {
        if (Input.GetMouseButton(0) && !speedUp) speedUp = true;
        if (!outIn) {
            countdown += Time.deltaTime * (speedUp ? 3 : 1);
            outIn = (countdown > timer);
        } else {
            countdown -= Time.deltaTime * (speedUp ? 4.5f : 1.5f);
        }
        mat.SetColor("_Color", new Color(1, 1, 1, Mathf.Clamp01(countdown / (timer - 1))));
        if (countdown < -0.75f) Destroy(gameObject);
    }
}