using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.IO;
using System;
using System.Reflection;

public class DataManager : Singleton<DataManager>
{
    public long sumDataLength;
    public long totalDataLength;
    public Dictionary<string, TestaDataInfo> TestDataDic = new Dictionary<string, TestaDataInfo>();
    public Dictionary<string, SoundDataInfo> SoundDataDic = new Dictionary<string, SoundDataInfo>();
    public Dictionary<int, BaseUnitConfig> UnitConfigDataDic = new Dictionary<int, BaseUnitConfig>();
    
    public Dictionary<string, LevelData> LevelDataDic = new Dictionary<string, LevelData>();
    public Dictionary<string, PickUpData> PickUpDataDic = new Dictionary<string, PickUpData>();

    private Dictionary<int, ShopGoodsItemConfig> _shopGoodsDic = new Dictionary<int, ShopGoodsItemConfig>();
    private Dictionary<int, WreckageDropItemConfig> _wreckageItemDic = new Dictionary<int, WreckageDropItemConfig>();
    private Dictionary<int, ShipPlugDataItemConfig> _shipPlugDic = new Dictionary<int, ShipPlugDataItemConfig>();
    private Dictionary<int, PlayerShipConfig> _shipConfigDic = new Dictionary<int, PlayerShipConfig>();
    private Dictionary<int, AIShipConfig> _AIShipConfigDic = new Dictionary<int, AIShipConfig>();
    private Dictionary<int, DroneConfig> _DroneConfigDic = new Dictionary<int, DroneConfig>();
    private Dictionary<int, AchievementItemConfig> _achievementDic = new Dictionary<int, AchievementItemConfig>();
    private Dictionary<int, LevelSpawnConfig> _levelPresetDic = new Dictionary<int, LevelSpawnConfig>();
    private Dictionary<int, CampConfig> _campConfigDic = new Dictionary<int, CampConfig>();
    private Dictionary<int, EnemyHardLevelData> _enemyHardLevelDatas = new Dictionary<int, EnemyHardLevelData>();
    private Dictionary<string, BulletConfig> _bulletConfig = new Dictionary<string, BulletConfig>();

    public BattleMainConfig battleCfg;
    public ShopMainConfig shopCfg;
    public GameMiscConfig gameMiscCfg;

    private bool loadFinish = false;

    public DataManager()
    {
        Initialization();
    }
    public override void Initialization()
    {
        base.Initialization();


    }
    public bool CollectCSV(out TextAsset[] datas)
    {
        string path = "DataRes";
        datas = Resources.LoadAll<TextAsset>(path);
        if(datas.Length == 0)
        {
            return false;
        }
        else
        {
            for (int i = 0; i < datas.Length; i++)
            {
                totalDataLength += datas[i].bytes.Length;
            }
            Debug.Log("All csv has been collected");
            return true;
        }

    }

    public void LoadAllData(UnityAction callback = null)
    {
        if (!loadFinish)
        {
            LoadLevelConfig();
            LoadAllBaseUnitConfig();
            LoadMiscData();
            SoundManager.Instance.LoadBanks();

            bool iscompleted = CollectCSV(out TextAsset[] datas);
            if (iscompleted)
            {
                for (int i = 0; i < datas.Length; i++)
                {
                    GetData(datas[i]);
                }
            }
        }
        callback?.Invoke();
        loadFinish = true;
    }

    public float GetLoadingRate()
    {
        return sumDataLength / totalDataLength;
    }

#if UNITY_EDITOR

    public void LoadShipPlugConfig_Editor()
    {
        var res = Resources.LoadAll<ShipPlugDataItemConfig>(DataConfigPath.ShipPlugConfigRoot);
        for (int i = 0; i < res.Length; i++)
        {
            if (!_shipPlugDic.ContainsKey(res[i].ID))
            {
                _shipPlugDic.Add(res[i].ID, res[i]);
            }
        }
    }

