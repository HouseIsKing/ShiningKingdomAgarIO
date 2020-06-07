using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools;

public class Virus : MonoBehaviour
{
    Dictionary<GameObject, float> heights = new Dictionary<GameObject, float>();
    public float mass = 100;
    private int timesGrown = 0;
    public Vector2 splitSpeed;
    private float timeToStopSplitting;
    private bool isGliding = false;
    private bool isTouchingWall = false;
    private float angle = 0;
    public TestAgarGamemodeManager agarGamemode;
	// Use this for initialization
	void Start ()
    {
		
	}
    private void SplitVirus()
    {
        GameObject v2 = Instantiate(agarGamemode.virus, transform.localPosition, transform.localRotation);
        mass = agarGamemode.GetVirusMass();
        Virus newVirus = v2.GetComponent<Virus>();
        newVirus.UpdateScale();
        newVirus.mass = mass;
        newVirus.splitSpeed = Calculate.BreakVector(angle + 180, Time.deltaTime * 1500 * Mathf.Pow(0.9999f, mass));
        newVirus.isGliding = true;
        newVirus.agarGamemode = agarGamemode;
        UpdateScale();
        agarGamemode.virusNum++;
        timesGrown = 0;
    }
    private void Move()
    {
        if (isGliding)
        {
            CheckTouchingWalls(new Vector2());
            if (!isTouchingWall)
            {
                splitSpeed.x *= CheckWallBounce().x;
                splitSpeed.y *= CheckWallBounce().y;
            }
            if (timeToStopSplitting>= agarGamemode.splitTime)
            {
                isGliding = false;
            }
            else
            {
                Vector2 speed = Vector2.Lerp(splitSpeed, new Vector2(), timeToStopSplitting / agarGamemode.splitTime);
                GetComponent<Rigidbody>().velocity = speed;
                timeToStopSplitting += Time.deltaTime;
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateLocation();
        Move();
	}
    private void UpdateScale()
    {
        float x = UnitConvereter.CellUnitToScale(Mathf.Sqrt(mass * 100)) / 2;
        transform.localScale = new Vector3(x, x, x);
    }
    private void OnTriggerStay(Collider other)
    {
        GameObject otherActor = other.gameObject;
        if (otherActor.tag.Equals("Cell"))
        {
            if (mass > otherActor.GetComponent<Cell>().Mass)
            {
                if (heights.ContainsKey(otherActor))
                {
                    heights.Remove(otherActor);
                }
                heights.Add(otherActor, -otherActor.transform.localScale.z*1.005f);
            }
            else
            {
                if (heights.ContainsKey(otherActor))
                {
                    heights.Remove(otherActor);
                }
            }
        }
        if (otherActor.tag.Equals("Food"))
        {
            if (heights.ContainsKey(otherActor))
            {
                heights.Remove(otherActor);
            }
            heights.Add(otherActor, -otherActor.transform.localScale.z * 1.1f);
        }
        if (otherActor.tag.Equals("ThrownMass"))
        {
            if (heights.ContainsKey(otherActor))
            {
                heights.Remove(otherActor);
            }
            heights.Add(otherActor, -otherActor.transform.localScale.z * 1.1f);
            if (Calculate.CanEat(gameObject,otherActor,2))
            {
                mass += otherActor.GetComponent<ThrownMass>().mass;
                timesGrown++;
                angle = Calculate.CalAngle(transform.localPosition, otherActor.transform.localPosition);
                Destroy(otherActor);
                UpdateScale();
                if (timesGrown == agarGamemode.virusTimesGrow)
                {
                    SplitVirus();
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        GameObject otherActor = other.gameObject;
        if (otherActor.tag.Equals("Cell"))
        {
            if (heights.ContainsKey(otherActor))
            {
                heights.Remove(otherActor);
            }
        }
    }
    private void UpdateLocation()
    {
        Dictionary<GameObject, float> helper = new Dictionary<GameObject, float>(heights);
        foreach (GameObject a in helper.Keys)
        {
            if (a == null)
            {
                heights.Remove(a);
            }
        }
        float maxz = 0;
        foreach (float z in heights.Values)
        {
            if (z<maxz)
            {
                maxz = z;
            }
        }
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, maxz);
    }
    private Vector2 CheckWallBounce()
    {
        Vector2 map = agarGamemode.GetMapSize();
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
    private void CheckTouchingWalls(Vector2 a)
    {
        Vector2 map = agarGamemode.GetMapSize();
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
}
