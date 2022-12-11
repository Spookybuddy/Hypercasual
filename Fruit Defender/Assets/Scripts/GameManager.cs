using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    //menus
    public TextMeshProUGUI score, total, record, wallet, gamedown, loginBonus, timer;
    public Slider[] sound;
    public GameObject rMenu, pMenu, mMenu, gMenu, gOver, sMenu, sBase, sLine, sBack, sFruit, confirm, refuse, trophy, treasure, arise, recieve, title, optMenu, optMen2, tMenu, checklist, money, scored;
    public GameObject[] shopScroll, spawns;
    public AudioSource MusicM, MusicG, SFX;
    public AudioClip loseTwang, buyFail, buySucc, menuBoop, menuBack;
    private Renderer MR;
    private int maxObjects, currentMenu, past, sub;
    private GameObject chest;
    public GameObject[] prefab1, prefab2;
    private GameObject[,] prefabs;
    private List<Vector3> warnings = new List<Vector3>();
    private List<GameObject> actives = new List<GameObject>();
    public bool canDraw, canDoodle;
    private bool mained, paused, shopping, lines, backs, fruit, confirming, rejecting, tutoring, rewarding, triggered, options, op2, read;
    private Vector2 price;
    private Vector3 local;
    public Backgrounds BG;
    public Fruits FT;
    public Line LN;
    public Tutorial TT;
    public int[] DESIGNS;
    private float difficultyTime, scrollDis, cooldown;
    public int countdown, points;

    //Save data
    public float vol, muse, snd;
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
        scrollDis = Mathf.Clamp(Screen.height / 10, 120, 300);
        Debug.Log(Screen.width + " : " + Screen.height);
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
        vol = GetFloatData("Volume");
        muse = GetFloatData("Music");
        snd = GetFloatData("Effect");
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
        options = false;
        op2 = false;
        tutoring = true;
        sub = 0;
        maxObjects = 0;
        MR = trophy.GetComponent<Renderer>();

        //Update shop text
        list = draws;
        ConvertData(unlocked);
        list = basket;
        ConvertData(unlocked2);
        list = area;
        ConvertData(unlocked3);
        for (currentMenu = 0; currentMenu < DESIGNS.Length; currentMenu++) {
            for (int j = 0; j < DESIGNS[currentMenu]; j++) if (unlocks[currentMenu].Contains(j)) SetText(j);
        }
        currentMenu = 0;

        Volumes();
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

        //show/hide the correct things
        ShowMenus();
        trophy.SetActive(false);
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
        canDraw = !(paused || mained || shopping || options || op2);
        canDoodle = (mained || shopping || options);
        maxObjects = canDraw ? past : 0;

        //Add timer, max objects based on time passed
        if (canDraw && countdown == 0 && !tutoring) {
            difficultyTime += Time.deltaTime;
            cooldown -= Time.deltaTime;
        }
        past = Mathf.FloorToInt(Mathf.Clamp(Mathf.Pow(difficultyTime + 50, 2) / 4800 + 0.5f, 1, 26));

        //Update texts
        timer.text = (difficultyTime / 60).ToString("00") + ":" + (difficultyTime % 60).ToString("00");
        score.text = "Score: " + points.ToString();
        total.text = "Final Score:\n" + points.ToString();
        record.text = best.ToString();
        wallet.text = currency.ToString("00000");
        if (countdown != 0) gamedown.text = countdown.ToString();
        else gamedown.text = " ";

        //Fruit knocked away: lose
        if (!MR.isVisible && !mained && !rewarding && !shopping) Over();

        //Spawn new bomb when #bomb is below desired amount
        if (actives.Count < past && canDraw && countdown == 0 && cooldown < 0 && !tutoring) {
            //Find a spot far enough away from other bombs
            bool clear = false;
            while (!clear) {
                int height = Random.Range(-5 - (int)Mathf.Clamp01(past / 5), -1 + (int)Mathf.Clamp(past / 8, 0, 2));
                local = new Vector3(spawns[Random.Range(0, 2)].transform.position.x, height, 0);
                bool good = true;
                foreach (GameObject actor in actives) {
                    if (Vector3.Distance(actor.transform.position, local) < 4) good = false;
                }
                clear = good;
            }

            int weighted = Mathf.FloorToInt(Random.Range(0, 8) / 7);
            cooldown = Mathf.Clamp01(1.1f / past * 10);
            warnings.Insert(0, local);
            GameObject baller = Instantiate(prefabs[weighted, 0], warnings[0], Quaternion.identity) as GameObject;
            actives.Insert(0, baller);
            Instantiate(prefabs[weighted, 1], warnings[0]/1.5f, Quaternion.identity);
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

        //Give time to read tutorial text & trigger next on tap
        if (Input.GetMouseButton(0) && TT.requirement && read && tutoring && sub < checklist.transform.childCount - 1) {
            sub = Mathf.Clamp(sub + 1, 0, checklist.transform.childCount - 1);
            ShowMenus();
            read = false;
            StartCoroutine(Tutor());
            TT = checklist.transform.GetChild(sub).GetComponent<Tutorial>();
            TT.Reset();
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
        SFX.PlayOneShot(clip, volume * vol * snd);
    }

    //Exit exe (Non mobile only)
    public void Quit()
    {
        Application.Quit();
    }

    //Fruit position
    private void Reset()
    {
        trophy.transform.position = Vector3.zero;
        trophy.transform.eulerAngles = Vector3.zero;
        Rigidbody component = trophy.GetComponent<Rigidbody>();
        component.velocity = Vector3.zero;
        component.angularVelocity = Vector3.zero;
    }

    //Chain rewards
    private void Rewards()
    {
        currency = Mathf.Clamp(currency + chain * 5, 0, 99999);
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
        trophy.SetActive(!rewarding && !(lines || fruit || backs) && !options);
        rMenu.SetActive(rewarding);
        mMenu.SetActive(mained && !rewarding);
        pMenu.SetActive(paused);
        gMenu.SetActive(canDraw && !tutoring);
        sMenu.SetActive(shopping);
        optMenu.SetActive(options);
        sBase.SetActive(!(lines || fruit || backs));
        sLine.SetActive(lines);
        sFruit.SetActive(fruit);
        sBack.SetActive(backs);
        confirm.SetActive(confirming);
        refuse.SetActive(rejecting);
        gOver.SetActive((!MR.isVisible && !mained && !shopping && !options && !op2 && !paused && !tutoring));
        money.SetActive(!rewarding && !canDraw && !paused && !options && !op2);
        scored.SetActive(!rewarding && !canDraw && !paused && !options && !op2 && !(lines || fruit || backs));
        tMenu.SetActive(tutoring && !mained && !shopping && !options);
        for (int i = 0; i < checklist.transform.childCount; i++) checklist.transform.GetChild(i).gameObject.SetActive(false);
        checklist.transform.GetChild(sub).gameObject.SetActive(true);
    }

    //Update slider displays
    private void SliderDisplay()
    {
        sound[0].value = vol;
        sound[1].value = muse;
        sound[2].value = snd;
        sound[3].value = vol;
        sound[4].value = muse;
        sound[5].value = snd;
    }

    //Volume settings
    private void Volumes()
    {
        MusicG.volume = 0.14f * vol * muse;
        MusicM.volume = 0.14f * vol * muse;
    }

    //--------------------------------------------------------------------------------------------- BUTTON FUNCTIONS
    //Accept the daily rewards
    public void Accept()
    {
        money.gameObject.SetActive(true);
        Destroy(chest);
        mained = true;
        rewarding = false;
    }

    //Gameplay
    public void Game()
    {
        if (!rewarding && !shopping && (mained || tutoring)) {
            countdown = 3;
            StartCoroutine(Delay());
            paused = false;
            mained = false;
            tutoring = false;
            Reset();
        }
    }

    //Tutorial
    public void Help()
    {
        if (tutoring) {
            sub = 0;
            mained = false;
            paused = false;
            read = false;
            TT = checklist.transform.GetChild(sub).GetComponent<Tutorial>();
            TT.Reset();
            StartCoroutine(Tutor());
        } else Game();
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
        Reset();

        //Record highest, double reward $
        if (points > best) {
            best = Mathf.Clamp(points, 0, 99999);
            currency = Mathf.Clamp(currency + best, 0, 99999);
        }
        currency = Mathf.Clamp(currency + points, 0, 99999);
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

    //Settings menu from main
    public void Settings(bool onOff)
    {
        mained = !onOff;
        options = onOff;
        SliderDisplay();
        RecordSaveData();
    }

    //Settings menu from option
    public void Settings(int overload)
    {
        op2 = (overload == 1);
        paused = (overload == 0);
        SliderDisplay();
        optMen2.SetActive(op2);
        RecordSaveData();
    }

    //Reset shop position and open shop menu
    public void Shop()
    {
        foreach(GameObject menu in shopScroll) menu.transform.localPosition = Vector3.zero;
        mained = false;
        shopping = true;
    }

    //Slider movement
    public void Slide(int three)
    {
        if (Input.GetMouseButton(0) && (options || op2)) {
            vol = sound[0 + three].value;
            muse = sound[1 + three].value;
            snd = sound[2 + three].value;
        }
        Volumes();
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
        month = 0;
        day = 0;
        best = 0;
        vol = 0;
        muse = 0;
        snd = 0;
        chain = 0;
        currency = 0;
        mode = 0;
        food = 0;
        ground = 0;
    }

    //Return the saved float data, 1 otherwise
    private float GetFloatData(string Key)
    {
        if (PlayerPrefs.HasKey(Key)) {
            return PlayerPrefs.GetFloat(Key);
        } else {
            PlayerPrefs.SetFloat(Key, 1);
            return 1;
        }
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
        SaveData("Volume", vol);
        SaveData("Music", muse);
        SaveData("Effect", snd);
        SaveData("LastLogin", day);
        SaveData("LoginChain", chain);
        PlayerPrefs.SetString("Inventory", draws);
        PlayerPrefs.SetString("Fruits", basket);
        PlayerPrefs.SetString("Background", area);
    }

    //Write input data to input Key
    public void SaveData(string Key, int Data) { PlayerPrefs.SetInt(Key, Data); }
    public void SaveData(string Key, float Data) { PlayerPrefs.SetFloat(Key, Data); }

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

    //Delay to read tutorial text
    private IEnumerator Tutor()
    {
        yield return new WaitForSeconds(2);
        read = true;
    }

    //"Not enough cash" message lasts for 1.25sec
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.25f);
        rejecting = false;
    }
}