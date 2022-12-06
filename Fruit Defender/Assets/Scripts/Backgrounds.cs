using UnityEngine;

public class Backgrounds : MonoBehaviour
{
    public Material[] materials;
    public Renderer render;

    public void Mat(int index) { render.material = materials[index]; }
}