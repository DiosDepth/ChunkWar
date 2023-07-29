using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class GameHelper 
{


    public static Vector2Int CoordinateArrayToMap(Vector2Int arraycoord, int mapsize)
    {
        return new Vector2Int(arraycoord.x - mapsize, mapsize - arraycoord.y);
    }

    public static Vector2Int CoordinateMapToArray(Vector2Int shipcoord, int mapsize)
    {
        return new Vector2Int(mapsize + shipcoord.x, mapsize - shipcoord.y);
    }


    public static Vector2Int GetReletiveCoordFromWorldPos(Transform trs, Vector2 worldpos)
    {
        Vector2 reletivePos = worldpos - trs.position.ToVector2();
        Vector2Int roundPos = reletivePos.Round();

        return roundPos;
    }

    public static Vector2 GetWorldPosFromReletiveCoord(Transform trs, Vector2Int coord)
    {
        Vector3 tempcoord = trs.TransformPoint(coord.ToVector3());
        return tempcoord.ToVector2();
    }

    public static List<uint> GetRogueShipPlugItems()
    {
        var allItems = RogueManager.Instance.GetAllCurrentShipPlugs;
        List<uint> goods = new List<uint>();
        for(int i = 0; i < allItems.Count; i++)
        {
            if (!goods.Contains((uint)allItems[i].GoodsID))
            {
                goods.Add((uint)allItems[i].GoodsID);
            }
        }

        return goods;
    }
}