    public void LoadAIShipConfig_Editor()
    {
        var enemy = Resources.LoadAll<AIShipConfig>(DataConfigPath.EnemyShipConfigRoot);
        if (enemy != null && enemy.Length > 0)
        {
            for (int i = 0; i < enemy.Length; i++)
            {
                if (!_AIShipConfigDic.ContainsKey(enemy[i].ID))
                {
                    _AIShipConfigDic.Add(enemy[i].ID, enemy[i]);
                }
            }
        }
    }

    public void LoadBulletConfig_Editor()
    {
        var allBullets = Resources.LoadAll<BulletConfig>(DataConfigPath.BulletConfigRoot);
        if(allBullets != null && allBullets.Length > 0)
        {
            for (int i = 0; i < allBullets.Length; i++)
            {
                if (!_bulletConfig.ContainsKey(allBullets[i].BulletName))
                {
                    _bulletConfig.Add(allBullets[i].BulletName, allBullets[i]);
                }
            }
        }
    }

    public List<BulletConfig> GetAllBulletConfig()
    {
        return _bulletConfig.Values.ToList();
    }

#endif

    private void LoadMiscData()
    {
        battleCfg = ResManager.Instance.Load<BattleMainConfig>(DataConfigPath.BattleMainConfigPath);
        shopCfg = ResManager.Instance.Load<ShopMainConfig>(DataConfigPath.ShopMainConfigPath);
        gameMiscCfg = ResManager.Instance.Load<GameMiscConfig>(DataConfigPath.GameMiscConfigPath);

#if UNITY_EDITOR
        if(battleCfg != null)
        {
            battleCfg.DataCheck();
        }

#endif

        if (shopCfg != null)
        {
            var goodLst = shopCfg.Goods;
            for(int i = 0; i < goodLst.Count; i++)
            {
                if (!_shopGoodsDic.ContainsKey(goodLst[i].GoodID))
                {
                    _shopGoodsDic.Add(goodLst[i].GoodID, goodLst[i]);
                }
            }

            var wreckageData = shopCfg.WreckageDrops;
            for(int i = 0; i < wreckageData.Count; i++)
            {
                if (!_wreckageItemDic.ContainsKey(wreckageData[i].UnitID))
                {
                    _wreckageItemDic.Add(wreckageData[i].UnitID, wreckageData[i]);
                }
            }
        }

    }

    public EnemyHardLevelItem GetEnemyHardLevelItem(int groupID, int hardLevelIndex)
    {
        if (_enemyHardLevelDatas.ContainsKey(groupID))
        {
            var item = _enemyHardLevelDatas[groupID];
            return item.GetHardLevelItemByIndex(hardLevelIndex);
        }
        Debug.LogError("HardLevel Group Null! ID = " + groupID);
        return null;
    }

    public EnemyHardLevelData GetEnemyHardLevelData(int groupID)
    {
        if (_enemyHardLevelDatas.ContainsKey(groupID))
        {
            return _enemyHardLevelDatas[groupID];
        }
        return null;
    }

    /// <summary>
    /// 根据稀有度获取掉落物
    /// </summary>
    /// <param name="rarity"></param>
    /// <returns></returns>
    public PickUpData GetWreckagePickUpData(GoodsItemRarity rarity)
    {
        foreach(var item in PickUpDataDic.Values)
        {
            if (item.Type == AvaliablePickUp.Wreckage && item.Rarity == rarity)
                return item;
        }
        return null;
    }

    /// <summary>
    /// 获取所有残骸拾取类型
    /// </summary>
    /// <returns></returns>
    public List<PickUpData> GetAllWastePickUpData()
    {
        var lst= PickUpDataDic.Values.ToList().FindAll(x => x.Type == AvaliablePickUp.WastePickup);
        lst.Sort((x, y) => 
        {
            return x.CountRef - y.CountRef;
        });
        return lst;
    }

    public LevelSpawnConfig GetLevelSpawnConfig(int presetID)
    {
        LevelSpawnConfig result = null;
        _levelPresetDic.TryGetValue(presetID, out result);
        Debug.Assert(result != null, "GetLevelSpawnConfig Null! ID= " + presetID);
        return result;
    }

    public List<PlayerShipConfig> GetAllShipConfigs(bool sort = true)
    {
        var valueLst = _shipConfigDic.Values.ToList();

        if (sort)
        {
            valueLst.Sort();
        }

        return valueLst;
    }

