using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommonExtensionTools
{
    public static Vector2 ToVector2(this Vector3 v3)
    {
        return new Vector2(v3.x, v3.y);
    }

    public static Vector3 ToVector3(this Vector2 v2)
    {
        return new Vector3(v2.x, v2.y, 0);
    }

    public static Vector3 ToVector3(this Vector2Int v2)
    {
        return new Vector3(v2.x, v2.y, 0);
    }

    public static void SetTransform(this Transform trs, Transform newtrs, bool setscale = false)
    {
        trs.position = newtrs.position;
        trs.rotation = newtrs.rotation;
        if(setscale)
        {
            trs.localScale = newtrs.localScale;
        }
    }

}
