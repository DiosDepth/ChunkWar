using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoolType
{
    True,
    False,
    All
}

[System.Serializable]
public abstract class ModifyTriggerConfig 
{
    [HorizontalGroup("AA", 250)]
    [LabelText("触发类型")]
    [LabelWidth(80)]
    [ReadOnly]
    public ModifyTriggerType TriggerType;

    [HorizontalGroup("AA", 120)]
    [LabelText("触发次数")]
    [LabelWidth(80)]
    public int TriggerCount;

    [HorizontalGroup("AA", 120)]
    [LabelText("效果唯一")]
    [LabelWidth(80)]
    public bool SelfUnique;

    [HorizontalGroup("AA", 200)]
    [LabelText("触发次数用完是否移除")]
    [LabelWidth(150)]
    public bool RemoveIfTriggerCountReach;

    [HorizontalGroup("AA", 120)]
    [LabelText("概率触发")]
    [LabelWidth(80)]
    public bool UsePercent;

    [ShowIf("UsePercent")]
    [HorizontalGroup("AA", 140)]
    [LabelText("概率")]
    [LabelWidth(50)]
    public float Percent;

    [ShowIf("UsePercent")]
    [HorizontalGroup("AA", 140)]
    [LabelText("幸运修正")]
    [LabelWidth(80)]
    public bool UseLuckModify = false;

    [ShowIf("UseLuckModify")]
    [HorizontalGroup("AA", 140)]
    [LabelText("修正比例")]
    [LabelWidth(80)]
    public float LuckModifyRate;

    [LabelText("触发效果")]
    [HorizontalGroup("AC", Order = 1000)]
    [HideReferenceObjectPicker]
    [DisableContextMenu(DisableForMember = true, DisableForCollectionElements = true)]
    [ValueDropdown("GetEffectList", DrawDropdownForListElements = false)]
    public ModifyTriggerEffectConfig[] Effects = new ModifyTriggerEffectConfig[0];

    public ModifyTriggerConfig(ModifyTriggerType type)
    {
        this.TriggerType = type;
    }

    public static ValueDropdownList<ModifyTriggerConfig> GetModifyTriggerList()
    {
        ValueDropdownList<ModifyTriggerConfig> result = new ValueDropdownList<ModifyTriggerConfig>();
        foreach(ModifyTriggerType type in System.Enum.GetValues(typeof(ModifyTriggerType)))
        {
            if (type == ModifyTriggerType.OnKillEnemy)
            {
                result.Add(type.ToString(), new MTC_OnKillEnemy(type));
            }
            else if (type == ModifyTriggerType.PropertyTransfer)
            {
                result.Add(type.ToString(), new MTC_PropertyTransfer(type));
            }
            else if (type == ModifyTriggerType.ItemTransfer)
            {
                result.Add(type.ToString(), new MTC_ItemTransfer(type));
            }
            else if (type == ModifyTriggerType.WaveState)
            {
                result.Add(type.ToString(), new MTC_WaveState(type));
            }
            else if (type == ModifyTriggerType.Timer)
            {
                result.Add(type.ToString(), new MTC_Timer(type));
            }
            else if (type == ModifyTriggerType.OnAdd)
            {
                result.Add(type.ToString(), new MTC_OnAdd(type));
            }
            else if (type == ModifyTriggerType.OnPlayerShipMove)
            {
                result.Add(type.ToString(), new MTC_OnPlayerShipMove(type));
            }
            else if (type == ModifyTriggerType.OnRefreshShop)
            {
                result.Add(type.ToString(), new MTC_OnRefreshShop(type));
            }
            else if (type == ModifyTriggerType.ItemRarityCount)
            {
                result.Add(type.ToString(), new MTC_ByItemRarityCount(type));
            }
            else if (type == ModifyTriggerType.OnEnterHarbor)
            {
                result.Add(type.ToString(), new MTC_OnEnterHarbor(type));
            }
            else if (type == ModifyTriggerType.ByCoreHPPercent)
            {
                result.Add(type.ToString(), new MTC_CoreHPPercent(type));
            }
            else if (type == ModifyTriggerType.OnShieldRecover)
            {
                result.Add(type.ToString(), new MTC_OnShieldRecover(type));
            }
            else if (type == ModifyTriggerType.OnWeaponHitTarget)
            {
                result.Add(type.ToString(), new MTC_OnWeaponHitTarget(type));
            }
            else if (type == ModifyTriggerType.OnPlayerWeaponReload)
            {
                result.Add(type.ToString(), new MTC_OnWeaponReload(type));
            }
            else if (type == ModifyTriggerType.OnPlayerWeaponFire)
            {
                result.Add(type.ToString(), new MTC_OnWeaponFire(type));
            }
            else if (type == ModifyTriggerType.OnPlayerShipParry)
            {
                result.Add(type.ToString(), new MTC_OnPlayerShipParry(type));
            }
            else if (type == ModifyTriggerType.OnShieldBroken)
            {
                result.Add(type.ToString(), new MTC_OnShieldBroken(type));
            }
            else if (type == ModifyTriggerType.OnPlayerUnitTakeDamage)
            {
                result.Add(type.ToString(), new MTC_OnPlayerUnitTakeDamage(type));
            }
            else if (type == ModifyTriggerType.OnPlayerCreateExplode)
            {
                result.Add(type.ToString(), new MTC_OnPlayerCreateExplode(type));
            }
            else if (type == ModifyTriggerType.OnBuyShopItem)
            {
                result.Add(type.ToString(), new MTC_OnBuyShopItem(type));
            }
            else if (type == ModifyTriggerType.OnCollectPickable)
            {
                result.Add(type.ToString(), new MTC_OnCollectPickable(type));
            }
            else if (type == ModifyTriggerType.OnPlayerUnitParalysis)
            {
                result.Add(type.ToString(), new MTC_OnPlayerUnitParalysis(type));
            }
            else if (type == ModifyTriggerType.OnEnterShop)
            {
                result.Add(type.ToString(), new MTC_OnEnterShop(type));
            }
            else if(type == ModifyTriggerType.OnSellShipEquip)
            {
                result.Add(type.ToString(), new MTC_OnSellShipUnit(type));
            }
            else if(type == ModifyTriggerType.OnEnemyCoreHPChange)
            {
                result.Add(type.ToString(), new MTC_EnemyCoreHPPercent(type));
            }
        }

        return result;
    }

