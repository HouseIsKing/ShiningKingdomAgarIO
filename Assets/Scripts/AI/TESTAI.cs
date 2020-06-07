using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools;

public class TESTAI : MonoBehaviour
{
    private CellManager cellManager = null;
    public ViewManager viewManager = null;
    public TestAgarGamemodeManager agarGamemodeManager = null;
    private BoxCollider myView = null;
    private List<GameObject> foodCells = null;
    private List<Virus> virusCells = null;
    private List<Cell> enemyCells = null;
    private Vector2 BoxSizeVirus = new Vector2(0.1f, 0.1f);
    private Vector2 BoxSizeFoods = new Vector2(0.1f, 0.1f);
    private Vector2 BoxSizeEnemies = new Vector2(0.1f, 0.1f);
    private Vector2 randomLoc = new Vector2();
    private readonly float turnRate = 0.35f / 180;
    private float timer = 0;
    private Cell splitOn;
    private bool distanceSplit;
    private float splitTimer;
    public string AIName;
    private bool splitAttempted = false;
    private void SetupView()
    {
        Vector2 view = new Vector2(Screen.width, Screen.height);
        myView.size = new Vector3(viewManager.GetViewSize().y * 2 * view.x / view.y, viewManager.GetViewSize().y * 2, 500);
    }
    public bool StartupComplete
    {
        get;
        private set;
    }
    public bool HasCells
    {
        get
        {
            return cellManager.GetCells().Count > 0;
        }
    }
    private void Setup()
    {
        StartupComplete = true;
        cellManager = GetComponent<CellManager>();
        viewManager = GetComponent<ViewManager>();
        myView = GetComponent<BoxCollider>();
        if (agarGamemodeManager == null)
        {
            print("Error no gamemode was provided");
            StartupComplete = false;
            return;
        }
        if (myView == null)
        {
            print("Error, view for AI is not found");
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
        cellManager.agarGamemodeManager = agarGamemodeManager;
    }
    void Start ()
    {
        Setup();
        if (!StartupComplete)
        {
            print("Error, AI failed to start and will be disabled");
        }
        else
        {
            cellManager.displayMass = false;
            cellManager.playerName = AIName;
            SetupView();
            foodCells = new List<GameObject>();
            virusCells = new List<Virus>();
            enemyCells = new List<Cell>();
        }
	}
    void Update()
    {
        if (StartupComplete)
        {
            if (cellManager.GetCells().Count != 0)
            {
                transform.localPosition = viewManager.GetViewLocation();
                SetupView();
                cellManager.TargetLocation = UpdateTargetLocation(OrganizeEnemyCells());
                foreach (Cell cell in cellManager.GetCells())
                {
                    Debug.DrawLine(cell.transform.position, cellManager.TargetLocation, Color.white, 0, false);
                }
                if (splitAttempted)
                {
                    splitTimer += Time.deltaTime;
                    if (splitTimer > 1)
                    {
                        splitTimer = 0;
                        splitAttempted = false;
                    }
                }
            }
            else
            {
                ResetAI();
            }
        }
    }
    private Vector2 RandomLocation()
    {
        Vector2 mapSize = agarGamemodeManager.GetMapSize();
        if (Mathf.Pow(transform.localPosition.x - randomLoc.x, 2) < 1 && Mathf.Pow(transform.localPosition.y - randomLoc.y, 2) < 1)
        {
            randomLoc = new Vector2(Random.Range(-mapSize.x, mapSize.x), Random.Range(-mapSize.y, mapSize.y));
        }
        return randomLoc;
    }
    public void ResetAI()
    {
        if (timer >= agarGamemodeManager.RespawnTime)
        {
            transform.localPosition = agarGamemodeManager.FindGoodSpawnPoint();
            cellManager.ResetManager();
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
    private Vector2 UpdateTargetLocation(Vector2 plannedLocation)
    {
        Vector2 loc = new Vector2(transform.localPosition.x, transform.localPosition.y);
        float currentDistance = Vector2.Distance(transform.position, cellManager.TargetLocation);
        float targetDistance = Vector2.Distance(transform.position, plannedLocation);
        float targetAngle = Calculate.CalAngle(loc, plannedLocation);
        float currentAngle = Calculate.CalAngle(loc, cellManager.TargetLocation);
        float changeInAngle = targetAngle - currentAngle;
        if (Mathf.Abs(changeInAngle) > 180)
        {
            if (targetAngle<currentAngle)
            {
                targetAngle += 360;
            }
            else
            {
                currentAngle += 360;
            }
            changeInAngle = targetAngle - currentAngle;
        }
        float finalDistnace;
        float finalAngle;
        float degAngleChanged = Time.deltaTime / turnRate;
        float distanceChanged = (targetDistance - currentDistance) * 0.5f;
        if (changeInAngle > 0)
        { 
            if (degAngleChanged > changeInAngle)
            {
                finalAngle = targetAngle;
                finalDistnace = targetDistance;
            }
            else
            {
                finalAngle = currentAngle + degAngleChanged;
                finalDistnace = currentDistance + distanceChanged;
            }
        }
        else
        {
            if (degAngleChanged > Mathf.Abs(changeInAngle))
            {
                finalAngle = targetAngle;
                finalDistnace = targetDistance;
            }
            else
            {
                finalAngle = currentAngle - degAngleChanged;
                finalDistnace = currentDistance + distanceChanged;
            }
        }
        return Calculate.BreakVector(finalAngle, finalDistnace) + loc;
    }
    private float OrganizeFoods(out GameObject bestFood)
    {
        float maxResult = 0;
        bestFood = null;
        List<GameObject> goodFoods = new List<GameObject>();
        foreach (GameObject food in foodCells)
        {
            if (food != null)
            {
                goodFoods.Add(food);
            }
        }
        foreach (GameObject food in goodFoods)
        {
            float result = 0;
            foreach (Cell cell in cellManager.GetCells())
            {
                float radius = UnitConvereter.ScaleToCm(food.transform.localScale.x);
                float distance = Vector2.Distance(cell.transform.position, food.transform.position);
                distance += radius * 0.05f;
                distance -= UnitConvereter.ScaleToCm(cell.transform.localScale.x) * 0.5f;
                float minDistance = radius * 0.5f;
                result += minDistance / distance;
            }
            if (result > maxResult)
            {
                maxResult = result;
                bestFood = food;
            }
        }
        return maxResult;
    }
    private Vector2 OrganizeEnemyCells()
    {
        SetFoodList();
        SetEnemyCellList();
        GameObject food;
        float maxValueFood = OrganizeFoods(out food);
        float maxValue = 0;
        Cell chosenInput = null;
        foreach (Cell enemy in enemyCells)
        {
            float value = 0;
            foreach (Cell myCell in cellManager.GetCells())
            {
                float input = 0;
                float radiusEnemy = UnitConvereter.ScaleToCm(enemy.transform.localScale.x);
                float radius = UnitConvereter.ScaleToCm(myCell.transform.localScale.x);
                float distance = Vector2.Distance(myCell.transform.position, enemy.transform.position);
                if (splitAttempted && splitOn != null)
                {
                    return splitOn.transform.position; 
                }
                else if (myCell.Mass * 2.5f < enemy.Mass && enemy.cellManager.GetCells().Count == 1)
                {
                    float x = enemy.Speed * Time.deltaTime - myCell.Speed * Time.deltaTime + enemy.Speed * 3 * Time.deltaTime;
                    distance -= x + UnitConvereter.CellUnitToCm(Mathf.Sqrt(100 * enemy.Mass * 0.5f));
                    distance += 0.05f * radius;
                    float val = myCell.Mass / (enemy.Mass / 2);
                    if (distance <= 0)
                    {
                        input += distance*100;
                    }
                    else
                    {
                        float minDistance = radius * 0.5f;
                        input -= minDistance / distance * val;
                    }
                }
                else if (myCell.Mass * 1.25f < enemy.Mass)
                {
                    distance -= radiusEnemy * 0.5f;
                    distance += 0.05f * radius;
                    float n = agarGamemodeManager.GetBasicView().x / 6;
                    float minDistance = 0.55f * radius + n;
                    input -= minDistance / distance;
                }
                else if (myCell.Mass > enemy.Mass * 2.5f && cellManager.GetCells().Count == 1)
                {
                    float x = myCell.Speed * Time.deltaTime - enemy.Speed * Time.deltaTime + myCell.Speed * 3 * Time.deltaTime;
                    distance -= x + UnitConvereter.CellUnitToCm(Mathf.Sqrt(100 * myCell.Mass * 0.5f));
                    distance += 0.05f * radiusEnemy;
                    float val = enemy.Mass / (myCell.Mass / 2);
                    if (distance <= 0)
                    {
                        distanceSplit = true;
                        input -= distance * 100;
                    }
                    else
                    {
                        distanceSplit = false;
                        float minDistance = radiusEnemy * 0.5f;
                        input += minDistance / distance * val;
                    }
                }
                else if (myCell.Mass > enemy.Mass * 1.25f)
                {
                    distance += 0.05f * radiusEnemy;
                    distance -= radius * 0.5f;
                    float minDistance = radiusEnemy * 0.5f;
                    input += minDistance / distance;
                }
                else
                {
                    float minDistance = 0.3f * radius + 0.3f * radiusEnemy;
                    input -= minDistance / Mathf.Pow(distance,2);
                }
                input *= myCell.Mass / cellManager.GetScore();
                value += input;
            }
            if (Mathf.Abs(value) >= Mathf.Abs(maxValue))
            {
                chosenInput = enemy;
                maxValue = value;
            }
        }
        if (Mathf.Abs(maxValue) > maxValueFood)
        {
            if (maxValue<0)
            {
                Vector2 result = CheckForCorners(chosenInput);
                if (cellManager.GetCells().Count == 1 && chosenInput.Mass > cellManager.GetCells()[0].Mass*1.25f)
                {
                    float distance = Vector2.Distance(cellManager.GetCells()[0].transform.position, chosenInput.transform.position);
                    float riskDistance = UnitConvereter.ScaleToCm(chosenInput.transform.localScale.x) + 0.06f*UnitConvereter.ScaleToCm(cellManager.GetCells()[0].transform.localScale.x);
                    if (distance < riskDistance)
                    {
                        cellManager.AllSplit();
                    }
                }
                return result;
            }
            else
            {
                Vector2 location = chosenInput.transform.position;
                if (cellManager.GetCells().Count == 1 && chosenInput.Mass*2.5f < cellManager.GetCells()[0].Mass && SafeSplit(location))
                {
                    cellManager.AllSplit();
                    splitOn = chosenInput;
                    splitAttempted = true;
                }
                return location;
            }
        }
        else
        {
            if (maxValueFood > 0)
            {
                return food.transform.position;
            }
            else
            {
                return RandomLocation();
            }
        }
    }
    private bool SafeSplit(Vector2 finalLocation)
    {
        bool answer = true;
        float currentAngle = Calculate.CalAngle(transform.position, finalLocation);
        float angle = Calculate.CalAngle(transform.position, cellManager.TargetLocation);
        if (angle - 1 < currentAngle && angle + 1 > currentAngle && distanceSplit)
        {
            Vector3 middlePoint = Vector3.Lerp(transform.position, new Vector3(finalLocation.x, finalLocation.y, transform.position.z), 0.5f);
            float x = Vector2.Distance(transform.position, finalLocation)/2;
            Collider[] colliders = Physics.OverlapBox(middlePoint, new Vector3(x, 0.05f, 0.05f), new Quaternion(0, 0, currentAngle,1));
            foreach (Collider collider in colliders)
            {
                Cell cell = collider.GetComponent<Cell>();
                if (cell != null)
                {
                    if (!cell.cellManager.Equals(cellManager))
                    {
                        answer = false;
                    }
                }
            }
        }
        else
        {
            answer = false;
        }
        return answer;
    }
    private Vector2 CheckForCorners(Cell enemy)
    {
        float angle = Calculate.CalAngle(enemy.transform.position, transform.position);
        Vector2 pos = transform.position;
        float viewX = myView.size.x;
        float viewY = myView.size.y;
        viewX /= 2;
        viewY /= 2;
        Vector2 mapBorders = agarGamemodeManager.GetMapSize();
        if (pos.x + viewX > mapBorders.x)
        {
            if (pos.y + viewY > mapBorders.y)
            {
                if (angle > 0 && angle <= 90)
                {
                    float angleEnemy = Calculate.CalAngle(enemy.transform.position, mapBorders);
                    if (angleEnemy > 0 && angleEnemy <= 45)
                    {
                        return new Vector2(mapBorders.x, mapBorders.y - viewY - 1);
                    }
                    else
                    {
                        return new Vector2(mapBorders.x - viewX - 1, mapBorders.y);
                    }
                }
                else
                {
                    return EscapeWall(mapBorders, angle);
                }
            }
            else if (pos.y-viewY < -mapBorders.y)
            {
                if (angle > 270 && angle <= 360)
                {
                    float angleEnemy = Calculate.CalAngle(enemy.transform.position, new Vector2(mapBorders.x,-mapBorders.y));
                    if (angleEnemy > 270 && angleEnemy <= 315)
                    {
                        return new Vector2(mapBorders.x - viewX - 1, -mapBorders.y);
                    }
                    else
                    {
                        return new Vector2(mapBorders.x, -mapBorders.y + viewY + 1);
                    }
                }
                else
                {
                    return EscapeWall(mapBorders,angle);
                }
            }
            else
            {
                return EscapeWall(mapBorders, angle);
            }
        }
        else
        {
            if (-mapBorders.x>pos.x-viewX)
            {
                if (pos.y + viewY > mapBorders.y)
                {
                    if (angle > 90 && angle <= 180)
                    {
                        float angleEnemy = Calculate.CalAngle(enemy.transform.position, new Vector2(-mapBorders.x,mapBorders.y));
                        if (angleEnemy > 90 && angleEnemy <= 135)
                        {
                            return new Vector2(-mapBorders.x + viewX + 1, mapBorders.y);
                        }
                        else
                        {
                            return new Vector2(-mapBorders.x, mapBorders.y - viewY - 1);
                        }
                    }
                    else
                    {
                        return EscapeWall(mapBorders, angle);
                    }
                }
                else if (pos.y - viewY < -mapBorders.y)
                {
                    if (angle > 180 && angle <= 270)
                    {
                        float angleEnemy = Calculate.CalAngle(enemy.transform.position, new Vector2(-mapBorders.x, -mapBorders.y)); ;
                        if (angleEnemy > 180 && angleEnemy <= 225)
                        {
                            return new Vector2(-mapBorders.x, -mapBorders.y + viewY + 1);
                        }
                        else
                        {
                            return new Vector2(-mapBorders.x + viewX + 1, -mapBorders.y);
                        }
                    }
                    else
                    {
                        return EscapeWall(mapBorders, angle);
                    }
                }
                else
                {
                    return EscapeWall(mapBorders, angle);
                }
            }
            else
            {
                if (pos.y + viewY > mapBorders.y || pos.y - viewY < -mapBorders.y)
                {
                    return EscapeWall(mapBorders,angle);
                }
                else
                {
                    return (Vector2)transform.position + Calculate.BreakVector(angle, 50);
                }
            }
        }
    }
    private Vector2 EscapeWall(Vector2 mapBorders, float angle)
    {
        if (angle >= 0 && angle < 90)
        {
            return mapBorders;
        }
        if (angle >= 90 && angle < 180)
        {
            return new Vector2(-mapBorders.x, mapBorders.y);
        }
        if (angle >= 180 && angle <270)
        {
            return new Vector2(-mapBorders.x, -mapBorders.y);
        }
        else
        {
            return new Vector2(mapBorders.x, -mapBorders.y);
        }
    }
    private void SetEnemyCellList()
    {
        bool ready = false;
        Vector2 pos = transform.position;
        Vector2 viewSize = viewManager.GetViewSize();
        while (!ready)
        {
            List<Cell> result = new List<Cell>();
            List<CellManager> managers = new List<CellManager>();
            Collider[] colliders = Physics.OverlapBox(pos, new Vector3(BoxSizeEnemies.x, BoxSizeEnemies.y, 100));
            foreach (Collider coll in colliders)
            {
                if (coll.gameObject.CompareTag("Cell") && !coll.gameObject.GetComponent<Cell>().cellManager.Equals(cellManager))
                {
                    CellManager manager = coll.gameObject.GetComponent<Cell>().cellManager;
                    result.Add(coll.gameObject.GetComponent<Cell>());
                    if (!managers.Contains(manager))
                    {
                        managers.Add(manager);
                    }
                }
            }
            if (managers.Count >= 3 && managers.Count <= 5)
            {
                ready = true;
                enemyCells = result;
            }
            else
            {
                if (managers.Count < 3)
                {
                    if (BoxSizeEnemies.x > viewSize.x && BoxSizeEnemies.y > viewSize.y)
                    {
                        ready = true;
                        enemyCells = result;
                    }
                    else
                    {
                        if (BoxSizeEnemies.x <= viewSize.x)
                        {
                            BoxSizeEnemies.x += 0.01f;
                        }
                        if (BoxSizeEnemies.y <= viewSize.y)
                        {
                            BoxSizeEnemies.y += 0.01f;
                        }
                    }
                }
                else if (managers.Count > 5)
                {
                    if (BoxSizeEnemies.x > 0.01f)
                    {
                        BoxSizeEnemies.x -= 0.01f;
                    }
                    if (BoxSizeEnemies.y > 0.01f)
                    {
                        BoxSizeEnemies.y -= 0.01f;
                    }
                    if (BoxSizeEnemies.x <= 0.01f && BoxSizeEnemies.y <= 0.01f)
                    {
                        ready = true;
                        enemyCells = result;
                    }
                }
            }
        }
    }
    private void SetFoodList()
    {  
        bool ready = false;
        Vector2 pos = transform.position;
        Vector2 viewSize = viewManager.GetViewSize();
        while (!ready)
        {
            List<GameObject> result = new List<GameObject>();
            Collider[] colliders = Physics.OverlapBox(pos, new Vector3(BoxSizeFoods.x, BoxSizeFoods.y, 100));
            foreach (Collider coll in colliders)
            {
                if (coll.gameObject.CompareTag("Food"))
                {
                    result.Add(coll.gameObject);
                }
            }
            if (result.Count > 10 && result.Count < 50)
            {
                ready = true;
                foodCells = result;
            }
            else
            {
                if (result.Count < 11)
                {
                    if (BoxSizeFoods.x > viewSize.x && BoxSizeFoods.y > viewSize.y)
                    {
                        ready = true;
                        foodCells = result;
                    }
                    else
                    {
                        if (BoxSizeFoods.x <= viewSize.x)
                        {
                            BoxSizeFoods.x += 0.01f;
                        }
                        if (BoxSizeFoods.y <= viewSize.y)
                        {
                            BoxSizeFoods.y += 0.01f;
                        }
                    }
                }
                else if (result.Count > 49)
                {
                    if (BoxSizeFoods.x > 0.01f)
                    {
                        BoxSizeFoods.x -= 0.01f;
                    }
                    if (BoxSizeFoods.y > 0.01f)
                    {
                        BoxSizeFoods.y -= 0.01f;
                    }
                    if (BoxSizeFoods.x <= 0.01f && BoxSizeFoods.y <= 0.01f)
                    {
                        ready = true;
                        foodCells = result;
                    }
                }
            }
        }
    }
}
