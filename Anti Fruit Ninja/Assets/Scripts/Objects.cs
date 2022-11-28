using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objects : MonoBehaviour
{
    public GameManager script;
    public MeshRenderer MR;
    public Rigidbody rig;

    public Vector3 spawn;
    private bool paused;
    private Vector3 storedVelo;
    private bool hasHit;

    public bool goodBad;
    public Vector3 rotateAxis;

    void Start()
    {
        script = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        spawn = transform.position;
        hasHit = false;
        rig.useGravity = false;
        StartCoroutine(waitFor());
    }

    void Update()
    {
        if (!paused) {
            //Enter pause, record velo
            if (!script.canDraw) {
                rig.useGravity = false;
                storedVelo = rig.velocity;
                rig.velocity = Vector3.zero;
                paused = true;
            }
            //Exit pause, set velo to recorded
        } else if (script.canDraw) {
            rig.useGravity = true;
            rig.velocity = storedVelo;
            paused = false;
        }

        //Out of sight and away from spawn location
        if (!MR.isVisible && Vector3.Distance(spawn, transform.position) > 10 && !paused) Destroy(gameObject);
    }

    private IEnumerator waitFor()
    {
        yield return new WaitForSeconds(0.75f);
        rig.useGravity = true;
        rig.AddForce(new Vector3(-1.25f * spawn.x, -1.33333f * spawn.y + 3, 0), ForceMode.Impulse);
    }

    void OnTriggerEnter(Collider collision)
    {
        if (goodBad) {
            if (collision.CompareTag("Finish")) {
                script.currency += 10;
                Destroy(gameObject);
            }
        } else {
            if (!hasHit && !goodBad) {
                script.points++;
                hasHit = true;
            }
        }
    }
}