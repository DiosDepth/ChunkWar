using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameHelper 
{


    public static Vector2Int CoordinateArrayToMap(Vector2Int arraycoord, int mapsize)
    {
        return new Vector2Int(arraycoord.y - mapsize, mapsize - arraycoord.x);
    }

    public static Vector2Int CoordinateMapToArray(Vector2Int shipcoord, int mapsize)
    {
        return new Vector2Int(mapsize - shipcoord.y, mapsize + shipcoord.x);
    }
}
