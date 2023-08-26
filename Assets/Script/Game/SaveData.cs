using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局存档
/// </summary>
[System.Serializable]
public class GlobalSaveData
{
    public List<AchievementSaveData> AchievementSaveData;
    public List<GameStatisticsSaveData> StatisticsSaveData;

    public static GlobalSaveData GenerateNewSaveData()
    {
        GlobalSaveData data = new GlobalSaveData();
        ///Create Achievement
        data.AchievementSaveData = new List<AchievementSaveData>();
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

        return data;
    }

    public void CombineAchievementSaves()
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

    public void Save()
    {
        StatisticsSaveData = AchievementManager.Instance.GenerateGameStatisticsSaveData();
    }
}

/// <summary>
/// 局内存档
/// </summary>
[System.Serializable]
public class SaveData 
{
    public int SaveIndex;

    public int newRecord;
    public string date;

    public string SaveName;
    public int GameTime;
    public int ModeID;

    public SaveData(int saveIndex)
    {
        this.SaveIndex = saveIndex;
        date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    }
}

public class ShipMapData
{
    public ChunkPartMapInfo[,] ShipMap = new ChunkPartMapInfo[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
    public List<UnitInfo> UnitList = new List<UnitInfo>();

    public ShipMapData(PlayerShip shipdata)
    {

        
        for (int row = 0; row < shipdata.ChunkMap.GetLength(0); row++)
        {
            for (int colume = 0; colume < shipdata.ChunkMap.GetLength(1); colume++)
            {
                ChunkPartMapInfo tempinfo = new ChunkPartMapInfo();
                tempinfo.shipCoord = shipdata.ChunkMap[row, colume].shipCoord;
                tempinfo.isOccupied = shipdata.ChunkMap[row, colume].isOccupied;
                tempinfo.isBuildingPiovt = shipdata.ChunkMap[row, colume].isBuildingPiovt;

                if(shipdata.ChunkMap[row, colume].GetType() == typeof (Core))
                {
                    tempinfo.type = ChunkType.Core;
                }
                else
                {
                    tempinfo.type = ChunkType.Base;
                }
                tempinfo.state = shipdata.ChunkMap[row, colume].state;

                ShipMap[row, colume] = tempinfo;
            }
        }

        UnitList.Clear();
        for (int i = 0; i < shipdata.UnitList.Count; i++)
        {
            UnitList.Add(new UnitInfo(shipdata.UnitList[i] as Building));
        }

    }


    public ShipMapData(int[,] shipmap)
    {
        
        for (int row = 0; row < shipmap.GetLength(0); row++)
        {
            for (int colume = 0; colume < shipmap.GetLength(1); colume++)
            {

                if(shipmap[row,colume] == 0)
                {
                    continue;
                }
                ChunkPartMapInfo tempinfo = new ChunkPartMapInfo();
                tempinfo.shipCoord = GameHelper.CoordinateArrayToMap(new Vector2Int(row, colume), GameGlobalConfig.ShipMapSize);
                tempinfo.isOccupied = false;
                tempinfo.isBuildingPiovt = false;

                if (shipmap[row, colume] == 1)
                {
                    tempinfo.type = ChunkType.Core;
                }
                if(shipmap[row, colume] == 2)
                {
                    tempinfo.type = ChunkType.Base;
                }
                tempinfo.state = DamagableState.Normal;

                ShipMap[row, colume] = tempinfo;
            }
        }

    }

}
