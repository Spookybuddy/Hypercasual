using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public int type;
    public float duration, original, drawLine;
    public bool requirement, spawned, empty;
    public GameObject warning, bomb, location, active;

    void FixedUpdate()
    {
        if (!empty){
            //Draw line for 2 seconds to continue
            if (gameObject.activeSelf && Input.GetMouseButton(0) && type == 0 && !requirement) {
                drawLine += Time.deltaTime;
                if (drawLine > 2) requirement = true;
            }

            //Bomb blew up to continue
            if (gameObject.activeSelf && type == 1 && active == null && spawned && !requirement) {
                requirement = true;
            }

            //Countdown because fuck you
            if (gameObject.activeSelf && type < 0 && !requirement) {
                duration -= Time.deltaTime;
                if (duration < 0) requirement = true;
            }

            //Spawn bomb
            if (gameObject.activeSelf && type == 1 && !spawned) {
                duration -= Time.deltaTime;
                if (duration < 0) {
                    Instantiate(warning, location.transform.position / 1.5f, Quaternion.identity);
                    active = Instantiate(bomb, location.transform.position, Quaternion.identity) as GameObject;
                    spawned = true;
                }
            }
        }
    }

    public void Reset()
    {
        duration = original;
        drawLine = 0;
        requirement = false;
        spawned = false;
    }
}