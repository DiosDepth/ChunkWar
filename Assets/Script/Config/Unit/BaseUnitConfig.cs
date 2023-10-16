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
    Reactor = 1<<6,
    Hangar = 1<<7,
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
    [LabelText("����Key")]
    [LabelWidth(80)]
    public PropertyModifyKey PropertyKey;
    [HorizontalGroup("CC", 300)]
    [LabelText("����")]
    [LabelWidth(80)]
    public float Ratio;
}


[CreateAssetMenu(fileName = "Configs_Unit_", menuName = "Configs/Unit/UnitConfig")]
public class BaseUnitConfig : BaseConfig
{
    [LabelText("����")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 200)]
    public UnitType unitType;

    [LabelText("��ID")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 200)]
    public int GroupID;

    [LabelText("����ת")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 150)]
    public bool redirection = true;

    [LabelText("Ĭ�Ͻ���")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 150)]
    public bool UnlockDefault = true;

    [LabelText("��������")]
    [LabelWidth(80)]
    public string ProertyDescText;

    [FoldoutGroup("��������")]
    [LabelText("�����ˢͼƬ")]
    [LabelWidth(80)]
    [PreviewField(60, Alignment = ObjectFieldAlignment.Left)]
    public Sprite EditBrushSprite;

    [LabelText("����Ĭ����ת")]
    [LabelWidth(70)]
    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 200)]
    public int EditorBrushDefaultRotation;

    [LabelText("ƫ��X")]
    [LabelWidth(70)]
    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 200)]
    public float CorsorOffsetX = 0;

    [LabelText("ƫ��Y")]
    [LabelWidth(70)]
    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 200)]
    public float CorsorOffsetY = 0;

    [LabelText("Tag")]
    [HorizontalGroup("D")]
    [EnumToggleButtons]
    public ItemTag ItemTags;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 200)]
    [LabelText("����Ѫ��")]
    [LabelWidth(80)]
    public int BaseHP;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/Z", 200)]
    [LabelText("��Դ����")]
    [LabelWidth(80)]
    public int BaseEnergyCost;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/Z", 200)]
    [LabelText("��Դ����")]
    [LabelWidth(80)]
    public int BaseEnergyGenerate;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/Z", 200)]
    [LabelText("���ؼӳ�")]
    [LabelWidth(80)]
    public int LoadAdd;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/ZZ", 200)]
    [LabelText("̱���ָ�")]
    [LabelWidth(80)]
    public bool ParalysisResume = false;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/ZZ", 200)]
    [LabelText("̱���ָ�ʱ��")]
    [ShowIf("ParalysisResume")]
    [LabelWidth(100)]
    public float ParalysisResumeTime;

    [FoldoutGroup("��������")]
    [LabelText("��������")]
    [LabelWidth(80)]
    [ListDrawerSettings(DraggableItems = false, CustomAddFunction = "AddPropertyMidifyConfig")]
    [HideReferenceObjectPicker]
    public PropertyModifyConfig[] PropertyModify = new PropertyModifyConfig[0];

    [FoldoutGroup("��������")]
    [LabelText("��������")]
    [LabelWidth(80)]
    [DisableContextMenu(DisableForMember = true, DisableForCollectionElements = true)]
    [ValueDropdown("GetTriggerLst", DrawDropdownForListElements = false)]
    [HideReferenceObjectPicker]
    public ModifyTriggerConfig[] ModifyTriggers = new ModifyTriggerConfig[0];

    [FoldoutGroup("��������")]
    [LabelText("������Ч��")]
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