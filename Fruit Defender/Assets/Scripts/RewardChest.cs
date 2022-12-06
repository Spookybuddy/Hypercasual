using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardChest : MonoBehaviour
{
    private GameManager script;
    public Animation anime;
    public ParticleSystem particles;
    private bool played;

    void Start() { script = GameObject.FindWithTag("GameController").GetComponent<GameManager>(); }

    private void OnTriggerEnter(Collider hit) { StartCoroutine(Wait()); }

    //Wait for chest to stop bouncing before opening
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.8f);
        if (!anime.isPlaying) {
            anime.Play();
            particles.Play();
            StartCoroutine(Coins(Mathf.Clamp(script.chain * 0.5f + 1.5f, 3, 5)));
        }
    }

    //Spray coins for longer depending on the chain
    private IEnumerator Coins(float time)
    {
        yield return new WaitForSeconds(time);
        particles.Stop();
    }
}