    public PlayerShipConfig GetShipConfig(int shipID)
    {
        PlayerShipConfig result = null;
        _shipConfigDic.TryGetValue(shipID, out result);
        Debug.Assert(result != null, "GetShipConfig Null! ID= " + shipID);
        return result;
    }

    public BaseUnitConfig GetUnitConfig(int unitID)
    {
        BaseUnitConfig result = null;
        UnitConfigDataDic.TryGetValue(unitID, out result);
        Debug.Assert(result != null, "GetUnitConfig Null! ID= " + unitID);
        return result;
    }

    /// <summary>
    /// 敌人舰船配置
    /// </summary>
    /// <param name="enemyID"></param>
    /// <returns></returns>
    public AIShipConfig GetAIShipConfig(int enemyID)
    {
        AIShipConfig result = null;
        _AIShipConfigDic.TryGetValue(enemyID, out result);
        Debug.Assert(result != null, "GetEnemyShipConfig Null! ID= " + enemyID);
        return result;
    }

    public DroneConfig GetDroneConfig(int droneID)
    {
        DroneConfig result = null;
        _DroneConfigDic.TryGetValue(droneID, out result);
        Debug.Assert(result != null, "GetDroneConfig Null! ID= " + droneID);
        return result;
    }

    public List<AIShipConfig> GetAllAIShips(bool order = true)
    {
        var lst = _AIShipConfigDic.Values.ToList();
        if (order)
            return lst.OrderBy(x => x.ID).ToList();

        return lst;
    }

    /// <summary>
    /// 获取所有精英AI船
    /// </summary>
    /// <returns></returns>
    public List<int> GetAllAIEliteIDs()
    {
        return _AIShipConfigDic.Values.Where(x => x.ClassLevel == EnemyClassType.Elite).Select(x => x.ID).ToList();
    }

    public List<int> GetAllBossIDs()
    {
        return _AIShipConfigDic.Values.Where(x => x.ClassLevel == EnemyClassType.Boss).Select(x => x.ID).ToList();
    }

    public ShopGoodsItemConfig GetShopGoodsCfg(int goodsID)
    {
        ShopGoodsItemConfig result = null;
        _shopGoodsDic.TryGetValue(goodsID, out result);
        Debug.Assert(result != null, "GetShopGoodsCfg Null! ID= " + goodsID);
        return result;
    }

    public WreckageDropItemConfig GetWreckageDropItemConfig(int unitID)
    {
        WreckageDropItemConfig result = null;
        _wreckageItemDic.TryGetValue(unitID, out result);
        Debug.Assert(result != null, "GetWreckageDropItemConfig Null! ID= " + unitID);
        return result;
    }

    public WreckageShopBuyItemConfig GetWreckageShopBuyItemConfig(GoodsItemRarity rarity)
    {
        var lst = shopCfg.WreckageShopBuyItemCfg;
        return lst.Find(x => x.Rarity == rarity);
    }

    public ShipPlugDataItemConfig GetShipPlugItemConfig(int id)
    {
        ShipPlugDataItemConfig result = null;
        _shipPlugDic.TryGetValue(id, out result);
        Debug.Assert(result != null, "GetShipPlugItemConfig Null! ID= " + id);
        return result;
    }

    public AchievementItemConfig GetAchievementItemConfig(int id)
    {
        AchievementItemConfig result = null;
        _achievementDic.TryGetValue(id, out result);
        Debug.Assert(result != null, "GetAchievementItemConfig Null! ID= " + id);
        return result;
    }

    public CampConfig GetCampConfigByID(int campID)
    {
        CampConfig result = null;
        _campConfigDic.TryGetValue(campID, out result);
        Debug.Assert(result != null, "GetCampConfigByID Null! ID= " + campID);
        return result;
    }

    public BulletConfig GetBulletConfigByType(string bulletName)
    {
        BulletConfig result = null;
        _bulletConfig.TryGetValue(bulletName, out result);
        Debug.Assert(result != null, "GetBulletConfigByID Null! ID= " + bulletName);
        return result;
    }

