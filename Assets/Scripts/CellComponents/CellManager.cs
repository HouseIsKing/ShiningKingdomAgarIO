using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools;

public class CellManager : MonoBehaviour
{
    private List<GameObject> cellObjects = new List<GameObject>();
    private List<Cell> cells = new List<Cell>();
    private Material cellsColor = null;
    public TestAgarGamemodeManager agarGamemodeManager = null;
    public string playerName;
    public bool displayMass = true;
    public float GetScore()
    {
        float result = 0;
        foreach (Cell cell in cells)
        {
            result += cell.Mass;
        }
        return result;
    }
    public Vector2 TargetLocation
    {
        set;
        get;
    }
    public bool StartupComplete
    {
        get;
        private set;
    }
    public void ResetManager()
    {
        Start();
    }
    public bool IsGamemodeReady()
    {
        if(agarGamemodeManager == null)
        {
            print("Error, no gamemode was provided.");
            return false;
        }
        return true;
    }
    public void Setup()
    {
        StartupComplete = IsGamemodeReady();
    }
    private void Start()
    {
        Setup();
        if (StartupComplete)
        {
            cellsColor = agarGamemodeManager.cellMaterials[Random.Range(0, agarGamemodeManager.cellMaterials.Count)];
            cells.Add(SpawnCell());
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "No name";
            }
        }
        else
        {
            print("Error, CellManager failed to start and will be disabled now.");
        }
    }
    private Cell SpawnCell()
    {
        GameObject cellObj = Instantiate(agarGamemodeManager.cellObject, new Vector3(transform.localPosition.x, transform.localPosition.y), new Quaternion(transform.localRotation.x - 90, 0, 0, 90));
        cellObj.GetComponent<Renderer>().material = cellsColor;
        cellObjects.Add(cellObj);
        Cell cell = cellObj.GetComponent<Cell>();
        cell.cellManager = this;
        cell.Mass = agarGamemodeManager.cellStartingMass;
        return cell;
    }
    private Cell SpawnCell(Vector3 location)
    {
        GameObject cellObj = Instantiate(agarGamemodeManager.cellObject, location, new Quaternion(transform.localRotation.x - 90, 0, 0, 90));
        cellObj.GetComponent<Renderer>().material = cellsColor;
        cellObjects.Add(cellObj);
        Cell cell = cellObj.GetComponent<Cell>();
        cell.cellManager = this;
        cell.Mass = agarGamemodeManager.cellStartingMass;
        return cell;
    }
    public List<Cell> GetCells()
    {
        return cells;
    }
	// Update is called once per frame
	void Update ()
    {
        foreach (Cell cell in cells)
        {
            cell.TargetLocation = TargetLocation;
        }
	}
    private void ShuffleCells()
    {
        List<Cell> result = new List<Cell>();
        while (cells.Count>0)
        {
            int index = Random.Range(0, cells.Count);
            result.Add(cells[index]);
            cells.RemoveAt(index);
        }
        cells = result;
    }
    private Cell Split(Cell a, float angle)
    {
        Cell result = SpawnCell(a.transform.localPosition);
        result.Mass = a.Mass;
        a.currentMergeTime = 0;
        result.splitSpeed = Calculate.BreakVector(angle, Time.deltaTime * a.Speed * 3);
        result.currentMergeTime = 0;
        result.isSpliting = true;
        return result;
    }
    public void AllSplit()
    {
        ShuffleCells();
        int splitNum = cells.Count;
        for (int i = 0; i < splitNum && cells.Count < agarGamemodeManager.GetMaxSplits(); i++)
        {
            if (cells[i].Mass >= 36)
            {
                cells[i].Mass /= 2;
                cells.Add(Split(cells[i], cells[i].Angle));
            }
        }
    }
    public void SpecificSplit(Cell splitingCell)
    {
        int splitNum = agarGamemodeManager.GetMaxSplits() - cells.Count;
        bool doneSplit = false;
        while (cells.Count != agarGamemodeManager.GetMaxSplits()&&!doneSplit)
        {
            if (splitingCell.Mass / splitNum < 36)
            {
                while(splitingCell.Mass / splitNum<10)
                {
                    splitNum--;
                }
                splitingCell.Mass /= splitNum;
                for (int i = 0; i < splitNum; i++)
                {
                    cells.Add(Split(splitingCell, Random.Range(0.0f, 360.0f)));
                }
                doneSplit = true;
            }
            else
            {
                splitNum--;
                splitingCell.Mass /= 2;
                cells.Add(Split(splitingCell, Random.Range(0.0f, 360.0f)));
            }
        }
    }
    public void ThrowMass()
    {
        foreach (Cell cell in cells)
        {
            if (cell.Mass > 32)
            {
                float angle = cell.Angle + Random.Range(-10.0f, 10.0f);
                Vector2 scaleLoc = Calculate.BreakVector(angle, UnitConvereter.ScaleToCm(cell.transform.localScale.x));
                Vector2 finalLoc = new Vector2(cell.transform.localPosition.x + scaleLoc.x, cell.transform.localPosition.y + scaleLoc.y);
                float mass = agarGamemodeManager.thrownMassLoss * 0.8f;
                float x = UnitConvereter.CellUnitToScale(Mathf.Sqrt(mass * 100));
                finalLoc += Calculate.BreakVector(angle, UnitConvereter.ScaleToCm(x));
                if (agarGamemodeManager.IsTouchingWall(finalLoc))
                {
                    angle += 180;
                    scaleLoc = Calculate.BreakVector(angle, UnitConvereter.ScaleToCm(cell.transform.localScale.x));
                    finalLoc = new Vector2(cell.transform.localPosition.x + scaleLoc.x, cell.transform.localPosition.y + scaleLoc.y);
                    x = UnitConvereter.CellUnitToScale(Mathf.Sqrt(mass * 100));
                    finalLoc += Calculate.BreakVector(angle, UnitConvereter.ScaleToCm(x));
                }
                GameObject th = Instantiate(agarGamemodeManager.throwMassObject, finalLoc, cell.transform.localRotation);
                th.GetComponent<ThrownMass>().maxGlidingTime = agarGamemodeManager.splitTime/2;
                th.GetComponent<ThrownMass>().vel = Calculate.BreakVector(angle, Time.unscaledDeltaTime * cell.Speed * 3);
                cell.Mass -= agarGamemodeManager.thrownMassLoss;
                th.transform.localScale = new Vector3(x, x, x);
                th.GetComponent<ThrownMass>().mass = mass;
                th.GetComponent<ThrownMass>().map = agarGamemodeManager.GetMapSize();
                th.GetComponent<MeshRenderer>().material = cellsColor;
            }
        }
    }
}
