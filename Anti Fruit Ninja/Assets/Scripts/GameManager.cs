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
    public GameObject shopBase;
    public GameObject shopLine;
    public GameObject shopBack;
    public GameObject shopFruit;
    public GameObject[] shopScroll;
    public GameObject confirm;
    public GameObject refusal;
    public GameObject trophy;
    private Renderer MR;

    private int maxObjects;
    public GameObject[] prefab1;
    public GameObject[] prefab2;
    public GameObject[,] prefabs;
    private List<Vector3> warnings = new List<Vector3>();
    private List<GameObject> actives = new List<GameObject>();
    public bool canDraw;
    public bool canDoodle;
    private bool mained;
    private bool paused;
    private bool shopping;
    private bool lines;
    private bool backs;
    private bool fruit;
    private bool confirming;
    private bool rejecting;
    private Vector2 price;

    public Backgrounds BG;
    public Fruits FT;
    public Line LN;
    private int currentMenu;
    public int[] DESIGNS;

    private float difficultyTime;
    private int past;
    public int points;

    //Save data
    private int best;
    public int currency;
    private List<int> unlocked = new List<int>();
    private string draws;
    private List<int> unlocked2 = new List<int>();
    private string basket;
    private List<int> unlocked3 = new List<int>();
    private string area;

    private List<List<int>> unlocks = new List<List<int>>();
    private string[] strings = new string[3];

    private string list;
    private string[] split;
    private int[] unlock;

    public int mode;
    public int food;
    public int ground;

    void Start()
    {
        prefabs = new GameObject[2, 2] { { prefab1[0], prefab1[1] }, { prefab2[0], prefab2[1] } };

        //Get any saved data
        best = GetSaveData("Record");
        currency = GetSaveData("Money");
        mode = GetSaveData("Design");
        food = GetSaveData("Defend");
        ground = GetSaveData("Picture");
        draws = GetStringData("Inventory");
        basket = GetStringData("Fruits");
        area = GetStringData("Background");

        unlocks.Insert(0, unlocked);
        unlocks.Insert(1, unlocked2);
        unlocks.Insert(2, unlocked3);
        Set();

        //Start on main
        mained = true;
        paused = false;
        canDraw = false;
        shopping = false;
        maxObjects = 0;
        MR = trophy.GetComponent<Renderer>();

        //Update shop text
        list = draws;
        ConvertData(unlocked);
        list = basket;
        ConvertData(unlocked2);
        list = area;
        ConvertData(unlocked3);
        for (int i = 0; i < DESIGNS.Length; i++) {
            currentMenu = i;
            for (int j = 0; j < DESIGNS[i]; j++) if (unlocks[i].Contains(j)) SetText(j);
        }
        currentMenu = 0;
    }

    void Update()
    {
        //Show desired menu
        mainMenu.SetActive(mained);
        pauseMenu.SetActive(paused);
        gameMenu.SetActive(canDraw);
        shopMenu.SetActive(shopping);
        shopBase.SetActive(!(lines || fruit || backs));
        shopLine.SetActive(lines);
        shopFruit.SetActive(fruit);
        shopBack.SetActive(backs);
        confirm.SetActive(confirming);
        refusal.SetActive(rejecting);
        gameOver.SetActive((!MR.isVisible && !mained));

        //Bools based on what menus are active
        canDraw = !(paused || mained || shopping);
        canDoodle = (mained || shopping);
        maxObjects = canDraw ? past : 0;

        //Add timer, max objects based on time passed
        if (canDraw) difficultyTime += Time.deltaTime;
        past = Mathf.FloorToInt(difficultyTime / 20) + 1;

        //Update texts
        score.text = "Score: " + points.ToString();
        total.text = "Final Score:\n" + points.ToString();
        record.text = "Record: " + best.ToString();
        wallet.text = "$" + currency.ToString();

        //Orange knocked away: lose
        if (!MR.isVisible && !mained) Over();

        //Spawn new bomb when #bomb is below desired amount
        if (actives.Count < past && canDraw) {
            int weighted = Mathf.FloorToInt(Random.Range(0, 7)/6);
            warnings.Insert(0, new Vector3(2.5f * Mathf.Sign(Random.Range(-1, 1)), Random.Range(-5, 5), 0));
            Vector3 outside = warnings[0] + new Vector3(warnings[0].x, 0, 0);
            GameObject baller = Instantiate(prefabs[weighted, 0], outside, Quaternion.identity) as GameObject;
            actives.Insert(0, baller);
            Instantiate(prefabs[weighted, 1], warnings[0], Quaternion.identity);
        }

        //Once bomb is gone, remove from lists to spawn new one
        for (int i = 0; i < actives.Count; i++) {
            if (actives[i] == null) {
                warnings.RemoveAt(i);
                actives.RemoveAt(i);
            }
        }
    }

    //Open pause menu, halt gameplay
    public void Pause(bool onOff)
    {
        paused = onOff;
        canDraw = !onOff;
    }

    //Main menu T/F, reset timer, orange, add points, clear bombs
    public void Main(bool onOff)
    {
        mained = onOff;
        paused = !onOff;
        difficultyTime = 0;
        trophy.transform.position = Vector3.zero;
        trophy.transform.eulerAngles = Vector3.zero;
        Rigidbody component = trophy.GetComponent<Rigidbody>();
        component.velocity = Vector3.zero;
        component.angularVelocity = Vector3.zero;

        //Record highest, double reward $
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
        foreach(GameObject menu in shopScroll) menu.transform.localPosition = Vector3.zero;
        shopping = false;

        //Save data for: Record, $, Current Line, Fruit, BG, all purchased Lines, Fruit & BGs
        SaveData("Record", best);
        SaveData("Money", currency);
        SaveData("Design", mode);
        SaveData("Defend", food);
        SaveData("Picture", ground);
        PlayerPrefs.SetString("Inventory", draws);
        PlayerPrefs.SetString("Fruits", basket);
        PlayerPrefs.SetString("Background", area);
    }

    //Gameplay
    public void Game()
    {
        canDraw = true;
        paused = false;
        mained = false;
    }

    //Gameover
    public void Over()
    {
        canDraw = false;
        paused = false;
        mained = false;
    }

    //Function takes NXXX, with N being design number & XXX being cost
    public void Item(int NumCost)
    {
        if (!confirming) {
            int num = NumCost/1000;
            int cost = NumCost - (num * 1000);
            if (!unlocks[currentMenu].Contains((int)num)) {
                if (currency >= (int)cost) {
                    price = new Vector2(num, cost);
                    confirming = true;
                } else {
                    rejecting = true;
                    StartCoroutine(Wait());
                }
            } else {
                Change((int)num);
            }
        }
    }

    //Confirm/deny purchase
    public void Purchase(bool buy)
    {
        if (buy) {
            //Buy: Set current design, then add new purchase to save data and list
            Change((int)price.x);
            currency -= (int)price.y;
            list = strings[currentMenu];
            ConvertData(unlocks[currentMenu]);
            SetText((int)price.x);
        }
        confirming = false;
    }

    //Change desired value 
    private void Change(int val)
    {
        switch (currentMenu) {
            case 0:
                mode = val;
                draws += val.ToString() + "_";
                break;
            case 1:
                food = val;
                basket += val.ToString() + "_";
                break;
            case 2:
                ground = val;
                area += val.ToString() + "_";
                break;
        }
        Set();
    }

    //Sets all things to their values
    private void Set()
    {
        BG.Mat(ground);
        FT.Swap(food);
        LN.Design(mode);
        strings[0] = draws;
        strings[1] = basket;
        strings[2] = area;
    }

    //Remove the price of designs already purchased
    public void SetText(int ID)
    {
        shopScroll[currentMenu].transform.GetChild(ID).GetChild(1).GetComponent<TextMeshProUGUI>().text = " ";
    }

    //Reset shop position and open shop menu
    public void Shop()
    {
        foreach(GameObject menu in shopScroll) menu.transform.localPosition = Vector3.zero;
        mained = false;
        shopping = true;
    } 

    //Set submenu to true/false based on input value
    public void SubMenu(int menu)
    {
        currentMenu = Mathf.Max(menu - 1, 0);
        Shop();
        lines = (menu == 1);
        fruit = (menu == 2);
        backs = (menu == 3);
    }

    //Move shop menu with swipe
    public void Scroll(float move)
    {
        Vector3 pos = shopScroll[currentMenu].transform.localPosition;
        shopScroll[currentMenu].transform.localPosition = new Vector3(pos.x, Mathf.Clamp(pos.y + (150 * move), 0, 300 * (DESIGNS[currentMenu] - 6)), pos.z);
    }

    //Take string, split it, convert to int, then set unlocked list
    private void ConvertData(List<int> array)
    {
        split = list.Split('_');
        unlock = new int[split.Length - 1];
        for (int i = 0; i < split.Length - 1; i++) {
            if (int.TryParse(split[i], out int res)) unlock[i] = res;
        }
        array.Clear();
        for (int i = 0; i < unlock.Length; i++) array.Insert(i, unlock[i]);
    }

    //Exit exe (Non mobile only)
    public void Quit()
    {
        Application.Quit();
        DeleteSaveData();
    }

    //Clear all saved data
    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteAll();
    }

    //Return any saved int data, 0 otherwise
    private int GetSaveData(string Key)
    {
        if (PlayerPrefs.HasKey(Key)) {
            return PlayerPrefs.GetInt(Key);
        } else {
            PlayerPrefs.SetInt(Key, 0);
            return 0;
        }
    }

    //Return any saved string data, 0 otherwise
    private string GetStringData(string Key)
    {
        if (PlayerPrefs.HasKey(Key)) {
            return PlayerPrefs.GetString(Key);
        } else {
            PlayerPrefs.SetString(Key, "0_");
            return "0_";
        }
    }

    //Write input data to input Key
    public void SaveData(string Key, int Data)
    {
        PlayerPrefs.SetInt(Key, Data);
    }

    //"Not enough cash" message lasts for 1.5sec
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.5f);
        rejecting = false;
    }
}