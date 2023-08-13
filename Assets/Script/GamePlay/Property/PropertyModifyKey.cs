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
    DamageDeltaPercent,
    /// <summary>
    /// 护盾伤害额外加成
    /// </summary>
    ShieldDamageAdd,
    /// <summary>
    /// 单位能量消耗百分比
    /// </summary>
    UnitEnergyCostPercent,
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
    /// 贯通衰减
    /// </summary>
    TransfixionReducePercent,
}
