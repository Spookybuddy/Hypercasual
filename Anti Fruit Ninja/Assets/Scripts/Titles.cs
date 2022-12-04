using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Titles : MonoBehaviour
{
    public GameObject previous;

    void Update()
    {
        if (Input.GetMouseButton(0) && previous == null) StartCoroutine(load());
    }

    private IEnumerator load()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Game");
        while (!asyncLoad.isDone) yield return null;
    }
}