    public List<CampConfig> GetAllCampConfigs()
    {
        return _campConfigDic.Values.ToList();
    }

    public List<BaseUnitConfig> GetAllUnitConfigs()
    {
        return UnitConfigDataDic.Values.ToList();
    }

    public List<AchievementItemConfig> GetAllAchievementConfigs()
    {
        return _achievementDic.Values.ToList();
    }

    private void LoadingData<T>(TextAsset file, Dictionary<string, T> dic, UnityAction callback = null) where T : DataInfo, new()
    {
        PersistentData<T>(file, dic);
        sumDataLength += file.bytes.Length;
        callback?.Invoke();
    }

    private void PersistentData<T>(TextAsset ast, Dictionary<string, T> dic) where T : DataInfo, new()
    {
        string[] raw = ast.text.Split(new char[] { '\r' });

        for (int i = 1; i < raw.Length - 1; i++)
        {
            string[] row = raw[i].Split(new char[] { ',' });
            // T temp_data = new T (row);
            T temp_data = new T();
            temp_data.Initialization(row);
            if (dic == null)
            {
                dic = new Dictionary<string, T>();
            }
            dic.Add(temp_data.Name, temp_data);
        }
    }

    private void GetData(TextAsset m_fileinfo)
    {
        switch (m_fileinfo.name)
        {
            case "TestData":
                LoadingData<TestaDataInfo>(m_fileinfo, TestDataDic);
                break;
            case "LevelData":
                LoadingData<LevelData>(m_fileinfo, LevelDataDic);
                break;
            case "SoundData":
                LoadingData<SoundDataInfo>(m_fileinfo, SoundDataDic);
                break;
            case "PickUpData":
                LoadingData<PickUpData>(m_fileinfo, PickUpDataDic);
                break;
            case "EnemyHardLevelItem":
                LoadHardLevelData(m_fileinfo);
                break;
            default:
                break;
        }
    }

    private void LoadLevelConfig()
    {
        var allLevelPresets = Resources.LoadAll<LevelSpawnConfig>(DataConfigPath.LevelSpawnConfigRoot);
        if(allLevelPresets != null && allLevelPresets.Length > 0)
        {
            for(int i = 0; i < allLevelPresets.Length; i++)
            {
                if (_levelPresetDic.ContainsKey(allLevelPresets[i].LevelPresetID))
                {
                    Debug.LogError("Find Same LevelPresetID !" + allLevelPresets[i].LevelPresetID);
                    continue;
                }
                _levelPresetDic.Add(allLevelPresets[i].LevelPresetID, allLevelPresets[i]);
            }
        }
    }

