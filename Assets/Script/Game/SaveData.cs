using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局存档
/// </summary>
[System.Serializable]
public class GlobalSaveData
{
    public List<AchievementSaveData> AchievementSaveData = new List<AchievementSaveData>();
    public List<GameStatisticsSaveData> StatisticsSaveData = new List<GameStatisticsSaveData>();
    public List<ShipSaveData> ShipSaveData = new List<ShipSaveData>();
    public List<CampSaveData> CampSaveData = new List<CampSaveData>();
    public List<UnitSaveData> UnitSaveDatas = new List<UnitSaveData>();

    public static GlobalSaveData GenerateNewSaveData()
    {
        GlobalSaveData data = new GlobalSaveData();
        ///Create Achievement
        var allAchievement = DataManager.Instance.GetAllAchievementConfigs();

        for (int i = 0; i < allAchievement.Count; i++) 
        {
            var achievementItem = allAchievement[i];
            AchievementSaveData aSav = new AchievementSaveData();
            aSav.AchievementID = achievementItem.AchievementID;
            aSav.Unlock = false;
            aSav.FinishTime = string.Empty;

            data.AchievementSaveData.Add(aSav);
        }

        var allShips = DataManager.Instance.GetAllShipConfigs();
        for(int i = 0; i < allShips.Count; i++)
        {
            var shipCfg = allShips[i];
            var shipSav = new ShipSaveData();
            shipSav.ShipID = shipCfg.ID;
            shipSav.Unlock = shipCfg.UnlockDefault;
            shipSav.InitHardLevelSave();
            data.ShipSaveData.Add(shipSav);
        }

        var allCamps = DataManager.Instance.GetAllCampConfigs();
        for(int i = 0; i < allCamps.Count; i++)
        {
            var camp = allCamps[i];
            var campSav = new CampSaveData();
            campSav.CampID = camp.CampID;
            campSav.Unlock = camp.Unlock;
            data.CampSaveData.Add(campSav);
        }

        var allUnits = DataManager.Instance.GetAllUnitConfigs();
        for(int i = 0; i < allUnits.Count; i++)
        {
            var unit = allUnits[i];
            var unitSav = new UnitSaveData();
            unitSav.UnitID = unit.ID;
            unitSav.Unlock = unit.UnlockDefault;
            data.UnitSaveDatas.Add(unitSav);
        }

        return data;
    }

    public void CombineSaveData()
    {
        CombineAchievementSaves();
        CombineShipSaveData();
        CombineUnitSaveData();
    }

    #region SetData

    public void SetShipUnlock(int shipID)
    {
        var shipSav = ShipSaveData.Find(x => x.ShipID == shipID);
        if(shipSav != null && !shipSav.Unlock )
        {
            shipSav.Unlock = true;
            Debug.Log("Unlock Ship , ID = " + shipID);
        }
    }

    public void SetUnitUnlock(int unitID)
    {
        var unitSav = UnitSaveDatas.Find(x => x.UnitID == unitID);
        if(unitSav != null && !unitSav.Unlock)
        {
            unitSav.Unlock = true;
        }
    }

    public bool GetShipUnlockState(int shipID)
    {
        var shipSav = ShipSaveData.Find(x => x.ShipID == shipID);
        if (shipSav != null )
        {
            return shipSav.Unlock;
        }
        return false;
    }

    public bool GetUnitUnlockState(int unitID)
    {
        var unitSav = UnitSaveDatas.Find(x => x.UnitID == unitID);
        if (unitSav != null)
        {
            return unitSav.Unlock;
        }
        return false;
    }

    /// <summary>
    /// 获取难度存档
    /// </summary>
    /// <param name="shipID"></param>
    /// <param name="hardLevelID"></param>
    /// <returns></returns>
    public ShipHardLevelSave GetShipHardLevelSaveData(int shipID, int hardLevelID)
    {
        var shipSav = ShipSaveData.Find(x => x.ShipID == shipID);
        if (shipSav != null)
            return shipSav.HardLevelSaves.Find(x => x.HardLevelID == hardLevelID);

        return null;
    }

    #endregion

    private void CombineAchievementSaves()
    {
        var allAchievement = DataManager.Instance.GetAllAchievementConfigs();
        for (int i = 0; i < allAchievement.Count; i++)
        {
            var achievementItem = allAchievement[i];
            ///Exists
            if (AchievementSaveData.Find(x => x.AchievementID == achievementItem.AchievementID) != null)
                continue;

            ///Add New
            AchievementSaveData aSav = new AchievementSaveData();
            aSav.AchievementID = achievementItem.AchievementID;
            aSav.Unlock = false;
            aSav.FinishTime = string.Empty;

            AchievementSaveData.Add(aSav);
        }
    }

