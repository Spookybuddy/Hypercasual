using UnityEngine;
using UnityEngine.UI;

public class Backgrounds : MonoBehaviour
{
    public Material[] materials;
    //public Renderer render;
    public Image render;

    public void Mat(int index) { render.material = materials[index]; }
}