    private void LoadAllBaseUnitConfig()
    {
        var builds = Resources.LoadAll<BuildingConfig>(DataConfigPath.BuildingConfigRoot);
        var ships = Resources.LoadAll<PlayerShipConfig>(DataConfigPath.ShipConfigRoot);
        var weapons = Resources.LoadAll<WeaponConfig>(DataConfigPath.WeaponConfigRoot);
        var enemy = Resources.LoadAll<AIShipConfig>(DataConfigPath.EnemyShipConfigRoot);
        var achievement = Resources.LoadAll<AchievementItemConfig>(DataConfigPath.AchievementConfigRoot);
        var camps = Resources.LoadAll<CampConfig>(DataConfigPath.CampConfigRoot);
        var allBullets = Resources.LoadAll<BulletConfig>(DataConfigPath.BulletConfigRoot);
        var allPlugs = Resources.LoadAll<ShipPlugDataItemConfig>(DataConfigPath.ShipPlugConfigRoot);
        var allDrones = Resources.LoadAll<DroneConfig>(DataConfigPath.DroneConfigRoot);

        if (builds != null && builds.Length > 0)
        {
            for(int i = 0; i < builds.Length; i++)
            {
                AddItemToUnitDic(builds[i].ID, builds[i]);
            }
        }

        if (weapons != null && weapons.Length > 0)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                AddItemToUnitDic(weapons[i].ID, weapons[i]);
            }
        }

        if (ships != null && ships.Length > 0)
        {
            for(int i = 0; i < ships.Length; i++)
            {
                if (_shipConfigDic.ContainsKey(ships[i].ID))
                {
                    Debug.LogError("Find Same ShipID !" + ships[i].ID);
                    continue;
                }
                _shipConfigDic.Add(ships[i].ID, ships[i]);
            }
        }

        if (enemy != null && enemy.Length > 0)
        {
            for (int i = 0; i < enemy.Length; i++)
            {
                if (_AIShipConfigDic.ContainsKey(enemy[i].ID))
                {
                    Debug.LogError("Find Same enemyID !" + enemy[i].ID);
                    continue;
                }
                _AIShipConfigDic.Add(enemy[i].ID, enemy[i]);
            }
        }

        if (allDrones != null && allDrones.Length > 0)
        {
            for (int i = 0; i < allDrones.Length; i++)
            {
                if (_DroneConfigDic.ContainsKey(allDrones[i].ID))
                {
                    Debug.LogError("Find Same DroneID !" + allDrones[i].ID);
                    continue;
                }
                _DroneConfigDic.Add(allDrones[i].ID, allDrones[i]);
            }
        }

        if (achievement != null && achievement.Length > 0)
        {
            for (int i = 0; i < achievement.Length; i++) 
            {
                if (_achievementDic.ContainsKey(achievement[i].AchievementID))
                {
                    Debug.LogError("Find Same achievementID !" + achievement[i].AchievementID);
                    continue;
                }
                _achievementDic.Add(achievement[i].AchievementID, achievement[i]);
            }
        }

        if(camps != null && camps.Length > 0)
        {
            for (int i = 0; i < camps.Length; i++)
            {
                if (_campConfigDic.ContainsKey(camps[i].CampID))
                {
                    Debug.LogError("Find Same camps !" + camps[i].CampID);
                    continue;
                }
                _campConfigDic.Add(camps[i].CampID, camps[i]);
            }
        }

        if(allBullets != null && allBullets.Length > 0)
        {
            for (int i = 0; i < allBullets.Length; i++)
            {
                if (_bulletConfig.ContainsKey(allBullets[i].BulletName))
                {
                    Debug.LogError("Find Same Bullet !" + allBullets[i].ID);
                    continue;
                }
                _bulletConfig.Add(allBullets[i].BulletName, allBullets[i]);
            }
        }

        if (allPlugs != null && allPlugs.Length > 0)
        {
            for (int i = 0; i < allPlugs.Length; i++)
            {
                if (_shipPlugDic.ContainsKey(allPlugs[i].ID))
                {
                    Debug.LogError("Find Same Plug !" + allPlugs[i].ID);
                    continue;
                }
                _shipPlugDic.Add(allPlugs[i].ID, allPlugs[i]);
            }
        }
    }

    private void LoadHardLevelData(TextAsset info)
    {
        string[] raw = info.text.Split(new char[] { '\r' });

        for (int i = 1; i < raw.Length - 1; i++)
        {
            string[] row = raw[i].Split(new char[] { ',' });
            EnemyHardLevelItem temp_data = new EnemyHardLevelItem();
            temp_data.Initialization(row);

            if (_enemyHardLevelDatas.ContainsKey(temp_data.GroupID))
            {
                _enemyHardLevelDatas[temp_data.GroupID].AddHardLevelItems(temp_data.ID, temp_data);
            }
            else
            {
                EnemyHardLevelData newData = new EnemyHardLevelData();
                newData.GroupID = temp_data.GroupID;
                newData.AddHardLevelItems(temp_data.ID, temp_data);
                _enemyHardLevelDatas.Add(newData.GroupID, newData);
            }
        }
    }

    private void AddItemToUnitDic(int uintID, BaseUnitConfig cfg)
    {
        if (!UnitConfigDataDic.ContainsKey(uintID))
        {
            UnitConfigDataDic.Add(uintID, cfg);
        }
    }


    #region CSVExport

    public static void WriteCSV(string fileName, object data)
    {
        string name;
        List<string> props = new List<string>();
        List<List<string>> items = new List<List<string>>();

        object[] objs = data.GetType().GetCustomAttributes(typeof(CSVTagAttribute), true);

        if (objs == null || objs.Length == 0)
            return;

        ///生成表头
        object[] rowObjs = data.GetType().GetCustomAttributes(typeof(CSVCommentAttribute), true);
        if(rowObjs != null || rowObjs.Length > 0)
        {
            name = (objs[0] as CSVTagAttribute).TagName + ",," + (rowObjs[0] as CSVCommentAttribute).Comment;
        }
        else
        {
            name = (objs[0] as CSVTagAttribute).TagName + ",";
        }

        ///Property
        foreach(PropertyInfo property in data.GetType().GetProperties())
        {
            object[] objTag = property.GetCustomAttributes(typeof(CSVTagAttribute), true);
            object[] objComment = property.GetCustomAttributes(typeof(CSVCommentAttribute), true);

            if (objTag == null || objTag.Length == 0)
                continue;

            object value = property.GetValue(data, null);

            if(value is System.Collections.IList)
            {
                List<string> item = new List<string>();
                items.Add(item);
                item.Add((objTag[0] as CSVTagAttribute).TagName);

                ///Title
                object objItem = (value as System.Collections.IList).GetType().GetGenericArguments()[0].GetConstructor(new Type[0]).Invoke(null);
                string itemStr = "";
                foreach (PropertyInfo propInfoItem in objItem.GetType().GetProperties())
                {
                    object[] objItems = propInfoItem.GetCustomAttributes(typeof(CSVTagAttribute), true);
                    if (objItems == null || objItems.Length == 0)
                    {
                        continue;
                    }
                    itemStr += (objItems[0] as CSVTagAttribute).TagName + ",";
                }
                item.Add(itemStr);

                ///Value
                foreach (object valueItem in (value as System.Collections.IList))
                {
                    string valueStr = "";
                    foreach (PropertyInfo propInfoItem in valueItem.GetType().GetProperties())
                    {
                        object[] objItems = propInfoItem.GetCustomAttributes(typeof(CSVTagAttribute), true);
                        if (objItems == null || objItems.Length == 0)
                        {
                            continue;
                        }
                        valueStr += propInfoItem.GetValue(valueItem, null)?.ToString() + ",";
                    }
                    item.Add(valueStr);
                }
            }
            else
            {
                if (objComment == null || objComment.Length == 0)
                {
                    props.Add((objTag[0] as CSVTagAttribute).TagName + "," + (value == null ? "" : value.ToString()) + ",");
                }
                else
                {
                    props.Add((objTag[0] as CSVTagAttribute).TagName + "," + (value == null ? "" : value.ToString()) + "," + (objComment[0] as CSVCommentAttribute).Comment);
                }
            }
        }

        List<string> line = new List<string>();
        line.Add(name);
        line.AddRange(props);
        line.Add("/,");
        items.ForEach(x =>
        {
            line.AddRange(x);
            line.Add("/,");
        });

        var root = DetermineLogSavePath();
        if (!Directory.Exists(root))
        {
            Directory.CreateDirectory(root);
        }
        var fullPath = string.Format("{0}/{1}.csv", root, fileName);

        File.WriteAllLines(fullPath, line, System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// Determines the save path to use when loading and saving a file based on a folder name.
    /// </summary>
    /// <returns>The save path.</returns>
    /// <param name="folderName">Folder name.</param>
    static string DetermineLogSavePath()
    {
        var baseRoot = DataConfigPath.Battle_Log_OutPutRootPath;
        string savePath;
        // depending on the device we're on, we assemble the path
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            savePath = Application.persistentDataPath + baseRoot;
        }
        else
        {
            savePath = Application.persistentDataPath + baseRoot;
        }
#if UNITY_EDITOR
        savePath = Application.dataPath + baseRoot;
#endif
        return savePath;
    }

    #endregion
}

public class CSVTagAttribute : Attribute
{
    public string TagName
    {
        get;
        set;
    }

    public CSVTagAttribute (string tagName)
    {
        TagName = tagName;
    }
}

public class CSVCommentAttribute : Attribute
{
    public string Comment { get; set; }

    public CSVCommentAttribute(string comment)
    {
        Comment = comment;
    }
}