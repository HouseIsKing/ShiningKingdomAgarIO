using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools;
using TMPro;

public class Cell : MonoBehaviour
{
    public float currentMergeTime = 0;
    private float targetMergeTime;
    public CellManager cellManager;
    public List<Cell> origin = new List<Cell>();
    public TextMeshPro displayInfo;
    private Vector2 colidingVelocity = new Vector2();
    private readonly List<GameObject> collidedObjects = new List<GameObject>();
    private float speed = 50;
    public float massChange;
    public bool isSpliting = false;
    private float time = 0;
    private float mass;
    private readonly float maxTime = 1f;
    public Vector2 splitSpeed;
    private float timeToStopSpliting;
    public bool isTouchingWall;
    public bool showMass = true;
    public string displayName;
    private Dictionary<GameObject, Vector2> colidSpeeds = new Dictionary<GameObject, Vector2>();
    private void SetOrigin()
    {
        origin = new List<Cell>();
        Vector3 currentLoc = transform.localPosition;
        float z = UnitConvereter.ScaleToCm(transform.localScale.x);
        float r = UnitConvereter.ScaleToCm(transform.localScale.x);
        Collider[] col = Physics.OverlapCapsule(new Vector3(currentLoc.x, currentLoc.y, currentLoc.z + z + r), new Vector3(currentLoc.x, currentLoc.y, currentLoc.z - z - r), r);
        for (int i = 0; i < col.Length; i++)
        {
            origin.Add(col[i].GetComponent<Cell>());
        }
    }
    public float Angle
    {
        get;
        private set;
    }
    public Vector2 TargetLocation
    {
        set;
        private get;
    }
    public float Mass
    {
        get
        {
            return mass;
        }
        set
        {
            mass = value;
            time = 0;
        }
    }
    public float Speed
    {
        get
        {
            return speed;
        }
        private set
        {
            speed = value;
        }
    }
    void UpdateMass()
    {
        Mass *= massChange;
    }
    private void SetScale()
    {
        float x = UnitConvereter.CellUnitToScale(Mathf.Sqrt(Mass * 100));
        transform.localScale = new Vector3(x, x, x);
    }
    private void UpdateScale()
    {
        float x = UnitConvereter.CellUnitToScale(Mathf.Sqrt(Mass * 100));
        if (time<maxTime)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(x, x, x), time / maxTime);
            time += Time.deltaTime;
        }
        else
        {
            transform.localScale = new Vector3(x, x, x);
        }
    }
    private void UpdateColidSpeed()
    {
        Dictionary<GameObject, Vector2> helper = new Dictionary<GameObject, Vector2>(colidSpeeds);
        colidingVelocity = new Vector2();
        foreach (GameObject obj in helper.Keys)
        {
            if (obj == null)
            {
                colidSpeeds.Remove(obj);
            }
        }
        foreach (Vector2 vel in colidSpeeds.Values)
        {
            colidingVelocity += vel;
        }
    }
    // Use this for initialization
    void Start ()
    {
        displayInfo = GetComponentInChildren<TextMeshPro>();
        displayName = cellManager.playerName;
        showMass = cellManager.displayMass;
        SetText();
        massChange = cellManager.agarGamemodeManager.MassChange;
        SetScale();
        InvokeRepeating(nameof(UpdateMass), 3f, 0.5f);
        timeToStopSpliting = 0;
        isTouchingWall = false;
        SetTargetMergeTime();
        SetOrigin();
    }
    private void Update()
    {
        CheckCollosion();
        UpdateColidSpeed();
        UpdateScale();
        SetSpeed();
        Move();
        SetText();
        SetTargetMergeTime();
        UpdateMergeTime();
        if (name == "Cell")
        {
            print(TargetLocation);
        }
    }
    private void SetTargetMergeTime()
    {
        targetMergeTime = cellManager.agarGamemodeManager.GetMinimumJoinTime() + 2.33f * Mass / 100;
    }
    private void SetSpeed()
    {
        speed = 800 * Mathf.Pow(0.99985f, Mass);
    }
    private void UpdateMergeTime()
    {
        if (targetMergeTime+1>=currentMergeTime)
        {
            currentMergeTime += Time.deltaTime;
        }
    }
    private void Move()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Angle = Calculate.CalAngle(transform.localPosition, TargetLocation);
        float c = Mathf.Sqrt(Mathf.Pow(TargetLocation.x - transform.localPosition.x, 2) + Mathf.Pow(TargetLocation.y - transform.localPosition.y, 2));
        Vector2 brokenVec = Calculate.BreakVector(Angle, speed * Time.deltaTime);
        if (speed * Time.deltaTime * Time.deltaTime > c)
        {
            brokenVec = Calculate.BreakVector(Angle, c);
        }
        if (isSpliting)
        {
            if (!isTouchingWall)
            {
                splitSpeed.x *= CheckWallBounce().x;
                splitSpeed.y *= CheckWallBounce().y;
            }
            Vector2 addSpeed = Vector2.Lerp(splitSpeed, new Vector2(0, 0), timeToStopSpliting / cellManager.agarGamemodeManager.splitTime);
            brokenVec = CheckWalls(brokenVec + colidingVelocity, Angle);
            CheckTouchingWalls(brokenVec);
            rb.velocity = new Vector2(brokenVec.x + addSpeed.x, brokenVec.y + addSpeed.y);
            if (timeToStopSpliting >= cellManager.agarGamemodeManager.splitTime)
            {
                splitSpeed = new Vector2();
                isSpliting = false;
                origin.RemoveRange(0, origin.Count);
            }
            else
            {
                timeToStopSpliting += Time.deltaTime;
            }
        }
        else
        {
            Vector2 vel = new Vector2(brokenVec.x + colidingVelocity.x, brokenVec.y + colidingVelocity.y);
            if (Mathf.Sqrt(vel.x * vel.x + vel.y * vel.y) > Speed * Time.deltaTime)
            {
                vel = Calculate.BreakVector(Calculate.CalAngle(vel), speed * Time.deltaTime);
            }
            rb.velocity = CheckWalls(vel, Calculate.CalAngle(brokenVec + colidingVelocity));
        }
    }
    private Vector2 CheckWallBounce()
    {
        Vector2 map = cellManager.agarGamemodeManager.GetMapSize();
        int resultX = 1;
        int resultY = 1;
        if (transform.localPosition.x >= map.x || transform.localPosition.x <= -map.x)
        {
            resultX = -1;
        }
        if (transform.localPosition.y >= map.y || transform.localPosition.y <= -map.y)
        {
            resultY = -1;
        }
        if (resultX == -1 || resultY == -1)
        {
            isTouchingWall = true;
        }
        else
        {
            isTouchingWall = false;
        }
        return new Vector2(resultX, resultY);
    }
    private Vector2 CheckWalls(Vector2 a,float angle)
    {
        Vector2 map = cellManager.agarGamemodeManager.GetMapSize();
        Vector2 result = a;
        if (transform.localPosition.x >= map.x && (angle > 270 || angle < 90))
        {
            result.x = map.x - transform.localPosition.x;
        }
        if (transform.localPosition.x <= -map.x && angle > 90 && angle < 270)
        {
            result.x = -map.x - transform.localPosition.x;
        }
        if (transform.localPosition.y >= map.y && angle > 0 && angle < 180)
        {
            result.y = map.y - transform.localPosition.y;
        }
        if (transform.localPosition.y <= -map.y && angle > 180 && angle < 360)
        {
            result.y = -map.y - transform.localPosition.y;
        }
        return result;
    }
    private void CheckTouchingWalls(Vector2 a)
    {
        Vector2 map = cellManager.agarGamemodeManager.GetMapSize();
        Vector2 result = a;
        if (transform.localPosition.x >= map.x)
        {
            result.x = map.x - transform.localPosition.x;
        }
        if (transform.localPosition.x <= -map.x)
        {
            result.x = -map.x - transform.localPosition.x;
        }
        if (transform.localPosition.y >= map.y)
        {
            result.y = map.y - transform.localPosition.y;
        }
        if (transform.localPosition.y <= -map.y)
        {
            result.y = -map.y - transform.localPosition.y;
        }
        if (result.Equals(a))
        {
            isTouchingWall = false;
        }
    }
    private void SetText()
    {
        string text = "";
        text += displayName;
        text += "\n";
        if (showMass)
        {
            text += mass.ToString("0.00");
        }
        displayInfo.text = text;
    }
    private void ColidingOwnCells(GameObject cell)
    {
        Cell otherCell = cell.GetComponent<Cell>();
        if (currentMergeTime >= targetMergeTime || otherCell.targetMergeTime <= otherCell.currentMergeTime)
        {
            if (Mass >= otherCell.Mass)
            {
                if (Calculate.CanEat(gameObject, cell))
                {
                    currentMergeTime = 0;
                    Mass += otherCell.Mass;
                    cellManager.GetCells().Remove(otherCell);
                    Destroy(cell);
                }
            }
        }
        else if (origin.IndexOf(otherCell) == -1 && otherCell.origin.IndexOf(this) == -1)
        {
            float angle = Calculate.CalAngle(transform.localPosition, cell.transform.localPosition);
            float radius = UnitConvereter.ScaleToCm(transform.localScale.x);
            float radiusOther = UnitConvereter.ScaleToCm(otherCell.transform.localScale.x);
            float minDistance = radius * 0.5f + radiusOther * 0.5f;
            float c = Vector2.Distance(transform.position, otherCell.transform.position) - radius * 0.5f - radiusOther * 0.5f;
            float alpha = c / minDistance;
            float power = Mathf.Lerp(speed * Time.deltaTime * 2f, 0, alpha);
            Vector2 colidSpeed = Calculate.BreakVector(angle + 180, power);
            colidSpeeds.Add(cell, colidSpeed);
        }
    }
    private void EatVirus(Virus v)
    {
        Mass += v.mass;
        Destroy(v.gameObject);
        cellManager.agarGamemodeManager.virusNum--;
        cellManager.SpecificSplit(this);
    }
    private void OnTriggerEnter(Collider other)
    {
        GameObject colidedActor = other.gameObject;
        collidedObjects.Add(colidedActor);
    }
    private void CheckCollosion()
    {
        colidSpeeds = new Dictionary<GameObject, Vector2>();
        for (int i = 0; i < collidedObjects.Count; i++)
        {
            GameObject actor = collidedObjects[i];
            if (actor == null)
            {
                collidedObjects.Remove(actor);
                i--;
            }
            else
            {
                if (actor.CompareTag("Food"))
                {
                    if (Calculate.CanEat(gameObject, actor))
                    {
                        Mass++;
                        Destroy(actor);
                        cellManager.agarGamemodeManager.foodCount--;
                    }
                }
                if (actor.CompareTag("Cell"))
                {
                    CellManager otherCellManager = actor.GetComponent<Cell>().cellManager;
                    if (otherCellManager.Equals(cellManager))
                    {
                        ColidingOwnCells(actor);
                    }
                    else
                    {
                        if (Calculate.CanEat(gameObject, actor) && Mass >= actor.GetComponent<Cell>().Mass * 1.25f)
                        {
                            Mass += actor.GetComponent<Cell>().Mass;
                            otherCellManager.GetCells().Remove(actor.GetComponent<Cell>());
                            Destroy(actor);
                        }
                    }
                }
                if (actor.CompareTag("Virus"))
                {
                    Virus v = actor.GetComponent<Virus>();
                    if (Mass > v.mass * 1.25f + 25 && Calculate.CanEat(gameObject, actor))
                    {
                        EatVirus(v);
                    }
                }
                if (actor.CompareTag("ThrownMass"))
                {
                    if (Calculate.CanEat(gameObject, actor) && Mass > actor.GetComponent<ThrownMass>().mass)
                    {
                        Mass += actor.GetComponent<ThrownMass>().mass;
                        actor.SetActive(false);
                        Destroy(actor);
                    }
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        GameObject colidedActor = other.gameObject;
        collidedObjects.Remove(colidedActor);
    }
}
