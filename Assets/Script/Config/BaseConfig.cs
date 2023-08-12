using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BaseConfig : SerializedScriptableObject
{
    [LabelText("ID")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 150)]
    public int ID;

    [LabelText("ª˘¥°≈‰÷√")]
    [LabelWidth(80)]
    [HorizontalGroup("C", 800)]
    public GeneralItemConfig GeneralConfig = new GeneralItemConfig();

    [LabelText("Prefab")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 300)]
    public GameObject Prefab;

    [TableMatrix(ResizableColumns = false, SquareCells = true, DrawElementMethod = "DrawTable")]
    [HorizontalGroup("A", 600)]
    public int[,] Map;

    public Vector2Int MapPivot
    {
        get
        {
            return _mapPivot;
        }

    }
    protected Vector2Int _mapPivot = new Vector2Int();

    public Vector2 MapSize
    {
        get
        {
            return _mapSize;
        }
    }
    protected Vector2 _mapSize = new Vector2();

    [System.Obsolete]
    protected virtual void OnEnable()
    {
        _mapPivot = GetMapPivot();
    }

    [HideInInspector]
    [SerializeField]
    public int[] m_FlattendMapLayout;

    [HideInInspector]
    [SerializeField]
    public int m_FlattendMapLayoutRows;

    protected virtual Vector2Int GetMapPivot()
    {

        if (Map == null)
            return Vector2Int.zero;
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


    public virtual Vector2 GetMapSize()
    {
        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        Vector2Int tempvalue;
        for (int row = 0; row < Map.GetLength(0); row++)
        {
            for (int colume = 0; colume < Map.GetLength(1); colume++)
            {
                if (Map[row, colume] != 0)
                {
                    tempvalue = GameHelper.CoordinateArrayToMap(new Vector2Int(row, colume), GameGlobalConfig.ShipMapSize);
                    if (tempvalue.x > max.x)
                    {
                        max.x = tempvalue.x;
                    }
                    else if (tempvalue.x < min.x)
                    {
                        min.x = tempvalue.x;
                    }
                    if (tempvalue.y > max.y)
                    {
                        max.y = tempvalue.y;
                    }
                    else if (tempvalue.y < min.y)
                    {
                        min.y = tempvalue.y;
                    }
                }
            }
        }
        return max - min + Vector2.one;
    }
    protected static int DrawTable(Rect rect, int value)
    {
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            value += 1;
            GUI.changed = true;
            Event.current.Use();
            if (value == 3)
            {
                value = 0;
            }
        }

        if (value == 2)
        {
            UnityEditor.EditorGUI.DrawRect(rect.Padding(1), new Color(0.1f, 0.8f, 0.2f));
            UnityEditor.EditorGUI.LabelField(rect.AlignCenterXY(rect.width, rect.height), value.ToString());
        }
        else if (value == 1)
        {
            UnityEditor.EditorGUI.DrawRect(rect.Padding(1), new Color(0.5f, 0f, 0f, 1f));
            UnityEditor.EditorGUI.LabelField(rect.AlignCenterXY(rect.width, rect.height), value.ToString());
        }
        else if (value == 0)
        {
            UnityEditor.EditorGUI.DrawRect(rect.Padding(1), new Color(0f, 0f, 0f, 1f));
            UnityEditor.EditorGUI.LabelField(rect.AlignCenterXY(rect.width, rect.height), value.ToString());
        }
        return value;
    }


    [OnInspectorInit]
    protected virtual void InitData()
    {

    }

#if UNITY_EDITOR

    [OnInspectorDispose]
    private void Save()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif

}


