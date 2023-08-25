using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using Sirenix.Serialization;


/// <summary>
/// Allows the save and load of objects in a specific folder and file.
/// </summary>
public class SaveLoadManager : Singleton<SaveLoadManager>
{
    private const string _baseFolderName = "/Data/";
    private const string _defaultFolderName = "PlayerSaveInfo";

    private const string globalSaveName = "GlobalSaveData";

    private List<SaveData> _allSaves;
    public List<SaveData> GetAllSave
    {
        get { return _allSaves; }
    }

    public GlobalSaveData globalSaveData;

    /// <summary>
    /// Achievement
    /// </summary>
    private Dictionary<int, AchievementSaveData> _achievementSaveDatas;

    public SaveLoadManager()
    {
        _achievementSaveDatas = new Dictionary<int, AchievementSaveData>();
    }
    public override void Initialization()
    {
        base.Initialization();
        LoadGlobalSaveData();
        var allSaves = LoadAllSaveData();
        _allSaves = allSaves;
    }

    public void AddSaveData(SaveData sav)
    {
        _allSaves.Add(sav);
    }

    public SaveData GetSaveDataByIndex(int saveIndex)
    {
        return _allSaves.Find(x => x.SaveIndex == saveIndex);
    }

    /// <summary>
    /// 获取存档Index
    /// </summary>
    /// <returns></returns>
    public int GetSaveIndex()
    {
        int newIndex = 1;
        while (ExistSaveIndex(newIndex))
        {
            newIndex++;
        }
        return newIndex;
    }


    /// <summary>
    /// 读取所有玩家存档
    /// </summary>
    /// <returns></returns>
    public List<SaveData> LoadAllSaveData()
    {
        List<SaveData> result = new List<SaveData>();
        string savePath = DetermineSavePath(_defaultFolderName);
        if (!Directory.Exists(savePath))
        {
            return result;
        }

        DirectoryInfo info = new DirectoryInfo(savePath);
        FileInfo[] files = info.GetFiles();
        for(int i = 0; i < files.Length; i++)
        {
            if (!files[i].Name.EndsWith(".sav"))
                continue;

            var bytes = File.ReadAllBytes(files[i].FullName);
            var savData = SerializationUtility.DeserializeValue<SaveData>(bytes, DataFormat.Binary);
            result.Add(savData);
        }
        return result;
    }

    /// <summary>
    /// Save
    /// </summary>
    public void SaveGlobalSaveData()
    {
        string savePath = DetermineSavePath(string.Empty);
        var fullPath = savePath + DetermineSaveFileName(globalSaveName);
        if (File.Exists(fullPath) && globalSaveData != null)
        {
            var saveBytes = SerializationUtility.SerializeValue<GlobalSaveData>(globalSaveData, DataFormat.Binary);
            using (FileStream fs = new FileStream(fullPath, FileMode.Create))
            {
                for (int i = 0; i < saveBytes.Length; i++)
                {
                    fs.WriteByte(saveBytes[i]);
                }

                fs.Seek(0, SeekOrigin.Begin);
            }
        }
    }

    /// <summary>
    /// 加载全局存档
    /// </summary>
    private void LoadGlobalSaveData()
    {
        string savePath = DetermineSavePath(string.Empty);
        var fullPath = savePath + DetermineSaveFileName(globalSaveName);
        if (!File.Exists(fullPath))
        {
            globalSaveData = GlobalSaveData.GenerateNewSaveData();
            ///Create
            var saveBytes = SerializationUtility.SerializeValue<GlobalSaveData>(globalSaveData, DataFormat.Binary);
            using (FileStream fs = new FileStream(fullPath, FileMode.Create))
            {
                for (int i = 0; i < saveBytes.Length; i++)
                {
                    fs.WriteByte(saveBytes[i]);
                }

                fs.Seek(0, SeekOrigin.Begin);
            }
        }
        else
        {
            ///Load
            var bytes = File.ReadAllBytes(fullPath);
            globalSaveData = SerializationUtility.DeserializeValue<GlobalSaveData>(bytes, DataFormat.Binary);
            Debug.Assert(globalSaveData != null, "GlobalSaveData Null!");

            InitAchievementInfo();
        }
    }


