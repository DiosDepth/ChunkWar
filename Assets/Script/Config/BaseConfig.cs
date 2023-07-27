using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseConfig : SerializedScriptableObject
{
    [LabelText("ID")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 300)]
    public int ID;

    [LabelText("Ãû³Æ")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 300)]
    public string UnitName;

    public Sprite Icon;

    [LabelText("Prefab")]
    [LabelWidth(80)]
    [HorizontalGroup("C", 300)]
    [AssetSelector(Paths = "Assets/Prefab/Chunk", DropdownWidth = 600, DropdownHeight = 800)]
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

    [System.Obsolete]
    private void OnEnable()
    {
        _mapPivot = GetMapPivot();
    }

    [HideInInspector]
    [SerializeField]
    public int[] m_FlattendMapLayout;

    [HideInInspector]
    [SerializeField]
    public int m_FlattendMapLayoutRows;

    protected virtual Vector2Int  GetMapPivot()
    {
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

        if (value == 2)
        {
            UnityEditor.EditorGUI.DrawRect(rect.Padding(1), new Color(0.1f, 0.8f, 0.2f));
            UnityEditor.EditorGUI.LabelField(rect.AlignCenterXY(rect.width,rect.height), value.ToString());
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
}
