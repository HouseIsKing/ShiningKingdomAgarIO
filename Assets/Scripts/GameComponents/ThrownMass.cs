using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools;

public class ThrownMass : MonoBehaviour
{
    public Vector2 vel = new Vector2();
    private float GlidingTime = 0;
    public float maxGlidingTime = 0;
    public Vector2 map;
    bool isTouchingWall;
    public float mass;
	// Use this for initialization
	void Start ()
    {
        isTouchingWall = false;
    }
    private Vector2 CheckWallBounce()
    {
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
    // Update is called once per frame
    void Update ()
    {
        if (GlidingTime<maxGlidingTime)
        {
            if (!isTouchingWall)
            {
                vel.x *= CheckWallBounce().x;
                vel.y *= CheckWallBounce().y;
            }
            Vector2 velocity = Vector2.Lerp(vel, new Vector2(), GlidingTime / maxGlidingTime);
            CheckTouchingWalls(velocity);
            GetComponent<Rigidbody>().velocity = velocity;
            GlidingTime += Time.deltaTime;
        }
        else
        {
            GetComponent<Rigidbody>().velocity = new Vector3();
        }
	}
}
