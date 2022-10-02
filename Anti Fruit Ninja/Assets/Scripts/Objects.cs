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
    public float ranX;
    public float ranY;
    private bool paused;
    private Vector3 storedVelo;

    void Start()
    {
        script = manager.GetComponent<GameManager>();
    }

    void Update()
    {
        if (!paused) {
            if (!script.canDraw) {
                rig.useGravity = false;
                storedVelo = rig.velocity;
                rig.velocity = Vector3.zero;
                paused = true;
            }
        } else if (script.canDraw) {
            rig.useGravity = true;
            rig.velocity = storedVelo;
            paused = false;
        }

        if (!MR.isVisible && Vector3.Distance(spawn, transform.position) > 10 && !paused) {
            rig.velocity = Vector3.zero;
            storedVelo = Vector3.zero;
            spawn = new Vector3(Random.Range(-ranX, ranX), Random.Range(-ranY, ranY), 0);
            transform.position = spawn;
            rig.AddForce(-1.5f * transform.position, ForceMode.Impulse);
        }
    }
}