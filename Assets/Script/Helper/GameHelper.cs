using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using System.Text;

public static class GameHelper 
{
    public static string GeneralButton_Back_Text = "UI_General_Back";

    public static string Bool_True_Text = "Bool_True_Text";
    public static string Bool_False_Text = "Bool_False_Text";

    public static string ItemTagTextTitle = "ItemTag_";

    public static string Color_White_Code = "#FFFFFF";
    public static string Color_Red_Code = "#C61616";
    public static string Color_Blue_Code = "#09B3CB";

    private static Color RarityColor_T1 = Color.white;
    private static Color RarityColor_T2 = new Color(0, 0.83f, 1f);
    private static Color RarityColor_T3 = new Color(0.73f, 0f, 1f);
    private static Color RarityColor_T4 = new Color(1f, 0.57f, 0.23f);

    private static string RarityColor_SBG_T1 = "Sprite/General/ItemRarity_SBG_Tier1";
    private static string RarityColor_SBG_T2 = "Sprite/General/ItemRarity_SBG_Tier2";
    private static string RarityColor_SBG_T3 = "Sprite/General/ItemRarity_SBG_Tier3";
    private static string RarityColor_SBG_T4 = "Sprite/General/ItemRarity_SBG_Tier4";
    private static string RarityColor_BBG_T1 = "Sprite/General/ItemRarity_BBG_Tier1";
    private static string RarityColor_BBG_T2 = "Sprite/General/ItemRarity_BBG_Tier2";
    private static string RarityColor_BBG_T3 = "Sprite/General/ItemRarity_BBG_Tier3";
    private static string RarityColor_BBG_T4 = "Sprite/General/ItemRarity_BBG_Tier4";
    private static string ShipLevelUpRarity_Frame_T1 = "Sprite/General/ShipLevelUpItem_Frame_Tier1";
    private static string ShipLevelUpRarity_Frame_T2 = "Sprite/General/ShipLevelUpItem_Frame_Tier2";
    private static string ShipLevelUpRarity_Frame_T3 = "Sprite/General/ShipLevelUpItem_Frame_Tier3";
    private static string ShipLevelUpRarity_Frame_T4 = "Sprite/General/ShipLevelUpItem_Frame_Tier4";



    private struct TempDropRandomItem : RandomObject
    {
        public int Weight { get; set; }
        public GoodsItemRarity Rarity;
    }


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

    /// <summary>
    /// 获取TAG名称
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static string GetItemTagName(ItemTag tag)
    {
        var tagStr = ItemTagTextTitle + tag.ToString();
        return LocalizationManager.Instance.GetTextValue(tagStr);
    }

    public static Sprite GetRarityBGSprite(GoodsItemRarity rarity)
    {
        switch (rarity)
        {
            case GoodsItemRarity.Tier1:
                return ResManager.Instance.Load<Sprite>(RarityColor_SBG_T1);
            case GoodsItemRarity.Tier2:
                return ResManager.Instance.Load<Sprite>(RarityColor_SBG_T2);
            case GoodsItemRarity.Tier3:
                return ResManager.Instance.Load<Sprite>(RarityColor_SBG_T3);
            case GoodsItemRarity.Tier4:
                return ResManager.Instance.Load<Sprite>(RarityColor_SBG_T4);
            default:
                return null;
        }
    }

    public static Sprite GetRarityBG_Big(GoodsItemRarity rarity)
    {
        switch (rarity)
        {
            case GoodsItemRarity.Tier1:
                return ResManager.Instance.Load<Sprite>(RarityColor_BBG_T1);
            case GoodsItemRarity.Tier2:
                return ResManager.Instance.Load<Sprite>(RarityColor_BBG_T2);
            case GoodsItemRarity.Tier3:
                return ResManager.Instance.Load<Sprite>(RarityColor_BBG_T3);
            case GoodsItemRarity.Tier4:
                return ResManager.Instance.Load<Sprite>(RarityColor_BBG_T4);
            default:
                return null;
        }
    }

