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
