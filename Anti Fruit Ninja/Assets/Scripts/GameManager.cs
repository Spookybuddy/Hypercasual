using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject background;
    public GameObject pauseMenu;
    public GameObject mainMenu;
    public GameObject gameMenu;
    public TextMeshProUGUI score;
    public TextMeshProUGUI total;
    public TextMeshProUGUI record;
    public GameObject gameOver;
    public GameObject trophy;

    private int maxObjects;
    public GameObject ball;
    public GameObject exclaim;
    private List<Vector3> warnings = new List<Vector3>();
    private List<GameObject> actives = new List<GameObject>();
    public bool canDraw;
    public bool canDoodle;
    private bool mained;
    private bool paused;

    private float difficultyTime;
    private int past;
    public int points;
    public int best;

    private Renderer MR;
    public bool rainbowMode;

    void Start()
    {
        rainbowMode = false;
        mained = true;
        paused = false;
        canDraw = false;
        maxObjects = 0;
        best = 0;
        MR = trophy.GetComponent<Renderer>();
    }

    void Update()
    {
        mainMenu.SetActive(mained);
        pauseMenu.SetActive(paused);
        gameMenu.SetActive(canDraw);
        gameOver.SetActive((!MR.isVisible && !mained));
        canDraw = !(paused || mained);
        canDoodle = mained;
        maxObjects = canDraw ? past : 0;

        if (canDraw) difficultyTime += Time.deltaTime;
        past = Mathf.FloorToInt(difficultyTime / 20) + 1;

        score.text = "Score: " + points;
        total.text = "Final Score:\n" + points;
        record.text = "Record: " + best;

        if (!MR.isVisible && !mained) Over();

        if (actives.Count < past && canDraw) {
            warnings.Insert(0, new Vector3(2.5f * Mathf.Sign(Random.Range(-1, 1)), Random.Range(-5, 5), 0));
            Vector3 outside = warnings[0] + new Vector3(warnings[0].x, 0, 0);
            GameObject baller = Instantiate(ball, outside, Quaternion.identity) as GameObject;
            actives.Insert(0, baller);
            Instantiate(exclaim, warnings[0], Quaternion.identity);
        }

        for (int i = 0; i < actives.Count; i++) {
            if (actives[i] == null) {
                warnings.RemoveAt(i);
                actives.RemoveAt(i);
            }
        }
    }

    public void Pause(bool onOff)
    {
        paused = onOff;
        canDraw = !onOff;
    }

    public void Main(bool onOff)
    {
        mained = onOff;
        paused = !onOff;
        difficultyTime = 0;
        trophy.transform.position = Vector3.zero;
        trophy.transform.eulerAngles = Vector3.left * 90;
        Rigidbody component = trophy.GetComponent<Rigidbody>();
        component.velocity = Vector3.zero;
        component.angularVelocity = Vector3.zero;
        if (points > best) best = points;
        points = 0;
        GameObject[] clear = GameObject.FindGameObjectsWithTag("EditorOnly");
        foreach (GameObject i in clear) {
            Destroy(i);
        }
    }

    public void Game()
    {
        canDraw = true;
        paused = false;
        mained = false;
    }

    public void Over()
    {
        canDraw = false;
        paused = false;
        mained = false;
    }

    public void Mode()
    {
        rainbowMode = !rainbowMode;
    }

    public void Quit()
    {
        Application.Quit();
    }
}