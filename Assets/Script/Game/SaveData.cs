

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData 
{
    public int newRecord;
    public string date;





    public SaveData()
    {
        newRecord = 0;
        date = System.DateTime.Now.ToString("yyyy:mm:dd");
    }
}

public class RuntimeData
{
    public int physicalResources;
    public int energyResources;
    public List<string> artifacts;

    public ChunkPartMapInfo[,] ShipMap = new ChunkPartMapInfo[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
    public List<UnitInfo> BuildingList = new List<UnitInfo>();

    public RuntimeData(Ship shipdata)
    {
        physicalResources = shipdata.physicalResources;
        energyResources = shipdata.energyResources;
        artifacts = shipdata.artifacts;

        
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

        BuildingList.Clear();
        for (int i = 0; i < shipdata.UnitList.Count; i++)
        {
            BuildingList.Add(new UnitInfo(shipdata.UnitList[i] as Building));
        }

    }


    public RuntimeData(int[,] shipmap)
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
