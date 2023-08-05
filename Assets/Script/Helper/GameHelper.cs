using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class GameHelper 
{
    public static string ShopItemType_ShipPlug_Text = "ShopItemType_ShipPlug_Text";
    public static string ShopItemType_ShipBuilding_Text = "ShopItemType_ShipBuilding_Text";
    public static string ShopItemType_ShipWeapon_Text = "ShopItemType_ShipWeapon_Text";

    #region Battle

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

    public static string GetWeaponPropertyDescContent(UI_WeaponUnitPropertyType type, WeaponConfig cfg)
    {
        var propertyData = RogueManager.Instance.MainPropertyData;
        if (type == UI_WeaponUnitPropertyType.Critical)
        {
            var criticalRateRow = propertyData.GetPropertyFinal(PropertyModifyKey.Critical);
            string criticalRate = string.Format("{0:F1}" ,criticalRateRow + cfg.BaseCriticalRate);
            var criticalDamageRow = propertyData.GetPropertyFinal(PropertyModifyKey.CriticalDamagePercentAdd);
            string criticalDamage = string.Format("{0:F1}", criticalDamageRow + cfg.CriticalDamage);
            return string.Format("{0}% | {1}%", criticalRate, criticalDamage);
        }
        else if (type == UI_WeaponUnitPropertyType.HP)
        {
            var hprow = propertyData.GetPropertyFinal(PropertyModifyKey.HP);
            int hp = Mathf.CeilToInt(hprow + cfg.BaseHP);
            return hp.ToString();
        }
        else if (type == UI_WeaponUnitPropertyType.DamageRatio)
        {
            return string.Format("{0:F2} ~ {1:F2}", cfg.DamageRatioMin, cfg.DamageRatioMax);
        }
        else if (type == UI_WeaponUnitPropertyType.Damage)
        {
            var damageCount = cfg.DamageCount;
            var damage = CalculteWeaponDamage(cfg);

            if(damageCount > 1)
            {
                return string.Format("{0} X{1}", damage, damageCount);
            }
            else
            {
                return string.Format("{0}", damage);
            }
        }
        else if(type == UI_WeaponUnitPropertyType.Range)
        {
            var rangerow = propertyData.GetPropertyFinal(PropertyModifyKey.WeaponRange);
            int range = Mathf.RoundToInt(rangerow + cfg.BaseRange);
            return range.ToString();
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
        return Mathf.RoundToInt(rowDamage);
    }
}
