using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objects : MonoBehaviour
{
    public GameManager script;
    public MeshRenderer MR;
    public Rigidbody rig;

    public Vector3 spawn;
    private bool paused, regain;
    private Vector3 storedVelo;

    public bool goodBad;
    public Vector3 rotateAxis;
    public AudioClip alertSound, whistleSound, hitSound, yesSound;
    public Vector3[] X, Y;

    void Start()
    {
        script = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        spawn = transform.position;
        regain = false;
        rig.useGravity = false;
        script.Play(alertSound, 0.5f);
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
        } else if (script.canDraw && script.countdown == 0) {
            rig.useGravity = true;
            rig.velocity = storedVelo;
            if (regain) Force();
            paused = false;
        }

        //Out of sight and away from spawn location
        if (!MR.isVisible && Vector3.Distance(spawn, transform.position) > 4 && !paused) Destroy(gameObject);
    }

    //Add force
    private void Force()
    {
        int type = Random.Range(0, 3);
        int index = Mathf.RoundToInt(Mathf.Clamp(-spawn.y, 0, 6));
        //Debug.Log(index + ": " + type);
        rig.useGravity = true;
        rig.AddForce(new Vector3(X[index][type] * -Mathf.Sign(spawn.x), Y[index][type], 0), ForceMode.Impulse);
        script.Play(whistleSound, 0.3f);
        regain = false;
    }

    private IEnumerator waitFor()
    {
        yield return new WaitForSeconds(0.75f);
        if (!paused) Force();
        else regain = true;
    }

    void OnTriggerEnter(Collider collision)
    {
        if (goodBad && collision.CompareTag("Finish")) {
            Destroy(gameObject);
            script.currency += 10;
            script.Play(yesSound, 0.3f);
        } else if (!collision.CompareTag("Respawn")) {
            script.points++;
            script.Play(hitSound, 0.3f);
        }
        if (!goodBad) Destroy(gameObject);
    }
}