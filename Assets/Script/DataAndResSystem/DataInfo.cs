﻿using System.Collections;
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


public class BulletData : DataInfo
{
    public string PrefabPath;
    public BulletData() { }

    public BulletData(string[] row)
    {
        int.TryParse(row[0], out ID);
        Name = row[1];
        PrefabPath = row[2];
    }

    public override void Initialization(string[] row)
    {
        int.TryParse(row[0], out ID);
        Name = row[1];
        PrefabPath = row[2];
    }
}




