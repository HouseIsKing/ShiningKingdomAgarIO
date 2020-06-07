using UnityEngine;

namespace Tools
{
    public static class Calculate
    {
        //Caluclates the angle between the line created via point (0,0) and point A, and the line Y=0
        public static float CalAngle(Vector2 a)
        {
            float result = 0;
            if (a.x < 0)
            {
                result += 180;
            }
            result += Mathf.Atan(a.y / a.x) * 180 / Mathf.PI;
            if (result < 0)
            {
                result += 360;
            }
            return result;
        }
        //Caculates the angle between 2 given points.
        public static float CalAngle(Vector2 a, Vector2 b)
        {
            return CalAngle(new Vector2(b.x - a.x, b.y - a.y));
        }
        public static Vector2 BreakVector(float angle, float c)
        {
            return new Vector2(c * Mathf.Cos(angle / 180 * Mathf.PI), c * Mathf.Sin(angle / 180 * Mathf.PI));
        }
        public static bool CanEat(GameObject a, GameObject target, float scaleFactor = 1)
        {
            float angle = CalAngle(a.transform.localPosition, target.transform.localPosition);
            Vector2 aScale = BreakVector(angle, UnitConvereter.ScaleToCm(a.transform.localScale.x * scaleFactor));
            Vector2 targetScale = BreakVector(angle,0.2f*UnitConvereter.ScaleToCm(target.transform.localScale.x));//In CM units
            Vector3 targetLocation = target.transform.localPosition;
            Vector3 aLocation = a.transform.localPosition;
            Vector2 targetFinalLoc = new Vector2(targetLocation.x + targetScale.x, targetLocation.y + targetScale.y);
            Vector2 aFinalLoc = new Vector2(aLocation.x + aScale.x, aLocation.y + aScale.y);
            return IsInPosition(aFinalLoc, targetFinalLoc, angle); 
        }
        public static bool IsInPosition(Vector2 a, Vector2 b,float angle)
        {
            if (angle >= 0 && angle < 90) 
            {
                if (a.x > b.x && a.y >= b.y)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (angle >= 90 && angle < 180) 
            {
                if (a.x <= b.x && a.y > b.y)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (angle >= 180 && angle < 270)
            {
                if (a.x < b.x && a.y <= b.y)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (a.x >= b.x && a.y < b.y)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

}
