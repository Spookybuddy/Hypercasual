using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    //menus
    public TextMeshProUGUI score, total, recordE, walletE, gamedown, loginBonus, ratio, baseWallet, topWallet, baseBest, topBest;
    public GameObject rMenu, pMenu, mMenu, gMenu, gOver, sMenu, sBase, sLine, sBack, sFruit, confirm, refuse, trophy, treasure, arise, recieve, title, top, bot, optMenu;
    public GameObject[] shopScroll;
    public AudioSource MusicM, MusicG, SFX;
    public AudioClip loseTwang, buyFail, buySucc, menuBoop, menuBack;
    private Renderer MR;
    public Renderer TR, BR;
    private int maxObjects, currentMenu, past;
    private GameObject chest;
    public GameObject[] prefab1, prefab2;
    private GameObject[,] prefabs;
    private List<Vector3> warnings = new List<Vector3>();
    private List<GameObject> actives = new List<GameObject>();
    public bool canDraw, canDoodle;
    private bool mained, paused, shopping, lines, backs, fruit, confirming, rejecting, tutoring, rewarding, triggered;
    private Vector2 price;
    public Backgrounds BG;
    public Fruits FT;
    public Line LN;
    public int[] DESIGNS;
    private float difficultyTime, scrollDis;
    public int countdown, points;

    //Save data
    private int month, day, best;
    public int chain, currency, mode, food, ground;
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

    void Awake()
    {
        //Screen resolution
        ratio.text = Screen.width.ToString() + " : " + Screen.height.ToString();
        scrollDis = Mathf.Clamp(Screen.height / 10, 120, 300);
    }

    void Start()
    {
        prefabs = new GameObject[2, 2] { { prefab1[0], prefab1[1] }, { prefab2[0], prefab2[1] } };

        //Get any saved data
        best = GetSaveData("Record");
        currency = GetSaveData("Money");
        mode = GetSaveData("Design");
        food = GetSaveData("Defend");
        ground = GetSaveData("Picture");
        day = GetSaveData("LastLogin");
        day = (day == 0 ? System.DateTime.Now.Day - 1 : day);
        chain = GetSaveData("LoginChain");
        month = GetSaveData("LastMonth");
        month = (month == 0 ? System.DateTime.Now.Month : month);

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
        //tutoring = true;
        maxObjects = 0;
        MR = trophy.GetComponent<Renderer>();
        trophy.SetActive(false);
        walletE.gameObject.SetActive(false);

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

        MusicG.Stop();
        MusicM.Play();

        //Daily rewards
        if (System.DateTime.Now.Day != day) {
            //If the last login was on the last day of the month and the current day is the 1st, give daily login
            bool first = (day == System.DateTime.DaysInMonth(System.DateTime.Now.Year, month) && System.DateTime.Now.Day == 1);

            //Reset chain if missed consecutive day, otherwise increase chain
            if (!first && !(System.DateTime.Now.Day == day + 1)) chain = 1;
            else chain++;

            //Record date as last login, trigger reward animation
            day = System.DateTime.Now.Day;
            month = System.DateTime.Now.Month;
            Rewards();
            rewarding = true;
            arise.transform.localPosition = new Vector3(0, -500, -2000);
            arise.SetActive(false);
            recieve.SetActive(false);
        }
    }

    void Update()
    {
        //Wait until the title screen is exited
        if (title == null && !triggered) {
            //Daily reward
            if (rewarding) {
                chest = Instantiate(treasure, new Vector3(0, 8, -3), Quaternion.identity) as GameObject;
                StartCoroutine(Halt(5.5f, arise));
                StartCoroutine(Halt(7, recieve));
            }

            triggered = true;
            ShowMenus();
        }

        if (!rewarding && title == null) ShowMenus();

        //Bools based on what menus are active
        canDraw = !(paused || mained || shopping);
        canDoodle = (mained || shopping);
        maxObjects = canDraw ? past : 0;

        //Add timer, max objects based on time passed
        if (canDraw && countdown == 0) difficultyTime += Time.deltaTime;
        past = Mathf.FloorToInt(difficultyTime / 20) + 1;

        //Update texts
        score.text = "Score: " + points.ToString();
        total.text = "Final Score:\n" + points.ToString();
        recordE.text = "Record: " + best.ToString();
        walletE.text = "$" + currency.ToString();
        if (countdown != 0) gamedown.text = countdown.ToString();
        else gamedown.text = " ";

        //Fruit knocked away: lose
        if (!MR.isVisible && !mained && !rewarding && !shopping) Over();

        //Spawn new bomb when #bomb is below desired amount
        if (actives.Count < past && canDraw && countdown == 0) {
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

        //Music
        if (!MusicG.isPlaying && canDraw) {
            MusicM.Stop();
            MusicG.Play();
        }
        if (!MusicM.isPlaying && canDoodle) {
            MusicG.Stop();
            MusicM.Play();
        }
        if (MusicG.isPlaying && !MR.isVisible) {
            MusicG.Stop();
            Play(loseTwang, 0.3f);
        }

        //Raise text
        Transform cashForm = arise.transform;
        if (arise.activeInHierarchy && cashForm.localPosition.y < 0.1f) {
            cashForm.localPosition = new Vector3(0, Mathf.MoveTowards(cashForm.localPosition.y, 0, Time.deltaTime * -cashForm.localPosition.y * 1.5f), -2000);
        }
    }

    //--------------------------------------------------------------------------------------------- GAME FUNCTIONS
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

    //Remove all bombs
    private void Erase()
    {
        GameObject[] clear = GameObject.FindGameObjectsWithTag("Respawn");
        foreach (GameObject obj in clear) Destroy(obj);
    }

    //Pause the game if the app is suspended midgame
    private void OnApplicationPause(bool status)
    {
        if (canDraw && status) Pause(true);
    }

    //Play sound effect
    public void Play(AudioClip clip, float volume)
    {
        SFX.PlayOneShot(clip, volume);
    }

    //Exit exe (Non mobile only)
    public void Quit()
    {
        Application.Quit();
    }

    //Chain rewards
    private void Rewards()
    {
        currency += chain * 5;
        loginBonus.text = "$" + (chain * 5).ToString();
        SaveData("LastLogin", day);
        SaveData("LastMonth", month);
        SaveData("LoginChain", chain);
        SaveData("Money", currency);
    }

    //Move shop menu with swipe: 1/10th screen height
    public void Scroll(float move)
    {
        Vector3 pos = shopScroll[currentMenu].transform.localPosition;
        shopScroll[currentMenu].transform.localPosition = new Vector3(pos.x, Mathf.Clamp(pos.y + (scrollDis * move), 0, 300 * (DESIGNS[currentMenu] - 6)), pos.z);
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

    //Show desired menu
    private void ShowMenus()
    {
        //Check if the top & bottom areas are visible, and if they are move some menu assets to them
        top.SetActive(TR.isVisible && !canDraw);
        bot.SetActive(BR.isVisible && !canDraw);
        if (TR.isVisible) { }

        trophy.SetActive(!rewarding && !(lines || fruit || backs));
        rMenu.SetActive(rewarding);
        mMenu.SetActive(mained && !rewarding);
        pMenu.SetActive(paused);
        gMenu.SetActive(canDraw);
        sMenu.SetActive(shopping);
        sBase.SetActive(!(lines || fruit || backs));
        sLine.SetActive(lines);
        sFruit.SetActive(fruit);
        sBack.SetActive(backs);
        confirm.SetActive(confirming);
        refuse.SetActive(rejecting);
        gOver.SetActive((!MR.isVisible && !mained && !shopping));
        walletE.gameObject.SetActive(!rewarding);
    }

    //--------------------------------------------------------------------------------------------- BUTTON FUNCTIONS
    //Accept the daily rewards
    public void Accept()
    {
        walletE.gameObject.SetActive(true);
        Destroy(chest);
        mained = true;
        rewarding = false;
    }

    //Gameplay
    public void Game()
    {
        if (!rewarding && !shopping && mained) {
            countdown = 3;
            StartCoroutine(Delay());
            paused = false;
            mained = false;
        }
    }

    //Function takes NXXX, with N being design number & XXX being cost
    public void Item(int NumCost)
    {
        if (!confirming) {
            int num = NumCost / 1000;
            int cost = NumCost - (num * 1000);
            if (!unlocks[currentMenu].Contains((int)num)) {
                if (currency >= (int)cost) {
                    price = new Vector2(num, cost);
                    confirming = true;
                } else {
                    rejecting = true;
                    Play(buyFail, 0.6f);
                    StartCoroutine(Wait());
                }
            } else {
                Change((int)num);
            }
        }
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
        Erase();
        foreach(GameObject menu in shopScroll) menu.transform.localPosition = Vector3.zero;
        shopping = false;

        //Save data for: Record, $, Current Line, Fruit, BG, all purchased Lines, Fruit & BGs
        RecordSaveData();
    }

    //Gameover
    public void Over()
    {
        canDraw = false;
        paused = false;
        mained = false;
        Erase();
    }

    //Open pause menu, halt gameplay
    public void Pause(bool onOff)
    {
        paused = onOff;
        canDraw = !onOff;
        if (!onOff) {
            countdown = 1;
            StartCoroutine(Delay());
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
            RecordSaveData();
            Play(buySucc, 0.4f);
        }
        confirming = false;
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

    //--------------------------------------------------------------------------------------------- SAVE DATA
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

    //Record all save data
    public void RecordSaveData()
    {
        SaveData("Record", best);
        SaveData("Money", currency);
        SaveData("Design", mode);
        SaveData("Defend", food);
        SaveData("Picture", ground);
        SaveData("LastLogin", day);
        SaveData("LoginChain", chain);
        PlayerPrefs.SetString("Inventory", draws);
        PlayerPrefs.SetString("Fruits", basket);
        PlayerPrefs.SetString("Background", area);
    }

    //Write input data to input Key
    public void SaveData(string Key, int Data)
    {
        PlayerPrefs.SetInt(Key, Data);
    }

    //--------------------------------------------------------------------------------------------- IENUMERATORS LAST
    //Game has a countdown before starting
    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(1);
        countdown--;
        if (countdown > 0) StartCoroutine(Delay());
    }

    //Daily rewards delayed UI appearance
    private IEnumerator Halt(float time, GameObject menu)
    {
        yield return new WaitForSeconds(time);
        menu.SetActive(true);
    }

    //"Not enough cash" message lasts for 1.25sec
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.25f);
        rejecting = false;
    }
}