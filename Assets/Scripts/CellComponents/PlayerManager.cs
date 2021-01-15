using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private CellManager cellManager = null;
    private ViewManager viewManager = null;
    private Camera myCamera = null;
    public TestAgarGamemodeManager agarGamemodeManager = null;
    private float timer = 0;
    public bool spectator = false;
    public List<TESTAI> spectatedAI = null;
    private TESTAI currentlySpectating = null;
    public TMPro.TMP_Text scoreShow = null;
    public string playerName;
    public bool StartupComplete
    {
        get;
        private set;
    }
    private void Setup()
    {
        StartupComplete = true;
        cellManager = GetComponent<CellManager>();
        viewManager = GetComponent<ViewManager>();
        myCamera = GetComponent<Camera>();
        if (agarGamemodeManager == null)
        {
            print("Error no gamemode was provided");
            StartupComplete = false;
            return;
        }
        if (myCamera == null)
        {
            print("Error, camera is not found");
            StartupComplete = false;
            return;
        }
        if (viewManager == null)
        {
            print("Error, View manager is not found");
            StartupComplete = false;
            return;
        }
        if (cellManager == null)
        {
            print("Error, Cell manager is not found");
            StartupComplete = false;
            return;
        }
        if (scoreShow == null && !spectator)
        {
            print("Error no scoreShow detected");
            StartupComplete = false;
            return;
        }
        cellManager.agarGamemodeManager = agarGamemodeManager;
        playerName = SaveLoad.Load().playerName;
        cellManager.playerName = playerName;
    }
	void Start ()
    {
        if (spectator)
        {
            myCamera = GetComponent<Camera>();
            if (spectatedAI.Count == 0 || myCamera == null)
            {
                print("Error, no AI found to spectate Or no Camera has been found, Or no pause menu has been found.");
                StartupComplete = false;
            }
            else
            {
                StartupComplete = true;
                ChangeSpectate();
                InvokeRepeating(nameof(ChangeSpectate), 10, 10);
            }
        }
        else
        {
            Setup();
            if (!StartupComplete)
            {
                print("Error, failed to start Player Manager it will now be disabled");
            }
            else
            {
                cellManager.TargetLocation = myCamera.ScreenToWorldPoint(Input.mousePosition);
            }
        }
	}
	void Update ()
    {
        if (StartupComplete && !spectator)
        {
            UpdateScoreShow();
            if (cellManager.GetCells().Count != 0)
            {
                cellManager.TargetLocation = myCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 viewLoc = viewManager.GetViewLocation();
                transform.localPosition = new Vector3(viewLoc.x, viewLoc.y, transform.localPosition.z);
                myCamera.orthographicSize = Mathf.Lerp(myCamera.orthographicSize, viewManager.GetViewSize().x, Time.deltaTime * 10);
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    cellManager.AllSplit();
                }
                if (Input.GetKeyDown(KeyCode.W))
                {
                    cellManager.ThrowMass();
                }

            }
            else
            {
                ResetPlayer();
            }
        }
        else
        {
            if (StartupComplete)
            {
                Spectate();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) && agarGamemodeManager.isPauseable && Time.timeScale == 0)
        {
            agarGamemodeManager.pauseMenu.SetActive(false);
            agarGamemodeManager.pauseMenu.GetComponent<PauseMenuManager>().optionsObject.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape) && agarGamemodeManager.isPauseable)
            {
                agarGamemodeManager.pauseMenu.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }
    private void UpdateScoreShow()
    {
        scoreShow.text = "Score: " + cellManager.GetScore().ToString("0.00");
    }
    private void ResetPlayer()
    {
        if (timer >= agarGamemodeManager.RespawnTime)
        {
            Vector2 spawnPoint = agarGamemodeManager.FindGoodSpawnPoint();
            transform.localPosition = new Vector3(spawnPoint.x, spawnPoint.y, transform.localPosition.z);
            cellManager.ResetManager();
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
    private void Spectate()
    {
        if (currentlySpectating.HasCells)
        {
            ViewManager viewManage = currentlySpectating.viewManager;
            myCamera.orthographicSize = Mathf.Lerp(myCamera.orthographicSize, viewManage.GetViewSize().y, Time.deltaTime * 10);
            transform.localPosition = new Vector3(viewManage.GetViewLocation().x, viewManage.GetViewLocation().y, transform.localPosition.z);
        }
        else
        {
            ChangeSpectate();
        }
    }
    private void ChangeSpectate()
    {
        do
        {
            currentlySpectating = spectatedAI[Random.Range(0, spectatedAI.Count)];
        } while (!currentlySpectating.HasCells);
    }
}
