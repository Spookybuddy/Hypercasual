using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruits : MonoBehaviour
{
    public GameObject[] fruitBasket;
    public Renderer active;

    public void Swap(int index) {
        foreach (GameObject obj in fruitBasket) obj.SetActive(false);
        fruitBasket[index].SetActive(true);
        active = fruitBasket[index].GetComponent<Renderer>();
    }
}