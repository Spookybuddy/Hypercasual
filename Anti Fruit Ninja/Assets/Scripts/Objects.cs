using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objects : MonoBehaviour
{
    public GameObject manager;
    private GameManager script;
    public MeshRenderer MR;
    public Rigidbody rig;

    public Vector3 spawn;
    private bool paused;
    private Vector3 storedVelo;
    private bool hasHit;

    void Start()
    {
        manager = GameObject.FindWithTag("GameController");
        script = manager.GetComponent<GameManager>();
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

        if (!MR.isVisible && Vector3.Distance(spawn, transform.position) > 10 && !paused) {
            Destroy(gameObject);
        }
    }

    private IEnumerator waitFor()
    {
        yield return new WaitForSeconds(0.75f);
        rig.useGravity = true;
        rig.AddForce(new Vector3(-1.25f * spawn.x, -1.33333f * spawn.y + 3, 0), ForceMode.Impulse);
        rig.AddTorque(Vector3.forward * 4 * Mathf.Sign(Random.Range(-1, 1)), ForceMode.Impulse);
    }

    void OnTriggerEnter(Collider collision)
    {
        if (!hasHit && !collision.CompareTag("Finish")) {
            script.points++;
            hasHit = true;
        }
    }
}