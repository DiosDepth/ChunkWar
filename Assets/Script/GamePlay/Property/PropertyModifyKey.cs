 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PropertyModifyKey 
{
    NONE,
    HP,
    /// <summary>
    /// 最终伤害百分比
    /// </summary>
    DamagePercent,
    /// <summary>
    /// 暴击率
    /// </summary>
    Critical,
    /// <summary>
    /// 幸运
    /// </summary>
    Luck,
    /// <summary>
    /// 武器范围
    /// </summary>
    WeaponRange,
    /// <summary>
    /// 商店刷新数量
    /// </summary>
    ShopRefreshCount,
    ShopCostPercent,
    /// <summary>
    /// 装填冷却
    /// </summary>
    AttackSpeed,
    /// <summary>
    /// 拾取范围
    /// </summary>
    SuckerRange,
    EnemyHPPercent,
    EnemyDamagePercent,
    /// <summary>
    /// 经验额外获得百分比
    /// </summary>
    EXPAddPercent,
    CurrencyAddPercent,
    /// <summary>
    /// 暴击伤害增加
    /// </summary>
    CriticalDamagePercentAdd,
    /// <summary>
    /// 实弹伤害
    /// </summary>
    PhysicsDamage,
    /// <summary>
    /// 能量伤害
    /// </summary>
    EnergyDamage,
    /// <summary>
    /// 伤害间隔
    /// </summary>
    FireSpeed,
    /// <summary>
    /// 护盾伤害额外加成
    /// </summary>
    ShieldDamageAdd,
    /// <summary>
    /// 武器能量消耗百分比
    /// </summary>
    WeaponEnergyCostPercent,
    /// <summary>
    /// 单位能源产生，百分比
    /// </summary>
    UnitEnergyGenerate,
    ShieldHP,
    /// <summary>
    /// 弹药量
    /// </summary>
    MagazineSize,
    /// <summary>
    /// 基础贯通
    /// </summary>
    Transfixion,
    /// <summary>
    /// 贯通伤害
    /// </summary>
    TransfixionDamagePercent,
    ShieldRecoverValue,
    /// <summary>
    /// 护盾多久不受击恢复CD百分比
    /// </summary>
    ShieldRecoverTimeReduce,
    /// <summary>
    /// 出售价格加成
    /// </summary>
    SellPrice,
    /// <summary>
    /// 负载消耗，百分比
    /// </summary>
    UnitLoadCost,
    /// <summary>
    /// 仓库负载能力，百分比
    /// </summary>
    UnitWreckageLoadAdd,
    /// <summary>
    /// 护盾半径增加
    /// </summary>
    ShieldRatioAdd,
    /// <summary>
    /// 船体装甲
    /// </summary>
    ShipArmor,
    /// <summary>
    /// 废品出售价格
    /// </summary>
    WasteSellPriceAdd,
    /// <summary>
    /// 废品负重比例
    /// </summary>
    WasteLoadPercent,
    /// <summary>
    /// 敌人掉落
    /// </summary>
    EnemyDropCountPercent,
    /// <summary>
    /// 护盾强化
    /// </summary>
    ShieldArmor,
    /// <summary>
    /// 总能量值消耗
    /// </summary>
    EnergyCostAdd,
    /// <summary>
    /// 护盾能源消耗
    /// </summary>
    ShieldEnergyCostPercent,
    /// <summary>
    /// 舰船速度
    /// </summary>
    ShipSpeed,
    /// <summary>
    /// 舰船闪避
    /// </summary>
    ShipParry,
    /// <summary>
    /// 船体再生
    /// </summary>
    UnitHPRecoverValue,
    Explode_Damage,
    Explode_Range,
    EnemyCount,
    /// <summary>
    /// 对精英和首领的伤害
    /// </summary>
    EliteBossDamage,
    /// <summary>
    /// 敌人速度
    /// </summary>
    EnemySpeed,
    /// <summary>
    /// 指挥
    /// </summary>
    Command,
    /// <summary>
    /// 负载惩罚效果
    /// </summary>
    LoadPunishRate,
    /// <summary>
    /// 护盾无视最低伤害
    /// </summary>
    ShieldIgnoreMinDamage,
    /// <summary>
    /// 导弹HP
    /// </summary>
    MissileHPPercent,
    /// <summary>
    /// 导弹数量
    /// </summary>
    MissileCount,
    /// <summary>
    /// 装备能源消耗
    /// </summary>
    UnitEnergyCostPercent,
    /// <summary>
    /// 能源超载惩罚效果
    /// </summary>
    EnergyPunishRate,
    /// <summary>
    /// 实弹数量
    /// </summary>
    PhysicalCount,
    /// <summary>
    /// 伤害范围下限
    /// </summary>
    DamageRangeMin,
    /// <summary>
    /// 伤害范围上限
    /// </summary>
    DamageRangeMax,
    /// <summary>
    /// 单位瘫痪时长比例
    /// </summary>
    UnitParalysisRecoverTimeRatio,
    /// <summary>
    /// 陨石生成数量
    /// </summary>
    Meteorite_Generate_Count,
    AncientUnit_Generate_Count,
    Aircraft_Damage,
    Aircraft_HP,
    Aircraft_Speed,
    Aircraft_ShieldRatio,
    /// <summary>
    /// 机库能源消耗
    /// </summary>
    Hangar_EnergyCostPercent,
    TOTAL_ENERGY,
    TOTAL_LOAD,
}

/// <summary>
/// Unit单独修正Key
/// </summary>
public enum UnitPropertyModifyKey
{
    NONE,
    UnitEnergyCostPercent,
    UnitEnergyGenerateValue,
    UnitDamagePercentAdd,
}