    private void CombineShipSaveData()
    {
        var allShips = DataManager.Instance.GetAllShipConfigs();
        for(int i = 0; i < allShips.Count; i++)
        {
            var shipItem = allShips[i];

            ///Try CombineShipHardLevelCfg
            var shipSav = ShipSaveData.Find(x => x.ShipID == shipItem.ID);
            if (shipSav != null)
            {
                CombineShipHardLevelSave(ref shipSav);
                continue;
            }

            ShipSaveData newSav = new ShipSaveData();
            newSav.ShipID = shipItem.ID;
            newSav.Unlock = shipItem.UnlockDefault;
            newSav.InitHardLevelSave();
            ShipSaveData.Add(newSav);
        }
    }

    private void CombineUnitSaveData()
    {
        var allUnit = DataManager.Instance.GetAllUnitConfigs();
        for(int i = 0; i < allUnit.Count; i++)
        {
            var unit = allUnit[i];

            var unitSav = UnitSaveDatas.Find(x => x.UnitID == unit.ID);
            if (unitSav != null)
                continue;

            UnitSaveData newSav = new UnitSaveData();
            newSav.UnitID = unit.ID;
            newSav.Unlock = unit.UnlockDefault;
            UnitSaveDatas.Add(newSav);
        }
    }

    /// <summary>
    /// 合并难度存档
    /// </summary>
    /// <param name="sav"></param>
    private void CombineShipHardLevelSave(ref ShipSaveData sav)
    {
        var shipCfg = DataManager.Instance.GetShipConfig(sav.ShipID);
        if (shipCfg == null)
            return;

        for (int i = 0; i < shipCfg.ShipHardLevels.Count; i++)
        {
            var level = shipCfg.ShipHardLevels[i];
            var hardLevelSav = sav.HardLevelSaves.Find(x => x.HardLevelID == level);
            if (hardLevelSav != null)
                continue;

            var hardLevelCfg = DataManager.Instance.battleCfg.GetHardLevelConfig(level);
            if (hardLevelCfg != null)
            {
                ShipHardLevelSave newSav = new ShipHardLevelSave();
                newSav.HardLevelID = level;
                newSav.Unlock = hardLevelCfg.DefaultUnlock;
                sav.HardLevelSaves.Add(newSav);
            }
        }
    }

    public void Save()
    {
        StatisticsSaveData = AchievementManager.Instance.GenerateGameStatisticsSaveData();
        CampSaveData = GameManager.Instance.CreateCampSaveDatas();
    }

    public void Load()
    {

    }
}

/// <summary>
/// 局内存档
/// </summary>
[System.Serializable]
public class SaveData 
{
    public int SaveIndex;
    public string date;

    public string SaveName;
    public int GameTime;
    public int ModeID;
    public int ShipID;
    public int MainWeaponID;

    public int WaveIndex;
    public byte ShopTotalEnterCount;

    public int Currency;
    public int WasteCount;
    public byte ShipLevel;
    public float EXP;

    public List<int> EliteSpawnWaves;
    public List<int> AlreadySpawnedEliteIDs;

    /* Unit Save */
    public List<ShipUnitRuntimeSaveData> ShipUnitRuntimeDatas;

    /* Plug Save */
    public List<PlugRuntimeSaveData> PlugRuntimeSaves;
    public Dictionary<PropertyModifyKey, float> PropertyRowSav;

    public SaveData(int saveIndex)
    {
        this.SaveIndex = saveIndex;
        date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    }
}

[System.Serializable]
public class PlugRuntimeSaveData
{
    public uint UID;
    public int ID;
    public int GoodsID;

    public List<int> TriggerCountSav;
}


public class ShipSaveData
{
    public int ShipID;
    public bool Unlock;
    public List<ShipHardLevelSave> HardLevelSaves;

    public void InitHardLevelSave()
    {
        HardLevelSaves = new List<ShipHardLevelSave>();
        var shipCfg = DataManager.Instance.GetShipConfig(ShipID);
        if (shipCfg == null)
            return;

        for (int i = 0; i < shipCfg.ShipHardLevels.Count; i++) 
        {
            var level = shipCfg.ShipHardLevels[i];
            var hardLevelCfg = DataManager.Instance.battleCfg.GetHardLevelConfig(level);
            if(hardLevelCfg != null)
            {
                ShipHardLevelSave sav = new ShipHardLevelSave();
                sav.HardLevelID = level;
                sav.Unlock = hardLevelCfg.DefaultUnlock;
                HardLevelSaves.Add(sav);
            }
        }
    }
}

public class ShipHardLevelSave
{
    public int HardLevelID;
    public bool Unlock;
    public bool Finish;
}

public class UnitSaveData
{
    public int UnitID;
    public bool Unlock;
}

public class ShipUnitRuntimeSaveData
{
    public int UnitID;
    public int PosX;
    public int PosY;
    public uint UID;
}

