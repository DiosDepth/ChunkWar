using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathExtensionTools 
{


    public static Vector2Int Round(this Vector2 v2)
    {
        return new Vector2Int(Mathf.RoundToInt(v2.x), Mathf.RoundToInt(v2.y));
    }

    public static Vector3 DirectionToXY(this Vector3 self, Vector3 target)
    {
        Vector3 a = new Vector3(self.x, self.y, 0);
        Vector3 b = new Vector3(target.x, target.y, 0);

        return (b - a).normalized;
    }

    public static float DistanceXY(this Vector3 self, Vector3 target)
    {
        Vector3 a = new Vector3(self.x, self.y, 0);
        Vector3 b = new Vector3(target.x, target.y, 0);

        return Vector3.Distance(a, b);
    }

    public static float SqrDistanceXY(this Vector3 self, Vector3 target)
    {
        Vector3 a = new Vector3(self.x, self.y, 0);
        Vector3 b = new Vector3(target.x, target.y, 0);

        return (b - a).sqrMagnitude;
    }

    //---2DVectorOperation---
    public static bool EqualXY(this Vector3 self, Vector3 target, float threshold = 0.1f)
    {
        Vector3 a = new Vector3(self.x, self.y, 0);
        Vector3 b = new Vector3(target.x, target.y, 0);

        if ((b - a).sqrMagnitude <= Mathf.Pow(threshold, 2))
        {
            return true;
        }
        return false;
    }


    public static bool IsInRange(this float value,float min, float max)
    {
        if(value>= min && value <= max)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool IsInRange(this int value, int min, int max)
    {
        if (value >= min && value <= max)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static Vector2Int[] AddToAll (this Vector2Int[] array, Vector2Int value) 
    {
        Vector2Int[] newarray = new Vector2Int[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            newarray[i] = array[i] + value;
        }
        return newarray;
    }

}
