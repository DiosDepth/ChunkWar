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
    /// 总负载能力，固定值
    /// </summary>
    ShipWreckageLoadTotal,
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
    /// 总能量值
    /// </summary>
    EnergyTotalAdd,
    /// <summary>
    /// 护盾能源消耗
    /// </summary>
    ShieldEnergyCostPercent,
}