    /// <summary>
    /// Determines the save path to use when loading and saving a file based on a folder name.
    /// </summary>
    /// <returns>The save path.</returns>
    /// <param name="folderName">Folder name.</param>
    static string DetermineSavePath(string folderName = _defaultFolderName)
    {
        string savePath;
        // depending on the device we're on, we assemble the path
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            savePath = Application.persistentDataPath + _baseFolderName;
        }
        else
        {
            savePath = Application.persistentDataPath + _baseFolderName;
        }
#if UNITY_EDITOR
        savePath = Application.dataPath + _baseFolderName;
#endif

        savePath = savePath + folderName + "/";
        return savePath;
    }

    /// <summary>
    /// Determines the name of the file to save
    /// </summary>
    /// <returns>The save file name.</returns>
    /// <param name="fileName">File name.</param>
    static string DetermineSaveFileName(string fileName)
    {
        return fileName + ".sav";
    }

    /// <summary>
    /// Save the specified saveObject, fileName and foldername into a file on disk.
    /// </summary>
    /// <param name="saveObject">Save object.</param>
    /// <param name="fileName">File name.</param>
    /// <param name="foldername">Foldername.</param>
    public static void Save<T>(T saveObject, string fileName, string foldername = _defaultFolderName)
    {
        string savePath = DetermineSavePath(foldername);
        string saveFileName = DetermineSaveFileName(fileName);
        // if the directory doesn't already exist, we create it
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        // we serialize and write our object into a file on disk
        var bytes = SerializationUtility.SerializeValue<T>(saveObject, DataFormat.Binary);

        using(FileStream fs = new FileStream (savePath + saveFileName, FileMode.Create))
        {
            for(int i = 0; i < bytes.Length; i++)
            {
                fs.WriteByte(bytes[i]);
            }

            fs.Seek(0, SeekOrigin.Begin);
        }
        Debug.Log(fileName + " has been saved in : " + savePath);
    }

    /// <summary>
    /// Load the specified file based on a file name into a specified folder
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <param name="foldername">Foldername.</param>
    public static object Load(string fileName, string foldername = _defaultFolderName)
    {
        string savePath = DetermineSavePath(foldername);
        string saveFileName = savePath + DetermineSaveFileName(fileName);

        object returnObject;

        // if the MMSaves directory or the save file doesn't exist, there's nothing to load, we do nothing and exit
        if (!Directory.Exists(savePath) || !File.Exists(saveFileName))
        {
            Debug.Log("Can't find " + fileName);
            return null;
        }
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Open(saveFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        returnObject = formatter.Deserialize(saveFile);
        saveFile.Close();
        Debug.Log(fileName + " has been Loaded");
        return returnObject;

    }

    /// <summary>
    /// Removes a save from disk
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <param name="folderName">Folder name.</param>
    public static void DeleteSave(string fileName, string folderName = _defaultFolderName)
    {
        string savePath = DetermineSavePath(folderName);
        string saveFileName = DetermineSaveFileName(fileName);
        if (File.Exists(savePath + saveFileName))
        {
            File.Delete(savePath + saveFileName);
        }
    }

    public static void DeleteSaveFolder(string folderName = _defaultFolderName)
    {
        string savePath = DetermineSavePath(folderName);
        if (Directory.Exists(savePath))
        {
            DeleteDirectory(savePath);
        }
    }

    public static void DeleteDirectory(string target_dir)
    {
        string[] files = Directory.GetFiles(target_dir);
        string[] dirs = Directory.GetDirectories(target_dir);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (string dir in dirs)
        {
            DeleteDirectory(dir);
        }

        Directory.Delete(target_dir, false);
    }

    /// <summary>
    /// 存在相同存档ID
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private bool ExistSaveIndex(int index)
    {
        for(int i = 0; i < _allSaves.Count; i++)
        {
            if (_allSaves[i].SaveIndex == index)
                return true;
        }
        return false;
    }

    #region Game

    public AchievementSaveData GetAchievementSaveDataByID(int achievementID)
    {
        if (_achievementSaveDatas.ContainsKey(achievementID))
            return _achievementSaveDatas[achievementID];
        return null;
    }

    private void InitAchievementInfo()
    {
        if (globalSaveData == null)
            return;

        var achievementInfos = globalSaveData.AchievementSaveData;
        for (int i = 0; i < achievementInfos.Count; i++) 
        {
            var info = achievementInfos[i];
            if (!_achievementSaveDatas.ContainsKey(info.AchievementID))
            {
                _achievementSaveDatas.Add(info.AchievementID, info);
            }
        }
    }

    #endregion
}