    public static Sprite GetShipLevelUpRarityFrameSprite(GoodsItemRarity rarity)
    {
        switch (rarity)
        {
            case GoodsItemRarity.Tier1:
                return ResManager.Instance.Load<Sprite>(ShipLevelUpRarity_Frame_T1);
            case GoodsItemRarity.Tier2:
                return ResManager.Instance.Load<Sprite>(ShipLevelUpRarity_Frame_T2);
            case GoodsItemRarity.Tier3:
                return ResManager.Instance.Load<Sprite>(ShipLevelUpRarity_Frame_T3);
            case GoodsItemRarity.Tier4:
                return ResManager.Instance.Load<Sprite>(ShipLevelUpRarity_Frame_T4);
            default:
                return null;
        }
    }

    /// <summary>
    /// 获取敌人掉落稀有度
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    public static GoodsItemRarity GetEnemyShipDropUnitRarity(BaseShipConfig cfg)
    {
        List<TempDropRandomItem> ranItems = new List<TempDropRandomItem>();
        var rarityDic = cfg.UnitDropRate;
        foreach(var item in rarityDic)
        {
            ///Luck & HardLevel modify
            TempDropRandomItem temp = new TempDropRandomItem
            {
                Rarity = item.Key,
                Weight = (int)item.Value
            };
            ranItems.Add(temp);
        }

        var ranResult = Utility.GetRandomList<TempDropRandomItem>(ranItems, 1);
        return ranResult[0].Rarity;
    }

    #region Battle

