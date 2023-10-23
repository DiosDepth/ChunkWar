using System.Collections;
using System.Collections.Generic;


public abstract class DataInfo
{
    public int ID;
    public string Name;
    public DataInfo() { }
    public abstract void Initialization(string[] row);
}

public class TestaDataInfo : DataInfo
{

    public string Path;

    public TestaDataInfo() { }
    public TestaDataInfo(string[] row)
    {
        int.TryParse(row[0], out ID);
        Name = row[1];
        Path = row[2];
    }

    public override void Initialization(string[] row)
    {
        int.TryParse(row[0], out ID);
        Name = row[1];
        Path = row[2];
    }
}

public class SoundDataInfo:DataInfo
{
    public string SoundSourcePath;

    public SoundDataInfo() { }
    public SoundDataInfo(string[] row)
    {
        int.TryParse(row[0], out ID);
        Name = row[1];
        SoundSourcePath = row[2];
    }

    public override void Initialization(string[] row)
    {
        int.TryParse(row[0], out ID);
        Name = row[1];
        SoundSourcePath = row[2];
    }
}

public class LevelData : DataInfo
{
    public string LevelPrefabPath;
    public LevelData() { }

    public LevelData(string[] row)
    {
        int.TryParse(row[0], out ID);
        Name = row[1];
        LevelPrefabPath = row[2];
    }

    public override void Initialization(string[] row)
    {
        int.TryParse(row[0], out ID);
        Name = row[1];
        LevelPrefabPath = row[2];
    }
}

public class EnemyHardLevelItem : DataInfo
{
    public int GroupID;
    public float ATKPercentAdd;
    public float HPpercentAdd;
    public float ShieldHPPercentAdd;
    public float MissileHPPercentAdd;

    public override void Initialization(string[] row)
    {
        int.TryParse(row[0], out ID);
        int.TryParse(row[1], out GroupID);
        float.TryParse(row[2], out ATKPercentAdd);
        float.TryParse(row[3], out HPpercentAdd);
        float.TryParse(row[4], out ShieldHPPercentAdd);
    }
}


public class PickUpData : DataInfo
{
    public string PrefabPath;

    public AvaliablePickUp Type;
    public GoodsItemRarity Rarity;

    /// <summary>
    /// 用于掉落物计算参数
    /// </summary>
    public byte CountRef;
    public float EXPAdd;

    public PickUpData() { }

    public PickUpData(string[] row)
    {
        int.TryParse(row[0], out ID);
        Name = row[1];
        PrefabPath = row[2];
        AvaliablePickUp.TryParse(row[3], out Type);
        GoodsItemRarity.TryParse(row[4], out Rarity);
        byte.TryParse(row[5], out CountRef);
        float.TryParse(row[6], out EXPAdd);
    }

    public override void Initialization(string[] row)
    {
        int.TryParse(row[0], out ID);
        Name = row[1];
        PrefabPath = row[2];
        AvaliablePickUp.TryParse(row[3], out Type);
        GoodsItemRarity.TryParse(row[4], out Rarity);
        byte.TryParse(row[5], out CountRef);
        float.TryParse(row[6], out EXPAdd);
    }
}



