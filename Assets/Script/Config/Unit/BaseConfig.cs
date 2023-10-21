using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BaseConfig : SerializedScriptableObject, IComparable<BaseConfig>
{
    [LabelText("ID")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 150)]
    public int ID;

    [LabelText("基础配置")]
    [LabelWidth(80)]
    [HorizontalGroup("C", 800)]
    public GeneralItemConfig GeneralConfig = new GeneralItemConfig();

    [LabelText("Prefab")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 300)]
    public GameObject Prefab;

    [TableMatrix(ResizableColumns = false, SquareCells = true, DrawElementMethod = "DrawTable")]
    [FoldoutGroup("网格配置")]
    [HorizontalGroup("网格配置/A", 600)]
    public int[,] Map;

    [LabelText("格子数量统计")]
    [FoldoutGroup("网格配置")]
    [ShowInInspector]
    [ReadOnly]
    private int SlotCount;

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
        return Vector2Int.zero;
    }

    public virtual Vector2 GetMapSize()
    {
        return Vector2.zero;
    }

#if UNITY_EDITOR

    protected virtual int DrawTable(Rect rect, int value)
    {
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            value += 1;
            GUI.changed = true;
            Event.current.Use();
            if (value == 4)
            {
                value = 0;
            }
            CalculateSlotCount();
        }

        if (value == 1)
        {
            EditorGUI.DrawRect(rect.Padding(1), new Color(0.1f, 0.8f, 0.2f));
            EditorGUI.LabelField(rect.AlignCenterXY(rect.width, rect.height), value.ToString());
        }
        else if (value == 2)
        {
            EditorGUI.DrawRect(rect.Padding(1), new Color(0.5f, 0f, 0f, 1f));
            EditorGUI.LabelField(rect.AlignCenterXY(rect.width, rect.height), value.ToString());
        }
        else if (value == 0)
        {
            EditorGUI.DrawRect(rect.Padding(1), new Color(0f, 0f, 0f, 1f));
            EditorGUI.LabelField(rect.AlignCenterXY(rect.width, rect.height), value.ToString());
        }
        else if(value == 3)
        {
            EditorGUI.DrawRect(rect.Padding(1), Color.blue);
            EditorGUI.LabelField(rect.AlignCenterXY(rect.width, rect.height), value.ToString());
        }
        return value;
    }

    [OnInspectorInit]
    protected virtual void InitData()
    {
        CalculateSlotCount();
    }

    [OnInspectorDispose]
    private void Save()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

#endif

    protected void CalculateSlotCount()
    {
        int totalCount = 0;
        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                if (Map[x, y] != 0)
                {
                    totalCount++;
                }
            }
        }
        SlotCount = totalCount;
    }

    public int CompareTo(BaseConfig other)
    {
        if (this.GeneralConfig.SortingOrder > other.GeneralConfig.SortingOrder)
            return 1;

        return -1;
    }

    public virtual string GetPrefabName()
    {
        if(Prefab == null)
        {
            return string.Empty;
        }
        return Prefab.name;
    }
}


