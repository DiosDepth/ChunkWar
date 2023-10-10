using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PropertyModifyKey 
{
    NONE,
    HP,
    /// <summary>
    /// �����˺��ٷֱ�
    /// </summary>
    DamagePercent,
    /// <summary>
    /// ������
    /// </summary>
    Critical,
    /// <summary>
    /// ����
    /// </summary>
    Luck,
    /// <summary>
    /// ������Χ
    /// </summary>
    WeaponRange,
    /// <summary>
    /// �̵�ˢ������
    /// </summary>
    ShopRefreshCount,
    ShopCostPercent,
    /// <summary>
    /// װ����ȴ
    /// </summary>
    AttackSpeed,
    /// <summary>
    /// ʰȡ��Χ
    /// </summary>
    SuckerRange,
    EnemyHPPercent,
    EnemyDamagePercent,
    /// <summary>
    /// ��������ðٷֱ�
    /// </summary>
    EXPAddPercent,
    CurrencyAddPercent,
    /// <summary>
    /// �����˺�����
    /// </summary>
    CriticalDamagePercentAdd,
    /// <summary>
    /// ʵ���˺�
    /// </summary>
    PhysicsDamage,
    /// <summary>
    /// �����˺�
    /// </summary>
    EnergyDamage,
    /// <summary>
    /// �˺����
    /// </summary>
    FireSpeed,
    /// <summary>
    /// �����˺�����ӳ�
    /// </summary>
    ShieldDamageAdd,
    /// <summary>
    /// �����������İٷֱ�
    /// </summary>
    WeaponEnergyCostPercent,
    /// <summary>
    /// ��λ��Դ�������ٷֱ�
    /// </summary>
    UnitEnergyGenerate,
    ShieldHP,
    /// <summary>
    /// ��ҩ��
    /// </summary>
    MagazineSize,
    /// <summary>
    /// ������ͨ
    /// </summary>
    Transfixion,
    /// <summary>
    /// ��ͨ�˺�
    /// </summary>
    TransfixionDamagePercent,
    ShieldRecoverValue,
    /// <summary>
    /// ���ܶ�ò��ܻ��ָ�CD�ٷֱ�
    /// </summary>
    ShieldRecoverTimeReduce,
    /// <summary>
    /// ���ۼ۸�ӳ�
    /// </summary>
    SellPrice,
    /// <summary>
    /// �������ģ��ٷֱ�
    /// </summary>
    UnitLoadCost,
    /// <summary>
    /// �ֿ⸺���������ٷֱ�
    /// </summary>
    UnitWreckageLoadAdd,
    /// <summary>
    /// ���ܰ뾶����
    /// </summary>
    ShieldRatioAdd,
    /// <summary>
    /// ����װ��
    /// </summary>
    ShipArmor,
    /// <summary>
    /// ��Ʒ���ۼ۸�
    /// </summary>
    WasteSellPriceAdd,
    /// <summary>
    /// ��Ʒ���ر���
    /// </summary>
    WasteLoadPercent,
    /// <summary>
    /// ���˵���
    /// </summary>
    EnemyDropCountPercent,
    /// <summary>
    /// ����ǿ��
    /// </summary>
    ShieldArmor,
    /// <summary>
    /// ������ֵ����
    /// </summary>
    EnergyCostAdd,
    /// <summary>
    /// ������Դ����
    /// </summary>
    ShieldEnergyCostPercent,
    /// <summary>
    /// �����ٶ�
    /// </summary>
    ShipSpeed,
    /// <summary>
    /// ��������
    /// </summary>
    ShipParry,
    /// <summary>
    /// ��������
    /// </summary>
    UnitHPRecoverValue,
    Explode_Damage,
    Explode_Range,
    EnemyCount,
    /// <summary>
    /// �Ծ�Ӣ��������˺�
    /// </summary>
    EliteBossDamage,
    /// <summary>
    /// �����ٶ�
    /// </summary>
    EnemySpeed,
    /// <summary>
    /// ָ��
    /// </summary>
    Command,
    /// <summary>
    /// ���سͷ�Ч��
    /// </summary>
    LoadPunishRate,
    /// <summary>
    /// ������������˺�
    /// </summary>
    ShieldIgnoreMinDamage,
    /// <summary>
    /// ����HP
    /// </summary>
    MissileHPPercent,
    /// <summary>
    /// ��������
    /// </summary>
    MissileCount,
    /// <summary>
    /// װ����Դ����
    /// </summary>
    UnitEnergyCostPercent,
    /// <summary>
    /// ��Դ���سͷ�Ч��
    /// </summary>
    EnergyPunishRate,
    /// <summary>
    /// ʵ������
    /// </summary>
    PhysicalCount,
    /// <summary>
    /// �˺���Χ����
    /// </summary>
    DamageRangeMin,
    /// <summary>
    /// �˺���Χ����
    /// </summary>
    DamageRangeMax,
    /// <summary>
    /// ��λ̱��ʱ������
    /// </summary>
    UnitParalysisRecoverTimeRatio,
    /// <summary>
    /// ��ʯ��������
    /// </summary>
    Meteorite_Generate_Count,
    AncientUnit_Generate_Count,
    Aircraft_Damage,
    Aircraft_HP,
    Aircraft_Speed,
    Aircraft_ShieldRatio,
    TOTAL_ENERGY,
    TOTAL_LOAD,
}

/// <summary>
/// Unit��������Key
/// </summary>
public enum UnitPropertyModifyKey
{
    NONE,
    UnitEnergyCostPercent,
    UnitEnergyGenerateValue,
}