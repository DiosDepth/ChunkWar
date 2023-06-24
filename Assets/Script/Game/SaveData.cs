

using System.Collections.Generic;

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
    public List<BuildingMapInfo> BuildingList = new List<BuildingMapInfo>();

    public RuntimeData(Ship shipdata)
    {
        physicalResources = shipdata.physicalResources;
        energyResources = shipdata.energyResources;
        artifacts = shipdata.artifacts;
    }

}
