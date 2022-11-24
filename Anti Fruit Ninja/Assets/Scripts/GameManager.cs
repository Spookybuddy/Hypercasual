using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    //menus
    public GameObject pauseMenu;
    public GameObject mainMenu;
    public GameObject gameMenu;
    public TextMeshProUGUI score;
    public TextMeshProUGUI total;
    public TextMeshProUGUI record;
    public TextMeshProUGUI wallet;
    public GameObject gameOver;
    public GameObject shopMenu;
    public GameObject shopScroll;
    public GameObject confirm;
    public GameObject refusal;
    public GameObject trophy;
    private Renderer MR;

    private int maxObjects;
    public GameObject ball;
    public GameObject exclaim;
    private List<Vector3> warnings = new List<Vector3>();
    private List<GameObject> actives = new List<GameObject>();
    public bool canDraw;
    public bool canDoodle;
    private bool mained;
    private bool paused;
    private bool shopping;
    private bool confirming;
    private bool rejecting;
    private Vector2 price;
    public int DESIGNS;

    private float difficultyTime;
    private int past;
    public int points;

    //Save data
    public int best;
    public int currency;
    public List<int> unlocked = new List<int>();
    public int mode;

    void Start()
    {
        best = GetSaveData("Record");
        currency = GetSaveData("Money");
        mode = GetSaveData("Design");

        mode = 0;
        mained = true;
        paused = false;
        canDraw = false;
        shopping = false;
        maxObjects = 0;
        MR = trophy.GetComponent<Renderer>();

        for (int i = 0; i < DESIGNS; i++) {
            if (unlocked.Contains(i)) SetText(i);
        }
    }

    void Update()
    {
        mainMenu.SetActive(mained);
        pauseMenu.SetActive(paused);
        gameMenu.SetActive(canDraw);
        shopMenu.SetActive(shopping);
        confirm.SetActive(confirming);
        refusal.SetActive(rejecting);
        gameOver.SetActive((!MR.isVisible && !mained));
        canDraw = !(paused || mained || shopping);
        canDoodle = (mained || shopping);
        maxObjects = canDraw ? past : 0;

        if (canDraw) difficultyTime += Time.deltaTime;
        past = Mathf.FloorToInt(difficultyTime / 20) + 1;

        score.text = "Score: " + points.ToString();
        total.text = "Final Score:\n" + points.ToString();
        record.text = "Record: " + best.ToString();
        wallet.text = "$" + currency.ToString();

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
        if (points > best) {
            best = points;
            currency += best;
        }
        currency += points;
        points = 0;
        GameObject[] clear = GameObject.FindGameObjectsWithTag("Respawn");
        foreach (GameObject i in clear) {
            Destroy(i);
        }
        shopScroll.transform.localPosition = Vector3.zero;
        shopping = false;

        //Save data
        SaveData("Record", best);
        SaveData("Money", currency);
        SaveData("Design", mode);
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

    public void Item(int NumCost)
    {
        if (!confirming) {
            int num = NumCost/1000;
            int cost = NumCost - (num * 1000);
            Debug.Log("#" + num + " : $" + cost);
            if (!unlocked.Contains((int)NumCost)) {
                if (currency >= (int)cost) {
                    price = new Vector2(num, cost);
                    confirming = true;
                } else {
                    rejecting = true;
                    StartCoroutine(Wait());
                }
            } else {
                mode = (int)NumCost;
            }
        }
    }

    public void Purchase(bool buy)
    {
        if (buy) {
            mode = (int)price.x;
            currency -= (int)price.y;
            unlocked.Add((int)price.x);
            SetText((int)price.x);
            confirming = false;
        } else {
            confirming = buy;
        }
    }

    public void SetText(int ID)
    {
        shopScroll.transform.GetChild(ID).GetChild(1).GetComponent<TextMeshProUGUI>().text = " ";
    }

    public void Shop()
    {
        shopScroll.transform.localPosition = Vector3.zero;
        mained = false;
        shopping = true;
    } 

    public void Scroll(float move)
    {
        Vector3 pos = shopScroll.transform.localPosition;
        shopScroll.transform.localPosition = new Vector3(pos.x, Mathf.Clamp(pos.y + (150 * move), 0, 300 * (DESIGNS - 6)), pos.z);
    }

    public void Quit()
    {
        Application.Quit();
        DeleteSaveData();
    }

    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteAll();
    }

    public int GetSaveData(string Key)
    {
        if (PlayerPrefs.HasKey(Key)) {
            return PlayerPrefs.GetInt(Key);
        } else {
            PlayerPrefs.SetInt(Key, 0);
            return 0;
        }
    }

    public void SaveData(string Key, int Data)
    {
        PlayerPrefs.SetInt(Key, Data);
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.5f);
        rejecting = false;
    }
}