    public abstract ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid);

    private static ValueDropdownList<ModifyTriggerEffectConfig> GetEffectList()
    {
        return ModifyTriggerEffectConfig.GetModifyEffectTriggerList();
    }
}

public class MTC_OnAdd : ModifyTriggerConfig
{
    public MTC_OnAdd(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_OnAdd(this, uid);
    }
}

public class MTC_OnKillEnemy : ModifyTriggerConfig
{
    [HorizontalGroup("AB", 150)]
    [LabelText("是否循环触发")]
    [LabelWidth(100)]
    public bool IsLoop;

    [HorizontalGroup("AB", 150)]
    [LabelText("触发击杀数")]
    [LabelWidth(100)]
    public int TriggerRequireCount;

    [HorizontalGroup("AB", 150)]
    [LabelText("暴击击杀")]
    [LabelWidth(100)]
    public BoolType CheckCriticalKill = BoolType.All;

    public MTC_OnKillEnemy(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_ShipDie(this, uid);
    }
}

public class MTC_WaveState : ModifyTriggerConfig
{
    [HorizontalGroup("AB", 150)]
    [LabelText("波次开始")]
    [LabelWidth(100)]
    public bool IsStart;

    public MTC_WaveState(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_WaveState(this, uid);
    }
}

public class MTC_Timer : ModifyTriggerConfig
{
    public enum TimerType
    {
        EveryPerTime,
        OnceTime
    }

    public enum SpecialTimeType
    {
        NONE,
        /// <summary>
        /// 倒数第X秒
        /// </summary>
        LastSeconds,
        /// <summary>
        /// 开始第X秒
        /// </summary>
        StartSeconds,
    }

    [HorizontalGroup("AB", 200)]
    [LabelText("时间出发类型")]
    [LabelWidth(50)]
    public TimerType TimerTriggerType;

    [HorizontalGroup("AB", 200)]
    [LabelText("特殊时间类型")]
    [LabelWidth(90)]
    public SpecialTimeType SpecialType;

    [HorizontalGroup("AB", 120)]
    [LabelText("时间参数")]
    [LabelWidth(50)]
    public float TimeParam;

    public MTC_Timer(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_Timer(this, uid);
    }
}

public class MTC_PropertyTransfer : ModifyTriggerConfig
{
    [HorizontalGroup("AB", 200)]
    [LabelText("来源")]
    [LabelWidth(50)]
    public PropertyModifyKey BaseProperty;

    [HorizontalGroup("AB", 150)]
    [LabelText("来源值")]
    [LabelWidth(50)]
    public float BaseValuePer;

    [HorizontalGroup("AB", 200)]
    [LabelText("目标")]
    [LabelWidth(50)]
    public PropertyModifyKey TargetProperty;

    [HorizontalGroup("AB", 150)]
    [LabelText("目标值")]
    [LabelWidth(50)]
    public float TargetValuePer;

