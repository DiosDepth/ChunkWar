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





