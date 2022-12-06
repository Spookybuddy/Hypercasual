using UnityEngine;

public class Titles : MonoBehaviour
{
    public GameObject previous;
    private float wait;

    void Update()
    {
        if (previous == null) wait += Time.deltaTime;
        if (Input.GetMouseButton(0) && previous == null && wait > 0.75f) Destroy(gameObject);
    }
}