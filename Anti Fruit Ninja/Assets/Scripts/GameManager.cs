using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject background;
    public GameObject pauseMenu;
    public GameObject mainMenu;
    public GameObject gameMenu;
    public char orientation;

    public int maxObjects;
    public bool canDraw;
    public bool mained;
    public bool paused;

    public float difficultyTime;
    public int tenths;

    void Start()
    {
        mained = true;
        paused = false;
        canDraw = false;
        maxObjects = 0;
    }

    void Update()
    {
        mainMenu.SetActive(mained);
        pauseMenu.SetActive(paused);
        gameMenu.SetActive(canDraw);
        canDraw = !(paused || mained);
        maxObjects = canDraw ? tenths : 0;

        if (canDraw) difficultyTime += Time.deltaTime;
        tenths = Mathf.FloorToInt(difficultyTime / 10) + 1;

        //Debug orientation testing
        orientation = char.ToUpper(orientation);
        switch (orientation) {
            case 'L':
                //Landscape Left
                break;
            case 'R':
                //Landscape Right
                break;
            case 'U':
                //Portrait Upside Down
                break;
            default:
                //Portrait, Unknown, FaceUp, FaceDown
                break;
        }

        //Detect device orientation and adjust background accordingly
        if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft) {
            //Landscape Left
        } else if (Input.deviceOrientation == DeviceOrientation.LandscapeRight) {
            //Landscape Right
        } else if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown) {
            //Portrait Upside Down
        } else {
            //Portrait, Unknown, FaceUp, FaceDown
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
    }

    public void Game()
    {
        canDraw = true;
        paused = false;
        mained = false;
    }
}