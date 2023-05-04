

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
