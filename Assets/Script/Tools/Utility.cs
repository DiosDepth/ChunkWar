using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class Utility 
{

    public static List<UICornerData> CornerData = new List<UICornerData>
    {
        { new UICornerData( UICornerData.CornerType.LeftTop)},
        { new UICornerData ( UICornerData.CornerType.LeftDown)},
        { new UICornerData ( UICornerData.CornerType.RightTop)},
        { new UICornerData ( UICornerData.CornerType.RightDown)},
    };

    public static UICornerData GetCornerData(UICornerData.CornerType type)
    {
        return CornerData.Find(x => x.Type == type);
    }

    /// <summary>
    /// General Clamp
    /// 限制值大小
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static T Clamp<T>(T value, T min, T max) where T : IComparable
    {
        if (min.CompareTo(max) > 0)
        {
            Debug.LogError("[General Clamp] Min > Max, Can not Clamp");
            return value;
        }

        if (value.CompareTo(min) < 0)
            return min;
        else if (value.CompareTo(max) > 0)
            return max;

        return value;
    }

    /// <summary>
    /// Random
    /// </summary>
    public static List<T> GetRandomList<T>(List<T> list, int count) where T : RandomObject
    {
        if (list == null || list.Count <= count || count <= 0)
            return list;

        int totalWeights = 0;
        for (int i = 0; i < list.Count; i++)
        {
            totalWeights += list[i].Weight + 1;
        }

        System.Random ran = new System.Random(GetRandomSeed());
        List<KeyValuePair<int, int>> wlist = new List<KeyValuePair<int, int>>();

        for (int i = 0; i < list.Count; i++)
        {
            int w = (list[i].Weight + 1) + ran.Next(0, totalWeights);
            wlist.Add(new KeyValuePair<int, int>(i, w));
        }

        wlist.Sort(delegate (KeyValuePair<int, int> kvp1, KeyValuePair<int, int> kvp2)
        {
            return kvp2.Value - kvp1.Value;
        });

        List<T> newList = new List<T>();
        for (int i = 0; i < count; i++)
        {
            T en = list[wlist[i].Key];
            newList.Add(en);
        }
        return newList;
    }

    private static int GetRandomSeed()
    {
        byte[] bytes = new byte[4];
        System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
        rng.GetBytes(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }

    public static bool RandomResultWithOne(float min, float max)
    {
        if (min < 0)
            min = 0;
        if (max > 1)
            max = 1;

        float dice = UnityEngine.Random.Range(0, 1f);

        if (dice >= min && dice <= max)
            return true;
        return false;
    }

    /// <summary>
    /// 在中间返回true
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool RandomResult(float min, float max)
    {
        if (min < 0)
            min = 0;
        if (max > 100)
            max = 100;

        float dice = UnityEngine.Random.Range(0, 100f);

        if (dice >= min && dice <= max)
            return true;
        return false;
    }

    public static List<SelectableItemBase> FormatToSelectableItem<T>(this List<T> contentlst) where T : IComparable
    {
        List<SelectableItemBase> result = new List<SelectableItemBase>();
        if (contentlst == null || contentlst.Count <= 0)
            return result;

        for (int i = 0; i < contentlst.Count; i++)
        {
            SelectableItemBase item = new SelectableItemBase();
            item.content = contentlst[i];
            result.Add(item);
        }
        return result;
    }
}

public interface RandomObject
{
    int Weight { get; set; }
}

public class UICornerData
{
    public enum CornerType
    {
        NONE,
        LeftTop,
        LeftDown,
        RightTop,
        RightDown
    }

    public CornerType Type;
    public float PivotX = 0;
    public float PivotY = 0;

    public UICornerData(CornerType type)
    {
        this.Type = type;
        switch (type)
        {
            case CornerType.LeftTop:
                PivotX = 0;
                PivotY = 1;
                break;

            case CornerType.LeftDown:
                PivotX = 0;
                PivotY = 0;
                break;

            case CornerType.RightTop:
                PivotX = 1;
                PivotY = 1;
                break;

            case CornerType.RightDown:
                PivotX = 1;
                PivotY = 0;
                break;

        }
    }
}