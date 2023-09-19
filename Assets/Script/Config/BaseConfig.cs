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

    [LabelText("��������")]
    [LabelWidth(80)]
    [HorizontalGroup("C", 800)]
    public GeneralItemConfig GeneralConfig = new GeneralItemConfig();

    [LabelText("Prefab")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 300)]
    public GameObject Prefab;

    [TableMatrix(ResizableColumns = false, SquareCells = true, DrawElementMethod = "DrawTable")]
    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 600)]
    public int[,] Map;

    [LabelText("��������ͳ��")]
    [FoldoutGroup("��������")]
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
    protected virtual int DrawTable(Rect rect, int value)
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
            CalculateSlotCount();
        }

        if (value == 1)
        {
            UnityEditor.EditorGUI.DrawRect(rect.Padding(1), new Color(0.1f, 0.8f, 0.2f));
            UnityEditor.EditorGUI.LabelField(rect.AlignCenterXY(rect.width, rect.height), value.ToString());
        }
        else if (value == 2)
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


    [OnInspectorInit]
    protected virtual void InitData()
    {
        CalculateSlotCount();
    }

    public int CompareTo(BaseConfig other)
    {
        if (this.GeneralConfig.SortingOrder > other.GeneralConfig.SortingOrder)
            return 1;

        return -1;
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


