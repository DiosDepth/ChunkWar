using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseConfig : SerializedScriptableObject, ISerializationCallbackReceiver
{
    [LabelText("ID")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 300)]
    public int ID;

    [LabelText("名称")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 300)]
    public string UnitName;

    [FoldoutGroup("基础属性")]
    [LabelText("血量")]
    [LabelWidth(80)]
    public float HP = 100;


    public Sprite Icon;
    public GameObject Prefab;

    [TableMatrix(ResizableColumns = false,SquareCells = true, DrawElementMethod = "DrawTable")]
    [HorizontalGroup("A", 600)]
    public int[,] Map = new int[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];

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


    public virtual void OnEnable()
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
                    return GameHelper.CoordinateArrayToMap(new Vector2Int(x, y), GameGlobalConfig.ShipMapSize);
                }
            }
        }
        return Vector2Int.zero;
    }

    private static int DrawTable(Rect rect, int value)
    {
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            value += 1;
            GUI.changed = true;
            Event.current.Use();
            if(value == 3)
            {
                value = 0;
            }
        }

        if (value == 1)
        {
            UnityEditor.EditorGUI.DrawRect(rect.Padding(1), new Color(0.1f, 0.8f, 0.2f));
        }
        else if (value == 2)
        {
            UnityEditor.EditorGUI.DrawRect(rect.Padding(1), new Color(0.5f, 0f, 0f, 1f));
        }
        else if (value == 0)
        {
            UnityEditor.EditorGUI.DrawRect(rect.Padding(1), new Color(0f, 0f, 0f, 1f));
        }
        return value;
    }

    //public void OnBeforeSerialize()
    //{
    //    int c1 = Map.GetLength(0);
    //    int c2 = Map.GetLength(1);
    //    int count = c1 * c2;
    //    m_FlattendMapLayout = new int[count];
    //    m_FlattendMapLayoutRows = c1;
    //    for (int i = 0; i < count; i++)
    //    {
    //        m_FlattendMapLayout[i] = Map[i / c1, i % c1];
    //    }
    //}
    //public void OnAfterDeserialize()
    //{
    //    int count = m_FlattendMapLayout.Length;
    //    int c1 = m_FlattendMapLayoutRows;
    //    int c2 = count / c1;
    //    Map = new int[c1, c2];
    //    for (int i = 0; i < count; i++)
    //    {
    //        Map[i / c1, i % c1] = m_FlattendMapLayout[i];
    //    }
    //}
}
