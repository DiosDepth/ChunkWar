using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum ItemTag 
{
    Shield = 1<<1,
    Weapon = 1<<2,
    MainWeapon = 1<<3,
    WareHouse = 1<<4,
    Building = 1 << 5,
    Reactor = 1<<6
}

[System.Flags]
public enum ShipPlugTag
{
    PhysicsDamage = 1<<1,
    EnergyDamage = 1<<2,
}

[System.Serializable]
[HideReferenceObjectPicker]
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

    [LabelText("组ID")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 200)]
    public int GroupID;

    [LabelText("可旋转")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 150)]
    public bool redirection = true;

    [LabelText("默认解锁")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 150)]
    public bool UnlockDefault = true;

    [LabelText("机制描述")]
    [LabelWidth(80)]
    public string ProertyDescText;

    [FoldoutGroup("建造配置")]
    [LabelText("建造笔刷图片")]
    [LabelWidth(80)]
    [PreviewField(60, Alignment = ObjectFieldAlignment.Left)]
    public Sprite EditBrushSprite;

    [LabelText("建造默认旋转")]
    [LabelWidth(70)]
    [FoldoutGroup("建造配置")]
    [HorizontalGroup("建造配置/A", 200)]
    public int EditorBrushDefaultRotation;

    [LabelText("偏移X")]
    [LabelWidth(70)]
    [FoldoutGroup("建造配置")]
    [HorizontalGroup("建造配置/A", 200)]
    public float CorsorOffsetX = 0;

    [LabelText("偏移Y")]
    [LabelWidth(70)]
    [FoldoutGroup("建造配置")]
    [HorizontalGroup("建造配置/A", 200)]
    public float CorsorOffsetY = 0;

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

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/Z", 200)]
    [LabelText("负载加成")]
    [LabelWidth(80)]
    public int LoadAdd;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/ZZ", 200)]
    [LabelText("瘫痪恢复")]
    [LabelWidth(80)]
    public bool ParalysisResume = false;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/ZZ", 200)]
    [LabelText("瘫痪恢复时长")]
    [ShowIf("ParalysisResume")]
    [LabelWidth(100)]
    public float ParalysisResumeTime;

    [FoldoutGroup("基础属性")]
    [LabelText("属性修正")]
    [LabelWidth(80)]
    [ListDrawerSettings(DraggableItems = false, CustomAddFunction = "AddPropertyMidifyConfig")]
    [HideReferenceObjectPicker]
    public PropertyModifyConfig[] PropertyModify = new PropertyModifyConfig[0];

    [FoldoutGroup("基础属性")]
    [LabelText("触发配置")]
    [LabelWidth(80)]
    [DisableContextMenu(DisableForMember = true, DisableForCollectionElements = true)]
    [ValueDropdown("GetTriggerLst", DrawDropdownForListElements = false)]
    [HideReferenceObjectPicker]
    public ModifyTriggerConfig[] ModifyTriggers = new ModifyTriggerConfig[0];

    [FoldoutGroup("基础属性")]
    [LabelText("触发格效果")]
    [LabelWidth(80)]
    [DisableContextMenu(DisableForMember = true, DisableForCollectionElements = true)]
    [HideReferenceObjectPicker]
    public UnitSlotEffectConfig[] SlotEffects = new UnitSlotEffectConfig[0];

    [System.Obsolete]
    protected override void OnEnable()
    {
        base.OnEnable();

        _mapSize = GetMapSize();
    }

#if UNITY_EDITOR

    [OnInspectorInit]
    protected override void InitData()
    {
        if (Map == null)
        {
            Map = new int[GameGlobalConfig.UnitMaxSize, GameGlobalConfig.UnitMaxSize];
        }
    }

    private PropertyModifyConfig AddPropertyMidifyConfig()
    {
        return new PropertyModifyConfig();
    }

    private ValueDropdownList<ModifyTriggerConfig> GetTriggerLst()
    {
        return ModifyTriggerConfig.GetModifyTriggerList();
    }

#endif


    protected override Vector2Int GetMapPivot()
    {
        //base.GetMapPivot();
        if (Map == null)
            return Vector2Int.zero;
        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                if (Map[x, y] == 2)
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
                if (Map[x, y] == 1)
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
                if (Map[x, y] == 1)
                {
                    coord = GameHelper.CoordinateArrayToMap(new Vector2Int(x, y), GameGlobalConfig.UnitMapSize);
                    coord = coord - MapPivot;

                    reletiveCoord.Add(coord);
                }
            }
        }
        return reletiveCoord.ToArray();
    }

    public Vector2Int[] GetEffectSlotCoord()
    {
        Vector2Int coord;
        List<Vector2Int> effectCoord = new List<Vector2Int>();

        effectCoord.Add(MapPivot);
        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                if (Map[x, y] == 3)
                {
                    coord = GameHelper.CoordinateArrayToMap(new Vector2Int(x, y), GameGlobalConfig.UnitMapSize);
                    coord = coord - MapPivot;

                    effectCoord.Add(coord);
                }
            }
        }
        return effectCoord.ToArray();
    }
}
