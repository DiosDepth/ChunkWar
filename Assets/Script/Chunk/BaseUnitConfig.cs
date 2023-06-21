using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnitConfig : ScriptableObject, ISerializationCallbackReceiver
{
    public int ID;
    public string UnitName;
    public UnitType Type;
    public float HP = 100;
    public Sprite Icon;
    public GameObject Prefab;



    public int[,] Map = new int[GameGlobalConfig.BuildingMaxSize, GameGlobalConfig.BuildingMaxSize];

    public Vector2Int MapPivot
    {
        get
        {
            return _mapPivot;
        }

    }
    private Vector2Int _mapPivot = new Vector2Int();


    [HideInInspector]
    [SerializeField]
    public int[] m_FlattendMapLayout;

    [HideInInspector]
    [SerializeField]
    public int m_FlattendMapLayoutRows;

    public void OnEnable()
    {
        _mapPivot = GetMapPivot();
    }

    public Vector2Int GetMapPivot()
    {
        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                if (Map[x, y] == 1)
                {
                    return GameHelper.CoordinateArrayToMap(new Vector2Int(x, y), GameGlobalConfig.BuildingMapSize);
                }
            }
        }
        return Vector2Int.zero;
    }
    public Vector2Int[] GetReletiveCoordByCenter(Vector2Int centercoord)
    {
        Vector2Int coord;
        List<Vector2Int> reletiveCoord = new List<Vector2Int>();
        reletiveCoord.Add(centercoord);
        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                if (Map[x, y] == 2)
                {
                    coord = GameHelper.CoordinateArrayToMap(new Vector2Int(x, y), GameGlobalConfig.BuildingMapSize);
                    coord = coord - MapPivot;
                    coord += centercoord;
                    reletiveCoord.Add(coord);
                }
            }
        }
        return reletiveCoord.ToArray();
    }

    public Vector2Int[] GetReletiveCoord()
    {
        Vector2Int coord;
        List<Vector2Int> reletiveCoord = new List<Vector2Int>();
        reletiveCoord.Add(MapPivot);
        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                if (Map[x, y] == 2)
                {
                    coord = GameHelper.CoordinateArrayToMap(new Vector2Int(x, y), GameGlobalConfig.BuildingMapSize);
                    coord = coord - MapPivot;

                    reletiveCoord.Add(coord);
                }
            }
        }
        return reletiveCoord.ToArray();
    }


    public void OnBeforeSerialize()
    {
        int c1 = Map.GetLength(0);
        int c2 = Map.GetLength(1);
        int count = c1 * c2;
        m_FlattendMapLayout = new int[count];
        m_FlattendMapLayoutRows = c1;
        for (int i = 0; i < count; i++)
        {
            m_FlattendMapLayout[i] = Map[i / c1, i % c1];
        }
    }
    public void OnAfterDeserialize()
    {
        int count = m_FlattendMapLayout.Length;
        int c1 = m_FlattendMapLayoutRows;
        int c2 = count / c1;
        Map = new int[c1, c2];
        for (int i = 0; i < count; i++)
        {
            Map[i / c1, i % c1] = m_FlattendMapLayout[i];
        }
    }


}