    /// <summary>
    /// 获取敌人当前hardLevel信息
    /// </summary>
    /// <param name="groupID"></param>
    /// <returns></returns>
    public static EnemyHardLevelItem GetEnemyHardLevelItem(int groupID)
    {
        var currentHardLevel = RogueManager.Instance.GetHardLevelValueIndex;
        return DataManager.Instance.GetEnemyHardLevelItem(groupID, currentHardLevel);
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
            UnityEngine.Debug.Log("EXP MAP ERROR! LEVEL = " + level);
            return int.MaxValue;
        }
        return expMap[level];
    }

    /// <summary>
    /// 计算掉落概率
    /// </summary>
    /// <param name="baseRate"></param>
    /// <returns></returns>
    public static float CalculateDropRate(float baseRate)
    {
        var luck = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.Luck);
        return baseRate * (100 + luck) / 100f;
    }

    /// <summary>
    /// 生成掉落信息
    /// </summary>
    /// <returns></returns>
    public static Dictionary<PickUpData, int> GeneratePickUpdata(float dropCount)
    {
        Dictionary<PickUpData, int> result = new Dictionary<PickUpData, int>();
        var allpickUpWaste = DataManager.Instance.GetAllWastePickUpData();
        int safeLoop = 0;

        Action<PickUpData, int> addDrop = (drop, count) =>
        {
            if (result.ContainsKey(drop))
            {
                result[drop]++;
            }
            else
            {
                result.Add(drop, 1);
            }
        };

        while(dropCount >= 0)
        {
            safeLoop++;
            if (safeLoop > 100)
                break;

            ///DropCount不足1的部分概率掉落
            if(dropCount < 1)
            {
                bool drop = Utility.RandomResultWithOne(0, dropCount);
                if (drop)
                {
                    addDrop(allpickUpWaste[0], 1);
                }
            }

            ///从大到小计算掉落物大小以及数量
            for (int i = allpickUpWaste.Count - 1; i >= 0; i--)  
            {
                var wasteInfo = allpickUpWaste[i];
                if(dropCount >= wasteInfo.CountRef)
                {
                    dropCount -= wasteInfo.CountRef;
                    addDrop(wasteInfo, wasteInfo.CountRef);
                    break;
                }
            }
        }
        return result;
    }

    public static Vector2 GetPlayerShipPosition()
    {
        var currentShip = RogueManager.Instance.currentShip;
        if (currentShip == null)
            return Vector2.zero;

        return currentShip.transform.position;
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

        return currentShip.CoreUnits[0].baseAttribute;
    }

    public static GeneralHPComponet GetPlayerShipHPComponet()
    {
        var currentShip = RogueManager.Instance.currentShip;
        if (currentShip == null)
            return null;

        return currentShip.CoreUnits[0].HpComponent;
    }

    /// <summary>
    /// 单位能源消耗
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    public static float GetUnitEnergyCost(BaseUnitConfig cfg)
    {
        if (cfg.unitType == UnitType.MainWeapons)
            return 0f;

        var energyCostBase = cfg.BaseEnergyCost;
        var costModify = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.WeaponEnergyCostPercent);
        float modify = 0;
        ///ShieldModify
        if (cfg.HasUnitTag(ItemTag.Shield))
        {
            modify = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.ShieldEnergyCostPercent);
        }
        else if (cfg.HasUnitTag(ItemTag.Weapon) || cfg.HasUnitTag(ItemTag.MainWeapon)) 
        {
            modify = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.WeaponEnergyCostPercent);
        }

        var totalUnitModify = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.UnitEnergyCostPercent);

        var cost = Mathf.Clamp(energyCostBase * (1 + (costModify + modify + totalUnitModify) / 100f), 0, float.MaxValue);
        return cost;
    }
    
    /// <summary>
    /// 单位负载消耗
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    public static float GetUnitLoadCost(BaseUnitConfig cfg)
    {
        if (cfg.unitType == UnitType.MainWeapons)
            return 0f;

        var wreckageCfg = DataManager.Instance.GetWreckageDropItemConfig(cfg.ID);
        if (wreckageCfg == null)
            return 0;

        var loadCost = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.UnitLoadCost);
        return Mathf.Clamp(wreckageCfg.LoadCost * (1 + loadCost / 100f), 0, float.MaxValue);
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
    /// 获取Unit距离
    /// </summary>
    /// <param name="target"></param>
    /// <param name="attackerUnit"></param>
    /// <returns></returns>
    public static float GetUnitDistance(IDamageble target, Unit attackerUnit)
    {
        if (attackerUnit == null || target == null)
            return 0;

        if(target is Unit)
        {
            return Vector2.Distance((target as Unit).transform.position, attackerUnit.transform.position);
        }
        return 0;
    }

    /// <summary>
    /// 所有存档ID
    /// </summary>
    /// <returns></returns>
    public static List<uint> GetAllSaveIDs()
    {
        var allsav = SaveLoadManager.Instance.GetAllSave;
        if (allsav == null || allsav.Count <= 0)
            return new List<uint>();

        return allsav.Select(x => (uint)x.SaveIndex).ToList();
    }
    
    /// <summary>
    /// 根据阵营ID获取所有舰船
    /// </summary>
    /// <param name="campID"></param>
    /// <returns></returns>
    public static List<uint> GetAllShipsByCampID(int campID)
    {
        List<uint> result = new List<uint>();
        var allShip = DataManager.Instance.GetAllShipConfigs();
        for (int i = 0; i < allShip.Count; i++) 
        {
            if(allShip[i].PlayerShipCampID == campID)
            {
                result.Add((uint)allShip[i].ID);
            }
        }
        return result;
    }

    public static CampConfig GetCampCfgByShipID(int shipID)
    {
        var shipCfg = DataManager.Instance.GetShipConfig(shipID);
        if (shipCfg == null)
            return null;

        return DataManager.Instance.GetCampConfigByID(shipCfg.PlayerShipCampID);
    }

    /// <summary>
    /// 获取成就分类
    /// 排序 => ID， 解锁状态
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<uint> GetAchievementsByGroupType(AchievementGroupType type)
    {
        var allAchievement = DataManager.Instance.GetAllAchievementConfigs();
        var lst = allAchievement.FindAll(x => x.GroupType == type)
            .OrderBy(item => item.Order).ToList();
        lst.Sort();
        return lst.Select(item => (uint)item.AchievementID).ToList();
    }

    public static List<uint> GetAllCampIDs()
    {
        var allcampData = GameManager.Instance.GetAllCampData;
        return allcampData.Select(x => (uint)x.CampID).ToList();
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

    public static List<uint> GetAllHardLevelsByShipID(int shipID)
    {
        var shipCfg = DataManager.Instance.GetShipConfig(shipID);
        if (shipCfg == null)
            return new List<uint>();

        return shipCfg.ShipHardLevels.Select(x => (uint)x).ToList();
    }

    public static List<uint> GetRogueShipPlugItems()
    {
        var allItems = RogueManager.Instance.AllCurrentShipPlugs;
        List<uint> goods = new List<uint>();
        for (int i = 0; i < allItems.Count; i++) 
        {
            if (!goods.Contains((uint)allItems[i].PlugID))
            {
                goods.Add((uint)allItems[i].PlugID);
            }
        }

        return goods;
    }

    public static List<uint> GetCurrentWreckageItems()
    {
        List<uint> result = new List<uint>();
        ///Waste
        if (RogueManager.Instance.GetDropWasteCount > 0)
        {
            result.Add(0);
        }
        
        var allItems = RogueManager.Instance.CurrentWreckageItems;
        result.AddRange( allItems.Values.Select(x => x.UID).ToList());
        return result;
    }

    public static List<uint> GetAllMainWeaponItems()
    {
        List<uint> result = new List<uint>();
        var allUnits = DataManager.Instance.GetAllUnitConfigs();
        for(int i = 0; i < allUnits.Count; i++)
        {
            if(allUnits[i].unitType == UnitType.MainWeapons)
            {
                result.Add((uint)allUnits[i].ID);
            }
        }
        result.Sort();
        return result;
    }

    /// <summary>
    /// 成就组进度
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static float GetAchievementGroupProgress(AchievementGroupType type)
    {
        int unlockCount = 0;
        var allAchievement = DataManager.Instance.GetAllAchievementConfigs();
        var items = allAchievement.FindAll(x => x.GroupType == type);
        if (items == null || items.Count <= 0)
            return 0;

        for(int i = 0; i < items.Count; i++)
        {
            if(SaveLoadManager.Instance.GetAchievementUnlockState(items[i].AchievementID))
            {
                unlockCount++;
            }
        }

        return unlockCount / (float)items.Count;
    }

    public static string GetTimeStringBySeconds(int time)
    {
        int hour = time / 3600;
        int minute = time % 3600 / 60;
        int second = time % 3600 % 60;

        return string.Format("{0:D2}-{1:D2}-{2:D2}", hour, minute, second);
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
    /// 计算伤害，含修正
    /// </summary>
    /// <param name="baseDamage"></param>
    /// <param name="modify"></param>
    /// <returns></returns>
    public static int CalculatePlayerDamageWithModify(int baseDamage, List<UnitPropertyModifyFrom> modify, out bool critical)
    {
        var cirticalRatio = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.Critical);
        critical = Utility.RandomResult(0, cirticalRatio);

        float damage = baseDamage;
        for (int i = 0; i < modify.Count; i++)
        {
            var item = modify[i];
            var value = RogueManager.Instance.MainPropertyData.GetPropertyFinal(item.PropertyKey);
            damage += value * item.Ratio;
        }

        var damagePercent = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.DamagePercent);
        damage = damage * (1 + damagePercent / 100f);
        damage = Mathf.Clamp(damage, 0, damage);
        return Mathf.RoundToInt(damage);
    }

    /// <summary>
    /// 玩家受到伤害
    /// </summary>
    /// <param name="info"></param>
    public static void ResolvePlayerUnitDamage(DamageResultInfo info)
    {
        ///计算闪避
        var parry = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.ShipParry);
        parry = Mathf.Clamp(parry, 0, 100);
        bool isParry = Utility.RandomResult(0, parry);
        info.IsHit = !isParry;

        if (!isParry)
        {
            var armor = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.ShipArmor);
            var armorParam = DataManager.Instance.battleCfg.PlayerShip_ArmorDamageReduce_Param;
            armorParam = Mathf.Clamp(armorParam, 1, float.MaxValue);
            float DamageTake = 1 / (float)(1 + armor / armorParam);
            info.Damage = Mathf.RoundToInt(info.Damage * DamageTake);
        }
    }

    /// <summary>
    /// 玩家受到护盾伤害
    /// </summary>
    /// <param name="info"></param>
    public static void ResolvePlayerShieldDamage(DamageResultInfo info)
    {
        var armor = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.ShieldArmor);
        var armorParam = DataManager.Instance.battleCfg.PlayerShip_ShieldDamageReduce_Param;
        armorParam = Mathf.Clamp(armorParam, 1, float.MaxValue);
        float DamageTake = 1 / (float)(1 + armor / armorParam);
        info.Damage = Mathf.RoundToInt(info.Damage * DamageTake);
        ///护盾最低无视伤害
        var ignoreDamage = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.ShieldIgnoreMinDamage);
        if (ignoreDamage <= 0)
            return;

        info.Damage = info.Damage > ignoreDamage ? info.Damage : 0;

    }

    /// <summary>
    /// 计算残骸出售价格
    /// </summary>
    /// <param name="wasteCount"></param>
    /// <returns></returns>
    public static int CalculateWasteSellPrice(int wasteCount)
    {
        var sellAdd = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.WasteSellPriceAdd);
        float sellPercent = Mathf.Clamp(sellAdd / 100f + 1, 0, float.MaxValue);
        return Mathf.RoundToInt(wasteCount * sellPercent);
    }
   
    /// <summary>
    /// 获取爆炸范围
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public static float CalculateExplodeRange(int range)
    {
        var rangeRatio = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.Explode_Range);
        var newRange = range * (1 + rangeRatio / 100f);
        newRange = Mathf.Max(0, newRange);
        return newRange / 10f;
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

    /// <summary>
    /// 获取Unit出售价格
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public static int GetUnitSellPrice(Unit unit)
    {
        var wreckageCfg = DataManager.Instance.GetWreckageDropItemConfig(unit.UnitID);
        if (wreckageCfg == null)
            return 0;

        var sellPriceAdd = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.SellPrice);
        var price = Mathf.Clamp(wreckageCfg.SellPrice * (1 + sellPriceAdd / 100f), 0, float.MaxValue);
        return Mathf.CeilToInt(price);
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
            var ratioModifyMin = propertyData.GetPropertyFinal(PropertyModifyKey.DamageRangeMin);
            var ratioModifyMax = propertyData.GetPropertyFinal(PropertyModifyKey.DamageRangeMax);

            float ratioMin = ratioModifyMin / 100f + cfg.DamageRatioMin;
            ratioMin = Mathf.Max(0, ratioMin);

            float ratioMax = ratioModifyMax / 100f + cfg.DamageRatioMax;

            return string.Format("{0:F2} ~ {1:F2}", ratioMin, ratioMax);
        }
        else if (type == UI_WeaponUnitPropertyType.Damage)
        {
            var damageCount = cfg.TotalDamageCount;
            var damage = CalculteWeaponDamage(cfg);
            string color = GetColorCode(damage, cfg.DamageBase, false);

            StringBuilder sb = new StringBuilder();
            ///伤害图文混排
            sb.Append(string.Format("<color={0}>{1}</color> ", color, damage));

            if(cfg.DamageModifyFrom != null && cfg.DamageModifyFrom.Count > 0)
            {
                sb.Append("[");
                for (int i = 0; i < cfg.DamageModifyFrom.Count; i++)
                {
                    var modifyFrom = cfg.DamageModifyFrom[i];
                    var proeprtyDisplayCfg = DataManager.Instance.battleCfg.GetPropertyDisplayConfig(modifyFrom.PropertyKey);
                    if(proeprtyDisplayCfg != null)
                    {
                        if(modifyFrom.Ratio == 1)
                        {
                            sb.Append(string.Format("{0}%<sprite={1}>", modifyFrom.Ratio * 100, proeprtyDisplayCfg.TextSpriteIndex));
                        }
                        else
                        {
                            sb.Append(string.Format("<sprite={0}>", proeprtyDisplayCfg.TextSpriteIndex));
                        }
                    }
                }
                sb.Append("]");
            }

            if (damageCount > 1)
            {
                sb.Append(string.Format(" X{0}", damageCount));
                return sb.ToString();
            }
            else
            {
                return sb.ToString();
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
            shieldDamage = Mathf.CeilToInt(shieldDamage);
            string damageColor = GetColorCode(shieldDamage, cfg.ShieldDamage, false);
            return string.Format("<color={0}>{1}%</color>", damageColor, shieldDamage);
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

    public static string GetColorCode(float value1, float value2, bool reverse)
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

    /// <summary>
    /// refencePos 是你找目标的参考点，
    /// radius 是参考周边范围，
    ///  targetsPos是一个NavitveArray 找寻目标的全集，可以从AIManage.Instance.aiActiveUnitPos里面取所有的unit位置作为全集
    ///  count 设置为-1 会获取所有范围内目标，否则需要指定数量
    ///  这里返回的是一个FindTargetInfo数组， 调用AIManage.Instance.GetActiveUnitReferenceByFindTargetInfo()来获取对应的Unit
    /// </summary>
    /// <param name="referencePos"></param>
    /// <param name="radius"></param>
    /// <param name="targetsPos"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static TargetInfo[] FindTargetsByPoint(Vector3 referencePos, float radius, NativeArray<float3> targetsPos, int count = -1)
    {
        //List<FindTargetInfoWithReference> result = new List<FindTargetInfoWithReference>();
        NativeQueue<TargetInfo> rv_findedTargetsInfo = new NativeQueue<TargetInfo>(Allocator.TempJob);

        JobHandle jobHandle;

        FindTargesByPoint findTargesByPoint = new FindTargesByPoint
        {
            job_selfPos = referencePos,
            job_radius = radius,
            job_targetsPos = targetsPos,

            rv_findedTargetsInfo = rv_findedTargetsInfo.AsParallelWriter(),
        };
        jobHandle = findTargesByPoint.ScheduleBatch(targetsPos.Length, 2);
        jobHandle.Complete();

        NativeArray<TargetInfo> targetsinfo = rv_findedTargetsInfo.ToArray(Allocator.TempJob);
        targetsinfo.Sort();

        if(targetsinfo.Length >= count && count != -1)
        {
            targetsinfo.Slice(0, count);
        }


        //FindTargetInfoWithReference referenceinfo;
        //for (int i = 0; i < targetsinfo.Length; i++)
        //{
        //    referenceinfo.info = targetsinfo[i];
        //    referenceinfo.refrence = AIManager.Instance.aiActiveUnitList[targetsinfo[i].index];
        //    result.Add(referenceinfo);
        //}

        TargetInfo[] result = targetsinfo.ToArray();

        targetsinfo.Dispose();
        rv_findedTargetsInfo.Dispose();

        return result;
    }

    public struct TargetInfo : IComparable<TargetInfo>
    {
        public int index;
        public float distance;
        public float3 pos;

        public int CompareTo(TargetInfo obj)
        {
            return this.distance.CompareTo(obj.distance);
        }
    }

    public struct FindTargesByPoint : IJobParallelForBatch
    {
        [Unity.Collections.ReadOnly] public float3 job_selfPos;
        [Unity.Collections.ReadOnly] public float job_radius;
        [Unity.Collections.ReadOnly] public NativeArray<float3> job_targetsPos;

        [NativeDisableContainerSafetyRestriction]
        public NativeQueue<TargetInfo>.ParallelWriter rv_findedTargetsInfo;

        private float distance;
        private TargetInfo info;
        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; i++)
            {
                distance = math.distance(job_selfPos, job_targetsPos[i]);
                if (distance <= job_radius)
                {
                    info.index = i;
                    info.distance = distance;
                    info.pos = job_targetsPos[i];
                    rv_findedTargetsInfo.Enqueue(info);
                }
            }
        }
    }

}