    public MTC_PropertyTransfer(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_PropertyTransfer(this, uid);
    }
}

public class MTC_ItemTransfer : ModifyTriggerConfig
{
    [HorizontalGroup("AB", 200)]
    [LabelText("来源")]
    [LabelWidth(50)]
    public GoodsItemType ItemType;

    [HorizontalGroup("AB", 200)]
    [LabelText("物品ID")]
    [InfoBox("残骸 为 -1")]
    [LabelWidth(50)]
    public int ItemID;

    [HorizontalGroup("AB", 150)]
    [LabelText("来源值")]
    [LabelWidth(50)]
    public float BaseValuePer;

    [HorizontalGroup("AB", 200)]
    [LabelText("目标")]
    [LabelWidth(50)]
    public PropertyModifyKey TargetProperty;

    [HorizontalGroup("AB", 150)]
    [LabelText("目标值")]
    [LabelWidth(50)]
    public float TargetValuePer;

    public MTC_ItemTransfer(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_ItemTransfer(this, uid);
    }
}

public class MTC_OnPlayerShipMove : ModifyTriggerConfig
{
    [HorizontalGroup("AB", 200)]
    [LabelText("是否移动")]
    [LabelWidth(50)]
    public bool IsMoving;

    public MTC_OnPlayerShipMove(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_PlayerShipMove(this, uid);
    }
}

/// <summary>
/// 根据所持物件稀有度类型数量
/// </summary>
public class MTC_ByItemRarityCount : ModifyTriggerConfig
{
    public enum ItemType
    {
        Unit,
        Plug,
        All
    }


    [HorizontalGroup("AB", 200)]
    [LabelText("类型")]
    [LabelWidth(50)]
    public ItemType Type;
    public List<GoodsItemRarity> VaildRarity = new List<GoodsItemRarity>();

    public MTC_ByItemRarityCount(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_ItemRarityCount(this, uid);
    }
}

public class MTC_OnRefreshShop : ModifyTriggerConfig
{
    public MTC_OnRefreshShop(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_OnShopRefresh(this, uid);
    }
}

public class MTC_OnEnterHarbor : ModifyTriggerConfig
{
    public MTC_OnEnterHarbor(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_OnEnterHarbor(this, uid);
    }
}


public class MTC_OnEnterShop : ModifyTriggerConfig
{
    [LabelText("是否进入")]
    [LabelWidth(80)]
    [HorizontalGroup("Z", 200)]
    public bool isEnter = true;

    public MTC_OnEnterShop(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_OnEnterShop(this, uid);
    }
}


public class MTC_CoreHPPercent : ModifyTriggerConfig
{

    [HorizontalGroup("AB", 200)]
    [LabelText("比例")]
    [LabelWidth(50)]
    public byte HPPercent;

    [HorizontalGroup("AB", 200)]
    [LabelText("比较")]
    [LabelWidth(50)]
    public CompareType Compare;

    public MTC_CoreHPPercent(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_CoreHPPercent(this, uid);
    }
}

public class MTC_EnemyCoreHPPercent : ModifyTriggerConfig
{

    [HorizontalGroup("AB", 200)]
    [LabelText("比例")]
    [LabelWidth(50)]
    public byte HPPercent;

    [HorizontalGroup("AB", 200)]
    [LabelText("比较")]
    [LabelWidth(50)]
    public CompareType Compare;

    public MTC_EnemyCoreHPPercent(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_EnemyCoreHPPercent(this, uid);
    }
}

public class MTC_OnShieldRecover : ModifyTriggerConfig
{

    public MTC_OnShieldRecover(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_OnShieldRecover(this, uid);
    }
}

public class MTC_OnWeaponHitTarget : ModifyTriggerConfig
{
    [HorizontalGroup("AB", 150)]
    [LabelText("判断伤害类型")]
    [LabelWidth(80)]
    public BoolType DamageTypeBool = BoolType.All;

    [HorizontalGroup("AB", 200)]
    [LabelText("伤害类型")]
    [LabelWidth(50)]
    public WeaponDamageType DamageType;

    [HorizontalGroup("AB", 120)]
    [LabelText("判断暴击")]
    [LabelWidth(50)]
    public BoolType CriticalBool = BoolType.All;

    [HorizontalGroup("AB", 120)]
    [LabelText("当前武器")]
    [LabelWidth(50)]
    public bool CheckCurrentFireWeapon = false;

    [HorizontalGroup("AD", 150)]
    [LabelText("修正最终伤害")]
    [LabelWidth(80)]
    public bool ModifyFinalDamage = false;

    [HorizontalGroup("AD", 120)]
    [LabelText("伤害增加%")]
    [LabelWidth(50)]
    [ShowIf("ModifyFinalDamage")]
    public float DamageAddPercent;

