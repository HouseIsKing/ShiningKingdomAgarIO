 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools;
using TMPro;

public class TestAgarGamemodeManager : MonoBehaviour
{
    private float massChange = 0.998f;
    public float cellStartingMass = 10;
    public int startingFood = 1000;
    public int startingVirus = 15;
    public float virusMass = 100;
    public int virusTimesGrow = 7;
    public int maxVirus = 25;
    public int spawnRateVirus = 2;
    public int virusNum = 0;
    public int foodCount = 0;
    public float splitTime = 1;
    public int maxFoodCount = 1500;
    public int spawnRate = 20;
    public int maxSplits = 16;
    public float thrownMassLoss = 16;
    public float minimumJoinTime = 30;
    public int numOfAI = 0;
    public int maxNumOfAI = 3;
    public int spawndelay = 30;
    public float RespawnTime = 10;
    public float massRequiredToWin = 5000;
    private Vector2 mapSize = new Vector2();//In CM
    private GameObject map = null;
    public GameObject throwMassObject = null;
    public GameObject playerManager = null;
    public GameObject foodCell = null;
    public GameObject cellObject = null;
    public GameObject virus = null;
    public GameObject aiObject = null;
    public GameObject pauseMenuObject = null;
    public GameObject pauseMenu = null;
    public GameObject optionsMenuObject = null;
    public GameObject textObject = null;
    private GameObject endOfGameMenu = null;
    public GameObject endOfGameMenuObject = null;
    private GameObject endOfGameContentObject = null;
    public GameObject scoreboardObject = null;
    public GameObject scoreShowObject = null;
    private List<TMP_Text> scoreBoardText = null;
    private List<TESTAI> AIList = null;
    private List<CellManager> players = null;
    public List<Material> cellMaterials = new List<Material>();
    public bool playerSpectator = false;
    public bool isPauseable = false;
    public bool isEnd = false;
    public Vector2 baseView = new Vector2(1920,1080);
    public float GetVirusMass()
    {
        return virusMass;
    }
    public float MassChange
    {
        get
        {
            return massChange;
        }

        protected set
        {
            massChange = value;
        }
    }
    public float GetMinimumJoinTime()
    {
        return minimumJoinTime;
    }
    public int GetMaxSplits()
    {
        return maxSplits;
    }
    private bool GetFloor()
    {
        map = transform.GetChild(0).gameObject;
        if (map == null && !map.CompareTag("Floor"))
        {
            print("Error failed to find the map.");
            return false;
        }
        return true;
    }
    private bool IsValidMaterials()
    {
        if (cellMaterials.Count>0)
        {
            foreach (Material mat in cellMaterials)
            {
                if (mat == null)
                {
                    print("Error, one of the materials is null");
                    return false;
                }
            }
            return true;
        }
        print("Error, no cell materials detected");
        return false;
    }
    private bool IsValidManagers()
    {
        if (playerManager == null)
        {
            print("Error, One of the game managers is null");
            return false;
        }
        if (playerManager.GetComponent<PlayerManager>() == null)
        {
            print("Error, the provided Player Manager has no Player Manager");
            return false;
        }
        return true;
    }
    public bool StartupComplete
    {
        get
        {
            return IsStartupComplete();
        }
    }
    public bool IsStartupComplete()
    {
        bool result;
        result = GetFloor();
        result &= IsValidMaterials();
        result &= IsValidManagers();
        result &= IsValidObjects();
        return result;
    }
    private bool IsValidObjects()
    {
        if (foodCell == null || !foodCell.CompareTag("Food"))
        {
            print("Error, no foodCell Provided, or the provided food cell is not food.");
            return false;
        }
        if (cellObject == null || cellObject.GetComponent<Cell>() == null || !cellObject.CompareTag("Cell"))
        {
            print("Error, no cell object provided or the Provided cell object is not a cell");
            return false;
        }
        if (virus == null || !virus.CompareTag("Virus") || virus.GetComponent<Virus>() == null)
        {
            print("Error, no Virus Provided, or the provided Virus is not Virus.");
            return false;
        }
        if (throwMassObject == null || !throwMassObject.CompareTag("ThrownMass"))
        {
            print("Error, no thrown mass object provided or the provided object isn't a thrownMass Object");
            return false;
        }
        if (pauseMenuObject == null && isPauseable)
        {
            print("Error no pause menu is provided.");
            return false;
        }
        if (optionsMenuObject == null && isPauseable)
        {
            print("Error no options menu is provided");
            return false;
        }
        if (textObject == null)
        {
            print("Error no text object is provided");
            return false;
        }
        if (endOfGameMenuObject == null && isPauseable)
        {
            print("Error no EndGameMenu is provided");
            return false;
        }
        if (scoreboardObject == null && isPauseable)
        {
            print("Error no scoreboard provided");
            return false;
        }
        if (scoreShowObject == null && !playerSpectator)
        {
            print("Error no ScoreShow object provided");
            return false;
        }
        return true;
    }
    protected virtual void SetMapSize()
    {
        mapSize = new Vector2(5 * map.transform.localScale.x, 5 * map.transform.localScale.y);
    }
    protected virtual void SpawnStartingFood()
    {
        for (int i = 0; i < startingFood; i++)
        {
            SpawnF();
        }
    }
    private void SpawnF()
    {
        Vector2 spawnPoint = FindGoodSpawnPoint();
        GameObject food = Instantiate(foodCell, new Vector3(spawnPoint.x, spawnPoint.y, 0), new Quaternion(transform.localRotation.x - 90, 0, 0, 90));
        float x = UnitConvereter.CellUnitToScale(Mathf.Sqrt(Random.Range(1.0f, 2.0f) * 100));
        food.transform.localScale = new Vector3(x, x, x);
        food.GetComponent<MeshRenderer>().material = cellMaterials[Random.Range(0, cellMaterials.Count)];
        foodCount++;
    }
    protected virtual void SpawnAI()
    {
        if (playerSpectator)
        {
            AIList = new List<TESTAI>();
        }
        for (int i = 0; i < maxNumOfAI; i++)
        {
            GameObject AI = Instantiate(aiObject, new Vector3(Random.Range(-mapSize.x, mapSize.x), Random.Range(-mapSize.y, mapSize.y), 0), new Quaternion());
            AI.GetComponent<TESTAI>().agarGamemodeManager = this;
            numOfAI++;
            AI.GetComponent<TESTAI>().AIName = "Test Bot " + numOfAI.ToString();
            players.Add(AI.GetComponent<CellManager>());
            if (playerSpectator)
            {
                AIList.Add(AI.GetComponent<TESTAI>());
            }
        }
    }
    private void SpawnPlayer()
    {
        Vector2 spawnPoint = FindGoodSpawnPoint();
        GameObject a = Instantiate(playerManager, new Vector3(spawnPoint.x, spawnPoint.y, -10000), new Quaternion());
        players.Add(a.GetComponent<CellManager>());
        if (!playerSpectator)
        {
            GameObject obj = Instantiate(scoreShowObject);
            obj.transform.SetParent(transform.GetChild(1));
            RectTransform rectTransform = (RectTransform)obj.transform;
            rectTransform.offsetMax = new Vector2();
            rectTransform.offsetMin = new Vector2();
            a.GetComponent<PlayerManager>().scoreShow = obj.GetComponent<TMP_Text>();
        }
        if (playerSpectator)
        {
            a.GetComponent<PlayerManager>().spectator = true;
            a.GetComponent<PlayerManager>().spectatedAI = AIList;
            players.Remove(a.GetComponent<CellManager>());
            Destroy(a.GetComponent<CellManager>());
            Destroy(a.GetComponent<ViewManager>());
        }
        a.GetComponent<PlayerManager>().agarGamemodeManager = this;
    }
    public Vector2 GetBasicView()
    {
        return baseView;
    }
    public Vector2 GetMapSize()
    {
        return mapSize;
    }
    public void SpawnFood()
    {
        for (int i = 0; i < spawnRate && foodCount < maxFoodCount; i++)
        {
            SpawnF();
        }
    }
    private void Awake()
    {
        if (StartupComplete)
        {
            players = new List<CellManager>();
            SpawnPauseMenu();
            SpawnEndOfGameMenu();
            SpawnScoreboard();
            foodCount = 0;
            baseView = new Vector2(1920,1080);
            SetMapSize();
            SpawnAI();
            SpawnPlayer();
            SpawnStartingFood();
            SpawnStartingVirus();
            InvokeRepeating(nameof(SpawnFood), 4f, 0.75f);
            InvokeRepeating(nameof(SpawnVirus), 4f, 15);
        }
        else
        {
            print("Error, Gamemode failed to start");
        }
    }
    private void SpawnVirus()
    {
        for (int i = 0; i < spawnRateVirus && virusNum < maxVirus; i++)
        {
            SpawnV();
        }
    }
    private void SpawnStartingVirus()
    {
        for (int i = 0; i < startingVirus; i++)
        {
            SpawnV();
        }
    }
    private void SpawnV()
    {
        Vector2 spawnPoint = FindGoodSpawnPoint();
        GameObject v = Instantiate(virus, new Vector3(spawnPoint.x, spawnPoint.y, 0), new Quaternion());
        v.GetComponent<Virus>().mass = virusMass;
        v.GetComponent<Virus>().agarGamemode = this;
        float x = UnitConvereter.CellUnitToScale(Mathf.Sqrt(virusMass * 100))/2;
        v.transform.localScale = new Vector3(x, x, x);
        v.transform.localPosition = new Vector3(v.transform.localPosition.x, v.transform.localPosition.y, -v.transform.localScale.z * 2);
        virusNum++;
    }
    public bool IsTouchingWall(Vector2 loc)
    {
        if (loc.x >= mapSize.x || loc.x <= -mapSize.x)
        {
            return true;
        }
        if (loc.y >= mapSize.y || loc.y <= -mapSize.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public Vector2 FindGoodSpawnPoint()
    {
        Vector2 result = new Vector2();
        bool isFound = false;
        for (int i = 0; i < 100 && !isFound; i++)
        {
            result = new Vector2(Random.Range(-mapSize.x, mapSize.x), Random.Range(-mapSize.y, mapSize.y));
            Collider[] colliders = Physics.OverlapBox(result, new Vector3(0.1f, 0.1f, 0.1f));
            if (colliders.Length == 0)
            {
                isFound = true;
            }
            foreach (Collider coll in colliders)
            {
                if (!coll.gameObject.CompareTag("Food") && !coll.gameObject.CompareTag("Virus") && !coll.gameObject.CompareTag("Cell"))
                {
                    isFound = true;
                }
            }
        }
        return result;
    }
    private void SpawnPauseMenu()
    {
        if (isPauseable)
        {
            pauseMenu = Instantiate(pauseMenuObject);
            pauseMenu.SetActive(false);
            pauseMenu.transform.SetParent(transform.GetChild(1).transform);
            pauseMenu.GetComponent<PauseMenuManager>().optionsObject = Instantiate(optionsMenuObject);
            pauseMenu.GetComponent<PauseMenuManager>().optionsObject.transform.SetParent(transform.GetChild(1).transform);
            pauseMenu.GetComponent<PauseMenuManager>().optionsObject.SetActive(false);
            RectTransform rectTransform = (RectTransform)pauseMenu.transform;
            rectTransform.offsetMax = new Vector2();
            rectTransform.offsetMin = new Vector2();
            rectTransform = (RectTransform)pauseMenu.GetComponent<PauseMenuManager>().optionsObject.transform;
            rectTransform.offsetMax = new Vector2();
            rectTransform.offsetMin = new Vector2();
        }
    }
    private void SpawnEndOfGameMenu()
    {
        if (isPauseable)
        {
            endOfGameMenu = Instantiate(endOfGameMenuObject);
            endOfGameMenu.SetActive(false);
            endOfGameMenu.transform.SetParent(transform.GetChild(1));
            RectTransform rectTransform = (RectTransform)endOfGameMenu.transform;
            rectTransform.offsetMax = new Vector2();
            rectTransform.offsetMin = new Vector2();
        }
    }
    private void SpawnScoreboard()
    {
        if (isPauseable)
        {
            scoreBoardText = new List<TMP_Text>();
            GameObject ScoreBoard = Instantiate(scoreboardObject);
            ScoreBoard.SetActive(true);
            ScoreBoard.transform.SetParent(transform.GetChild(1));
            RectTransform rectTransform = (RectTransform)ScoreBoard.transform;
            rectTransform.offsetMax = new Vector2();
            rectTransform.offsetMin = new Vector2();
            for (int i = 0; i < 10; i++)
            {
                GameObject textObj = Instantiate(textObject);
                scoreBoardText.Add(textObj.GetComponent<TMP_Text>());
                textObj.transform.SetParent(ScoreBoard.transform);
                scoreBoardText[i].enableAutoSizing = true;
                scoreBoardText[i].fontSizeMin = 0;
                scoreBoardText[i].enableWordWrapping = false;
                scoreBoardText[i].text = "";
            }
        }
    }
    private void Update()
    {
        if (!isEnd && isPauseable)
        {
            foreach (CellManager player in players)
            {
                if (player.GetScore() >= massRequiredToWin)
                {
                    EndOfGame();
                }
            }
            UpdateScoreboard();
        }
    }
    private void UpdateScoreboard()
    {
        Queue<CellManager> cellManagers = SortPlayers(new List<CellManager>(players));
        for (int i = 0; i < 10 && i < cellManagers.Count; i++)
        {
            CellManager p = cellManagers.Dequeue();
            scoreBoardText[i].text = "#" + (i + 1) + ": " + p.playerName + " " + p.GetScore().ToString("0.00");
            if (p.gameObject.GetComponent<PlayerManager>() != null)
            {
                scoreBoardText[i].color = new Color(180, 0, 0);
            }
            else
            {
                scoreBoardText[i].color = new Color(255,255,255);
            }
        }
    }
    private void EndOfGame()
    {
        isEnd = true;
        Time.timeScale = 0;
        endOfGameMenu.SetActive(true);
        endOfGameContentObject = endOfGameMenu.transform.GetChild(1).GetChild(0).GetChild(0).gameObject;
        Transform parent = endOfGameContentObject.transform;
        Queue<CellManager> cellManagers = SortPlayers(new List<CellManager>(players));
        for (int i = 0; i < players.Count; i++)
        {
            CellManager p = cellManagers.Dequeue();
            GameObject textObj = Instantiate(textObject);
            textObj.transform.SetParent(parent);
            TMP_Text text = textObj.GetComponent<TMP_Text>();
            text.text = "#" + (i + 1) + ": " + p.playerName +" "+p.GetScore().ToString("0.00");
            if (p.gameObject.GetComponent<PlayerManager>() != null)
            {
                text.color = new Color(180, 0, 0);
                if (i == 0)
                {
                    endOfGameMenu.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = "You win!";
                }
            }
        }
    }
    private Queue<CellManager> SortPlayers(List<CellManager> cellManagers)
    {
        Queue<CellManager> result = new Queue<CellManager>();
        for (int i = 0; i < players.Count; i++)
        {
            CellManager helper = HighScore(cellManagers);
            result.Enqueue(helper);
            cellManagers.Remove(helper);
        }
        return result;
        
    }
    private CellManager HighScore(List<CellManager> cellManagers)
    {
        float max = cellManagers[0].GetScore();
        CellManager result = cellManagers[0];
        foreach (CellManager player in cellManagers)
        {
            if (player.GetScore()>max)
            {
                max = player.GetScore();
                result = player;
            }
        }
        return result;
    }
}
