using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathExtensionTools 
{

    public static bool IsEven(this float self)
    {
        return (self % 2 == 0);
    }

    public static bool IsEven(this int self)
    {
        return (self % 2 == 0);
    }

    public static bool CalculateCompareType(float value, float target, CompareType type)
    {
        if (type == CompareType.Equla)
        {
            return value == target;
        }
        else if (type == CompareType.Greater)
        {
            return value > target;
        }
        else if (type == CompareType.GreaterEqula)
        {
            return value >= target;
        }
        else if (type == CompareType.Less)
        {
            return value < target;
        }
        else if (type == CompareType.LessEqula)
        {
            return value <= target;
        }
        else if (type == CompareType.NonEqula)
        {
            return value != target;
        }
        return false;
    }

    public static bool CalculateCompareType(int value, int target, CompareType type)
    {
        if(type == CompareType.Equla)
        {
            return value == target;
        }
        else if (type == CompareType.Greater)
        {
            return value > target;
        }
        else if (type == CompareType.GreaterEqula)
        {
            return value >= target;
        }
        else if (type == CompareType.Less)
        {
            return value < target;
        }
        else if (type == CompareType.LessEqula)
        {
            return value <= target;
        }
        else if (type == CompareType.NonEqula)
        {
            return value != target;
        }
        return false;
    }

    public static Vector2Int Round(this Vector2 v2)
    {
        return new Vector2Int(Mathf.RoundToInt(v2.x), Mathf.RoundToInt(v2.y));
    }

    public static float ToAngleZ(this Vector2 dir)
    {
        Vector2 normaldir = dir.normalized;
        return Quaternion.LookRotation(new Vector3(0, 0, 1), normaldir).eulerAngles.z;
    }

    /// <summary>
    ///  reference dirction with angle
    /// </summary>
    /// <param name="m_refdir"></param>
    /// <param name="m_rangeangle"></param>
    /// <returns></returns>

    public static Vector2 GetRandomDirection(Vector2 m_refdir, float m_rangeangle)
    {

        //UnityEngine.Random.InitState(Mathf.RoundToInt(System.DateTime.Now.Ticks));
        float randomangle = UnityEngine.Random.Range(-m_rangeangle, m_rangeangle);

         Vector3 dir = Quaternion.Euler(0, 0, randomangle) * m_refdir;

        return dir;
    }

    /// <summary>
    /// 获取每一帧的转向角度
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="rotspeed"></param>
    /// <returns></returns>
    public static Quaternion CalculateRotation(Vector3 from, Vector3 to, float rotspeed)
    {
        Quaternion fromrot = Quaternion.LookRotation(new Vector3(0, 0, 1), from);
        Quaternion torot = Quaternion.LookRotation(new Vector3(0, 0, 1), to);
        return Quaternion.RotateTowards(fromrot, torot, rotspeed);
    }


    public static Quaternion GetRotationFromDirection(Vector2 direction)
    {
        return Quaternion.LookRotation(new Vector3(0, 0, 1), direction);
    }
    /// <summary>
    /// 获取当前ship位置的圆形区间范围随机点，
    /// </summary>
    /// <param name="innerrange"></param>
    /// <param name="outrange"></param>
    /// <returns></returns>
    public static Vector2 GetRadomPosFromOutRange(float innerrange, float outrange, Vector2 referencepos)
    {
        float rad = UnityEngine.Random.Range(0f, 2f) * Mathf.PI;
        var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        float distance = UnityEngine.Random.Range(innerrange, outrange);
        Vector2 pos = dir * distance + referencepos;
        return pos;
    }

    public static float Lager(this Vector2 self)
    {
        if(self.x > self.y)
        {
            return self.x;
        }
        else
        {
            return self.y;
        }
    }
    public static Vector3 DirectionToXY(this Vector3 self, Vector3 target)
    {
        Vector3 a = new Vector3(self.x, self.y, 0);
        Vector3 b = new Vector3(target.x, target.y, 0);

        return (b - a).normalized;
    }

    public static Vector2 DirectionToXY(this Vector2 self, Vector2 target)
    {
        Vector2 a = new Vector2(self.x, self.y);
        Vector2 b = new Vector2(target.x, target.y);

        return (b - a).normalized;
    }

    public static float DistanceXY(this Vector3 self, Vector3 target)
    {
        Vector3 a = new Vector3(self.x, self.y, 0);
        Vector3 b = new Vector3(target.x, target.y, 0);

        return Vector3.Distance(a, b);
    }

    public static float DistanceXY(this Vector2 self, Vector2 target)
    {
        Vector2 a = new Vector2(self.x, self.y);
        Vector2 b = new Vector2(target.x, target.y);
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
