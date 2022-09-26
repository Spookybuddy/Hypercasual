using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objects : MonoBehaviour
{
    public MeshRenderer MR;
    public Rigidbody rig;

    public Vector3 spawn;
    public float ranX;
    public float ranY;

    void Update()
    {
        if (!MR.isVisible && Vector3.Distance(spawn, transform.position) > 10) {
            rig.velocity = Vector3.zero;
            spawn = new Vector3(Random.Range(-ranX, ranX), Random.Range(-ranY, ranY), 0);
            transform.position = spawn;
            rig.AddForce(-2 * transform.position, ForceMode.Impulse);
        }
    }
}