    public MTC_OnWeaponHitTarget(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_OnWeaponHitTarget(this, uid);
    }
}

public class MTC_OnWeaponReload : ModifyTriggerConfig
{
    public enum EffectMode
    {
        DurationEffect,
        TriggerInterval
    }

    [HorizontalGroup("AD", 350)]
    [LabelText("生效模式")]
    [LabelWidth(120)]
    public EffectMode Mode = EffectMode.DurationEffect;

    [HorizontalGroup("AD", 150)]
    [LabelText("间隔")]
    [LabelWidth(80)]
    [ShowIf("Mode", EffectMode.TriggerInterval)]
    public float Interval;

    public MTC_OnWeaponReload(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_OnWeaponReload(this, uid);
    }
}

public class MTC_OnWeaponFire : ModifyTriggerConfig
{
    [HorizontalGroup("AD", 120)]
    [LabelText("射击次数")]
    [LabelWidth(50)]
    public bool CheckFireCount;

    [HorizontalGroup("AD", 120)]
    [LabelText("射击次数")]
    [LabelWidth(50)]
    [ShowIf("CheckFireCount")]
    public int FireCount;

    public MTC_OnWeaponFire(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_OnWeaponFire(this, uid);
    }
}

public class MTC_OnPlayerShipParry : ModifyTriggerConfig
{
    public MTC_OnPlayerShipParry(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_PlayerShipParry(this, uid);
    }
}

public class MTC_OnShieldBroken : ModifyTriggerConfig
{
    public bool IsPlayer = true;

    public MTC_OnShieldBroken(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_OnShieldBroken(this, uid);
    }
}

public class MTC_OnPlayerUnitTakeDamage : ModifyTriggerConfig
{
    [HorizontalGroup("AD", 120)]
    [LabelText("是否核心")]
    [LabelWidth(50)]
    public bool IsCoreUnit = false;

    [HorizontalGroup("AD", 120)]
    [LabelText("值")]
    [LabelWidth(50)]
    public int Value;

    [HorizontalGroup("AD", 250)]
    [LabelText("比较")]
    [LabelWidth(50)]
    public CompareType Compare;

    [HorizontalGroup("AD", 150)]
    [LabelText("修正最终伤害")]
    [LabelWidth(80)]
    public bool ModifyFinalDamage = false;

    [HorizontalGroup("AD", 120)]
    [LabelText("伤害增加%")]
    [LabelWidth(50)]
    [ShowIf("ModifyFinalDamage")]
    public float DamageAddPercent;

    public MTC_OnPlayerUnitTakeDamage(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_PlayerUnitTakeDamage(this, uid);
    }
}

public class MTC_OnPlayerCreateExplode : ModifyTriggerConfig
{
    public MTC_OnPlayerCreateExplode(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_OnPlayerCreateExplode(this, uid);
    }
}

public class MTC_OnBuyShopItem : ModifyTriggerConfig
{
    [HorizontalGroup("AD", 250)]
    [LabelText("检测特殊商品")]
    [LabelWidth(100)]
    public bool UseSpecialShopItemID = false;

    [ShowIf("UseSpecialShopItemID")]
    [LabelText("特殊商品ID")]
    public int ShopItemID;

    public MTC_OnBuyShopItem(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_OnBuyShopItem(this, uid);
    }
}

public class MTC_OnCollectPickable : ModifyTriggerConfig
{
    public AvaliablePickUp PickUpType;
    public MTC_OnCollectPickable(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_OnCollectPickable(this, uid);
    }
}

public class MTC_OnPlayerUnitParalysis : ModifyTriggerConfig
{
    [HorizontalGroup("AD", 250)]
    [LabelText("进入瘫痪")]
    [LabelWidth(100)]
    public bool IsEnterParalysis;

    public MTC_OnPlayerUnitParalysis(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_OnPlayerUnitParalysis(this, uid);
    }
}

public class MTC_OnSellShipUnit : ModifyTriggerConfig
{

    [HorizontalGroup("AD", 250)]
    [LabelText("检测ID")]
    [LabelWidth(100)]
    public bool CheckUnitID = false;

    [HorizontalGroup("AD", 250)]
    [LabelText("单位ID")]
    [LabelWidth(100)]
    [ShowIf("CheckUnitID")]
    public int UnitID;

    public MTC_OnSellShipUnit(ModifyTriggerType type) : base(type)
    {

    }

    public override ModifyTriggerData Create(ModifyTriggerConfig cfg, uint uid)
    {
        return new MT_OnPlayerUnitParalysis(this, uid);
    }
}