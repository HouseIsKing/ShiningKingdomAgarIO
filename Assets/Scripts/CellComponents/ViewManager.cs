using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools;

public class ViewManager : MonoBehaviour
{
    private CellManager cellManager = null;
    private TestAgarGamemodeManager agarGamemodeManager = null;
    private Vector2 viewSize;//In CM units.
    public bool StartupComplete
    {
        get;
        private set;
    }
    private void Setup()
    {
        StartupComplete = true;
        cellManager = GetComponent<CellManager>();
        if (cellManager == null)
        {
            print("Error no cellManager Detected");
            StartupComplete = false;
            return;
        }
        if (!cellManager.StartupComplete)
        {
            StartupComplete = false;
            print("Error cellManager failed to start");
            return;
        }
        agarGamemodeManager = cellManager.agarGamemodeManager;
    }
    public Vector2 GetViewLocation()
    {
        return Location();
    }
    public Vector2 GetViewSize()
    {
        return viewSize;
    }
    private Vector2 Size()
    {
        Vector2 result = new Vector2(agarGamemodeManager.baseView.x, agarGamemodeManager.baseView.y);
        List<Cell> cells = cellManager.GetCells();
        foreach (Cell cell in cells)
        {
            result.x += (1 - 1 / 3.5860857f) * Mathf.Sqrt(5) * Mathf.Sqrt(cell.Mass * 100);
            result.y += (1 - 1 / 3.5860857f) * Mathf.Sqrt(5) * Mathf.Sqrt(cell.Mass * 100);
        }
        result = new Vector2(UnitConvereter.AUToCm(result.x), UnitConvereter.AUToCm(result.y));
        return result;
    }
    private Vector2 Location()
    {
        List<Cell> cells = cellManager.GetCells();
        float x = 0;
        float y = 0;
        foreach (Cell cell in cells)
        {
            x += cell.transform.localPosition.x;
            y += cell.transform.localPosition.y;
        }
        x /= cells.Count;
        y /= cells.Count;
        return new Vector2(x, y);
    }

    // Use this for initialization
    void Start ()
    {
        Setup();
        if (!StartupComplete)
        {
            print("View manager failed to start and will be disabled");
        }
        else
        {
            viewSize = Size();
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (StartupComplete)
        {
            viewSize = Size();
        }	
	}
}
