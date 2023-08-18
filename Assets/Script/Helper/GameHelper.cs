using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class GameHelper 
{
    public static string ShopItemType_ShipPlug_Text = "ShopItemType_ShipPlug_Text";
    public static string ShopItemType_ShipBuilding_Text = "ShopItemType_ShipBuilding_Text";
    public static string ShopItemType_ShipWeapon_Text = "ShopItemType_ShipWeapon_Text";
    public static string Bool_True_Text = "Bool_True_Text";
    public static string Bool_False_Text = "Bool_False_Text";

    private static string Color_White_Code = "#FFFFFF";
    private static string Color_Red_Code = "#C61616";
    private static string Color_Blue_Code = "#09B3CB";

    private static Color RarityColor_T1 = Color.white;
    private static Color RarityColor_T2 = new Color(0, 0.83f, 1f);
    private static Color RarityColor_T3 = new Color(0.73f, 0f, 1f);
    private static Color RarityColor_T4 = new Color(1f, 0.57f, 0.23f);


    /// <summary>
    /// 获取稀有度颜色
    /// </summary>
    /// <param name="rarity"></param>
    /// <returns></returns>
    public static Color GetRarityColor(GoodsItemRarity rarity)
    {
        switch (rarity)
        {
            case GoodsItemRarity.Tier1:
                return RarityColor_T1;
            case GoodsItemRarity.Tier2:
                return RarityColor_T2;
            case GoodsItemRarity.Tier3:
                return RarityColor_T3;
            case GoodsItemRarity.Tier4:
                return RarityColor_T4;
            default:
                return Color.white;
        }
    }

    #region Battle

    public static float GetEnemyDamageByHardLevel(EnemyHardLevelMap cfg)
    {
        var currentHardLevel = RogueManager.Instance.GetHardLevelValue;
        return currentHardLevel * cfg.DamageRatio;
    }

    public static float GetEnemyHPByHardLevel(EnemyHardLevelMap cfg)
    {
        var currentHardLevel = RogueManager.Instance.GetHardLevelValue;
        return currentHardLevel * cfg.HPRatio;
    }

    /// <summary>
    /// 获取当前升级所需最大值EXP
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static int GetEXPRequireMaxCount(byte level)
    {
        var expMap = DataManager.Instance.battleCfg.EXPMap;
        if(expMap == null || expMap.Length < level)
        {
            Debug.Log("EXP MAP ERROR! LEVEL = " + level);
            return int.MaxValue;
        }
        return expMap[level];
    }

    /// <summary>
    /// 获取玩家飞船核心属性
    /// </summary>
    /// <returns></returns>
    public static UnitBaseAttribute GetPlayerShipCoreAttribute()
    {
        var currentShip = RogueManager.Instance.currentShip;
        if (currentShip == null)
            return null;

        return currentShip.core.unit.baseAttribute;
    }

    public static GeneralHPComponet GetPlayerShipHPComponet()
    {
        var currentShip = RogueManager.Instance.currentShip;
        if (currentShip == null)
            return null;

        return currentShip.core.unit.HpComponent;
    }

    public static int GetUnitEnergyCost(BaseUnitConfig cfg)
    {
        var rate = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.UnitEnergyCostPercent);
        rate = Mathf.Clamp(rate, -100, float.MaxValue);
        return Mathf.RoundToInt(cfg.BaseEnergyCost * (100 + rate) / 100f);
    }

    #endregion

    public static Vector2Int CoordinateArrayToMap(Vector2Int arraycoord, int mapsize)
    {
        return new Vector2Int(arraycoord.x - mapsize, mapsize - arraycoord.y);
    }

    public static Vector2Int CoordinateMapToArray(Vector2Int shipcoord, int mapsize)
    {
        return new Vector2Int(mapsize + shipcoord.x, mapsize - shipcoord.y);
    }

    public static Vector2Int GetReletiveCoordFromWorldPos(Transform trs, Vector2 worldpos)
    {
        Vector2 reletivePos = worldpos - trs.position.ToVector2();
        Vector2Int roundPos = reletivePos.Round();

        return roundPos;
    }

    public static Vector2 GetWorldPosFromReletiveCoord(Transform trs, Vector2Int coord)
    {
        Vector3 tempcoord = trs.TransformPoint(coord.ToVector3());
        return tempcoord.ToVector2();
    }

    /// <summary>
    /// 全部舰船ID
    /// </summary>
    /// <returns></returns>
    public static List<uint> GetAllShipIDs()
    {
        var allShips = DataManager.Instance.GetAllShipConfigs();
        return allShips.Select(x => (uint)x.ID).ToList();
    }

    public static List<uint> GetAllHardLevels()
    {
        var allHardLevel = GameManager.Instance.GetAllHardLevelInfos;
        return allHardLevel.Select(x => (uint)x.HardLevelID).ToList();
    }

    public static List<uint> GetRogueShipPlugItems()
    {
        var allItems = RogueManager.Instance.GetAllCurrentShipPlugs;
        List<uint> goods = new List<uint>();
        for(int i = 0; i < allItems.Count; i++)
        {
            if (!goods.Contains((uint)allItems[i].GoodsID))
            {
                goods.Add((uint)allItems[i].GoodsID);
            }
        }

        return goods;
    }

    /// <summary>
    /// 单位升级花费
    /// </summary>
    /// <param name="rarity"></param>
    /// <returns></returns>
    public static byte GetUnitUpgradeCost(GoodsItemRarity rarity)
    {
        var requireMap = DataManager.Instance.battleCfg.EvolveRequireMap;
        if (rarity == GoodsItemRarity.Tier4)
            return 0;

        return requireMap[(int)rarity];
    }

    public static byte GetEvoluveRarityPoints(GoodsItemRarity rarity)
    {
        var addMap = DataManager.Instance.battleCfg.EvolveAddMap;
        return addMap[(int)rarity];
    }

    /// <summary>
    /// 获取升级后的稀有度
    /// </summary>
    /// <param name="baseRarity"></param>
    /// <param name="addRarity"></param>
    /// <returns></returns>
    public static GoodsItemRarity GetTargetEvolveRarity(GoodsItemRarity baseRarity, byte currentRank, GoodsItemRarity addRarity, out byte outRank)
    {
        var addMap = DataManager.Instance.battleCfg.EvolveAddMap;
        var requireMap = DataManager.Instance.battleCfg.EvolveRequireMap;
        ///增加的稀有度级
        byte addRank = addMap[(int)addRarity];
        var totalRank = currentRank + addRank;

        int tempRarity = (int)baseRarity;
        var nextRequireEvolove = requireMap[tempRarity];
        while(totalRank >= nextRequireEvolove)
        {
            totalRank -= nextRequireEvolove;
            tempRarity++;
            if (tempRarity > 3)
                break;
            nextRequireEvolove = requireMap[tempRarity];
        }
        outRank = (byte)totalRank;
        return (GoodsItemRarity)tempRarity;
    }

    public static string GetHardLevelModifyTypeName(HardLevelModifyType type)
    {
        string textID = string.Format("HardLevel_{0}_Name", type);
        return LocalizationManager.Instance.GetTextValue(textID);
    }

    public static string GetUI_WeaponUnitPropertyType(UI_WeaponUnitPropertyType type)
    {
        string textID = string.Format("UI_WeaponProperty_{0}_Name", type);
        return LocalizationManager.Instance.GetTextValue(textID);
    }

    public static string GetWeaponDamageTypeText(WeaponDamageType type)
    {
        switch (type)
        {
            case WeaponDamageType.Energy:
                return LocalizationManager.Instance.GetTextValue("WeaponDamageType_Energy_Name");
            case WeaponDamageType.Physics:
                return LocalizationManager.Instance.GetTextValue("WeaponDamageType_Physics_Name");
            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// 计算实际CD
    /// </summary>
    /// <param name="rowValue"></param>
    /// <returns></returns>
    public static float CalculatePlayerWeaponCD(float rowValue)
    {
        var attackSpd = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.AttackSpeed);
        if(attackSpd > 0)
        {
            float rate = (100 + attackSpd) / 100f;
            return rowValue / rate;
        }
        else if (attackSpd == 0)
        {
            return rowValue;
        }
        else
        {
            float rate = (100 - attackSpd) / 100f;
            return rowValue * rate;
        }
    }

    public static float CalculatePlayerWeaponDamageDeltaCD(float rowValue)
    {
        var attackSpd = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.FireSpeed);
        if (attackSpd > 0)
        {
            float rate = (1 + attackSpd) / 100f;
            return rowValue / rate;
        }
        else if (attackSpd == 0)
        {
            return rowValue;
        }
        else
        {
            float rate = (1 - attackSpd) / 100f;
            return rowValue * rate;
        }
    }

    public static string GetWeaponPropertyDescContent(UI_WeaponUnitPropertyType type, WeaponConfig cfg)
    {
        var propertyData = RogueManager.Instance.MainPropertyData;
        if (type == UI_WeaponUnitPropertyType.Critical)
        {
            ///Rate
            var criticalRateRow = propertyData.GetPropertyFinal(PropertyModifyKey.Critical);
            float criticalRateValue = criticalRateRow + cfg.BaseCriticalRate;
            string criticalRateColor = GetColorCode(criticalRateValue, cfg.BaseCriticalRate, false);
            string criticalRate = string.Format("<color={0}>{1:F1}%</color>", criticalRateColor, criticalRateValue);

            ///Damage
            var criticalDamageRow = propertyData.GetPropertyFinal(PropertyModifyKey.CriticalDamagePercentAdd);
            var criticalDamageValue = criticalDamageRow + cfg.CriticalDamage;
            string criticalDamageColor = GetColorCode(criticalDamageValue, cfg.CriticalDamage, false);
            string criticalDamage = string.Format("<color={0}>{1:F1}%</color>", criticalDamageColor, criticalDamageValue);
            return string.Format("{0} | {1}", criticalRate, criticalDamage);
        }
        else if (type == UI_WeaponUnitPropertyType.HP)
        {
            var hprow = propertyData.GetPropertyFinal(PropertyModifyKey.HP);
            int hp = Mathf.CeilToInt(hprow + cfg.BaseHP);
            string color = GetColorCode(hp, cfg.BaseHP, false);

            return string.Format("<color={0}>{1}</color>", color, hp);
        }
        else if (type == UI_WeaponUnitPropertyType.DamageRatio)
        {
            return string.Format("{0:F2} ~ {1:F2}", cfg.DamageRatioMin, cfg.DamageRatioMax);
        }
        else if (type == UI_WeaponUnitPropertyType.Damage)
        {
            var damageCount = cfg.TotalDamageCount;
            var damage = CalculteWeaponDamage(cfg);
            string color = GetColorCode(damage, cfg.DamageBase, false);

            if (damageCount > 1)
            {
                return string.Format("<color={0}>{1}</color> X{2}", color, damage, damageCount);
            }
            else
            {
                return string.Format("<color={0}>{1}</color>", color, damage);
            }
        }
        else if (type == UI_WeaponUnitPropertyType.Range)
        {
            var rangerow = propertyData.GetPropertyFinal(PropertyModifyKey.WeaponRange);
            int range = Mathf.RoundToInt(rangerow + cfg.BaseRange);
            string color = GetColorCode(range, cfg.BaseRange, false);

            return string.Format("<color={0}>{1}</color>", color, range);
        }
        else if (type == UI_WeaponUnitPropertyType.CD)
        {
            var fireCD = CalculatePlayerWeaponDamageDeltaCD(cfg.FireCD);
            var cd = CalculatePlayerWeaponCD(cfg.CD);

            string FireCDcolor = GetColorCode(fireCD, cfg.FireCD, true);
            string cdColor = GetColorCode(cd, cfg.CD, true);

            string fireCDStr = string.Format("<color={0}>{1:F2}s</color>", FireCDcolor, fireCD);
            string cdStr = string.Format("<color={0}>{1:F2}s</color>", cdColor, cd);
            return string.Format("{0} | {1}", fireCDStr, cdStr);
        } 
        else if (type == UI_WeaponUnitPropertyType.ShieldTransfixion)
        {
            string boolColor = GetColorCode(cfg.ShieldTransfixion ? 0f : 1f, 0.5f, true);
            return string.Format("<color={0}>{1}</color>", boolColor,
                cfg.ShieldTransfixion ?
                LocalizationManager.Instance.GetTextValue(Bool_True_Text) :
                LocalizationManager.Instance.GetTextValue(Bool_False_Text));
        }
        else if (type == UI_WeaponUnitPropertyType.ShieldDamage)
        {
            var shieldDamage = CalculateShieldDamage(cfg);
            string damageColor = GetColorCode(shieldDamage, cfg.ShieldDamage, false);
            return string.Format("<color={0}>{1:F2}%</color>", damageColor, shieldDamage);
        }

        return string.Empty;
    }

    /// <summary>
    /// 计算武器的伤害
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    private static int CalculteWeaponDamage(WeaponConfig cfg)
    {
        float rowDamage = cfg.DamageBase;

        for(int i = 0; i < cfg.DamageModifyFrom.Count; i++)
        {
            var item = cfg.DamageModifyFrom[i];
            var value = RogueManager.Instance.MainPropertyData.GetPropertyFinal(item.PropertyKey);
            rowDamage += value * item.Ratio;
        }
        rowDamage = Mathf.Clamp(rowDamage, 0, rowDamage);

        var damagePercent = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.DamagePercent);
        rowDamage = rowDamage * (1 + damagePercent / 100f);

        return Mathf.RoundToInt(rowDamage);
    }

    private static float CalculateShieldDamage(WeaponConfig cfg)
    {
        float shieldbase = cfg.ShieldDamage;
        var shieldAdd = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.ShieldDamageAdd);
        var result = shieldbase + shieldAdd;
        result = Mathf.Clamp(result, 0, float.MaxValue);
        return result;
    }

    private static string GetColorCode(float value1, float value2, bool reverse)
    {
        if (value1 == value2)
            return Color_White_Code;

        if(value1 > value2)
        {
            return reverse ? Color_Red_Code : Color_Blue_Code;
        }
        else
        {
            return reverse ? Color_Blue_Code : Color_Red_Code;
        }
    }
}
