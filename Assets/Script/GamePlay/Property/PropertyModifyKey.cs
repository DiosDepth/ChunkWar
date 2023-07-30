using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PropertyModifyKey 
{
    NONE,
    ShipHP,
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
}
