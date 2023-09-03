using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum ItemTag : ulong
{
    Shield = 1<<1,
    Weapon = 1<<2,
}

[System.Serializable]
public class UnitPropertyModifyFrom
{
    [HorizontalGroup("CC", 300)]
    [LabelText("属性Key")]
    [LabelWidth(80)]
    public PropertyModifyKey PropertyKey;
    [HorizontalGroup("CC", 300)]
    [LabelText("比例")]
    [LabelWidth(80)]
    public float Ratio;
}


[CreateAssetMenu(fileName = "Configs_Unit_", menuName = "Configs/Unit/UnitConfig")]
public class BaseUnitConfig : BaseConfig
{
    [LabelText("类型")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 200)]
    public UnitType unitType;

    [LabelText("可旋转")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 150)]
    public bool redirection = true;

    [LabelText("升级组")]
    [LabelWidth(70)]
    [HorizontalGroup("B", 130)]
    public int UpgradeGroupID;

    [LabelText("建造笔刷图片")]
    [LabelWidth(80)]
    [HorizontalGroup("C", 150)]
    [PreviewField(60, Alignment = ObjectFieldAlignment.Left)]
    public Sprite EditBrushSprite;

    [LabelText("建造默认旋转")]
    [LabelWidth(70)]
    [HorizontalGroup("C", 130)]
    public int EditorBrushDefaultRotation;

    [LabelText("Tag")]
    [HorizontalGroup("D")]
    [EnumToggleButtons]
    public ItemTag ItemTags;


    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/A", 200)]
    [LabelText("基础血量")]
    [LabelWidth(80)]
    public int BaseHP;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/Z", 200)]
    [LabelText("能源消耗")]
    [LabelWidth(80)]
    public int BaseEnergyCost;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/Z", 200)]
    [LabelText("能源产生")]
    [LabelWidth(80)]
    public int BaseEnergyGenerate;


    [System.Obsolete]
    protected override void OnEnable()
    {
        base.OnEnable();

        _mapSize = GetMapSize();
    }

    [OnInspectorInit]
    protected override void InitData()
    {
        if (Map == null)
        {
            Map = new int[GameGlobalConfig.UnitMaxSize, GameGlobalConfig.UnitMaxSize];
        }
    }

    protected override Vector2Int GetMapPivot()
    {
        //base.GetMapPivot();
        if (Map == null)
            return Vector2Int.zero;
        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                if (Map[x, y] == 1)
                {
                    return GameHelper.CoordinateArrayToMap(new Vector2Int(x, y), GameGlobalConfig.UnitMapSize);
                }
            }
        }
        return Vector2Int.zero;
    }

    public bool HasUnitTag(ItemTag tag)
    {
        return ItemTags.HasFlag(tag);
    }

    public override Vector2 GetMapSize()
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
                    tempvalue = GameHelper.CoordinateArrayToMap(new Vector2Int(row, colume), GameGlobalConfig.UnitMapSize);
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
                    coord = GameHelper.CoordinateArrayToMap(new Vector2Int(x, y), GameGlobalConfig.UnitMapSize);
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
                    coord = GameHelper.CoordinateArrayToMap(new Vector2Int(x, y), GameGlobalConfig.UnitMapSize);
                    coord = coord - MapPivot;

                    reletiveCoord.Add(coord);
                }
            }
        }
        return reletiveCoord.ToArray();
    }
}
