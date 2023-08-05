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
    AttackSpeed,
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
}
