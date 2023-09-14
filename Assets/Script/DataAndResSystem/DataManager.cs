using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Linq;

public class DataManager : Singleton<DataManager>
{
    public FileInfo[] fileinfo;
    public long sumDataLength;
    public long totalDataLength;
    public Dictionary<string, TestaDataInfo> TestDataDic = new Dictionary<string, TestaDataInfo>();
    public Dictionary<string, SoundDataInfo> SoundDataDic = new Dictionary<string, SoundDataInfo>();
    public Dictionary<int, BaseUnitConfig> UnitConfigDataDic = new Dictionary<int, BaseUnitConfig>();
    
    public Dictionary<string, LevelData> LevelDataDic = new Dictionary<string, LevelData>();
    public Dictionary<string, BulletData> BulletDataDic = new Dictionary<string, BulletData>();
    public Dictionary<string, PickUpData> PickUpDataDic = new Dictionary<string, PickUpData>();

    private Dictionary<int, ShopGoodsItemConfig> _shopGoodsDic = new Dictionary<int, ShopGoodsItemConfig>();
    private Dictionary<int, WreckageDropItemConfig> _wreckageItemDic = new Dictionary<int, WreckageDropItemConfig>();
    private Dictionary<int, ShipPlugItemConfig> _shipPlugDic = new Dictionary<int, ShipPlugItemConfig>();
    private Dictionary<int, PlayerShipConfig> _shipConfigDic = new Dictionary<int, PlayerShipConfig>();
    private Dictionary<int, AIShipConfig> _AIShipConfigDic = new Dictionary<int, AIShipConfig>();
    private Dictionary<int, AchievementItemConfig> _achievementDic = new Dictionary<int, AchievementItemConfig>();
    private Dictionary<int, LevelSpawnConfig> _levelPresetDic = new Dictionary<int, LevelSpawnConfig>();
    private Dictionary<int, CampConfig> _campConfigDic = new Dictionary<int, CampConfig>();

    public BattleMainConfig battleCfg;
    public ShopMainConfig shopCfg;
    public ShipPlugConfig shipPlugCfg;
    public GameMiscConfig gameMiscCfg;

    public DataManager()
    {
        Initialization();
    }
    public override void Initialization()
    {
        base.Initialization();


    }
    public bool CollectCSV()
    {
        string path = Directory.GetCurrentDirectory() + "\\Assets\\Resources\\DataRes";
        //Debug.Log(path);
        DirectoryInfo dirinfo = new DirectoryInfo(path);
        fileinfo = dirinfo.GetFiles("*.csv");
        if(fileinfo.Length == 0)
        {
            return false;
        }
        else
        {
            for (int i = 0; i < fileinfo.Length; i++)
            {
                totalDataLength += fileinfo[i].Length;
            }
            Debug.Log("All csv has been collected");
            return true;
        }

    }

    public IEnumerator LoadAllData(UnityAction callback = null)
    {
        LoadLevelConfig();
        LoadAllBaseUnitConfig();
        LoadMiscData();
        bool iscompleted = CollectCSV();
        if (iscompleted)
        {
            for (int i = 0; i < fileinfo.Length; i++)
            {
                GetData(fileinfo[i]);
            }
            //模拟Loading，至少3秒钟
            yield return new WaitForSeconds(2);

            while (GetLoadingRate() < 1)
            {
                Debug.Log("DataLoading...");
                yield return null;
            }
            //yield return new WaitForSeconds(5);
            Debug.Log("All csv has been loaded");
            if (callback != null)
            {
                callback();
            }
            yield return new WaitForEndOfFrame();
        }
        else
        {
            yield return new WaitForSeconds(2);
            if (callback != null)
            {
                callback();
            }
            yield return null;
        }
    }

    public float GetLoadingRate()
    {
        return sumDataLength / totalDataLength;
    }

    private void LoadMiscData()
    {
        battleCfg = ResManager.Instance.Load<BattleMainConfig>(DataConfigPath.BattleMainConfigPath);
        shopCfg = ResManager.Instance.Load<ShopMainConfig>(DataConfigPath.ShopMainConfigPath);
        shipPlugCfg = ResManager.Instance.Load<ShipPlugConfig>(DataConfigPath.ShipPlugMainConfigPath);
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

        if (shipPlugCfg != null)
        {
            var plug = shipPlugCfg.PlugConfigs;
            for (int i = 0; i < plug.Count; i++)
            {
                if (!_shipPlugDic.ContainsKey(plug[i].ID))
                {
                    _shipPlugDic.Add(plug[i].ID, plug[i]);
                }
            }
        }
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

    public List<PlayerShipConfig> GetAllShipConfigs()
    {
        return _shipConfigDic.Values.ToList();
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

    public ShipPlugItemConfig GetShipPlugItemConfig(int id)
    {
        ShipPlugItemConfig result = null;
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

    public List<CampConfig> GetAllCampConfigs()
    {
        return _campConfigDic.Values.ToList();
    }

    public List<AchievementItemConfig> GetAllAchievementConfigs()
    {
        return _achievementDic.Values.ToList();
    }

    private IEnumerator LoadingData<T>(FileInfo file, Dictionary<string, T> dic, UnityAction callback) where T : DataInfo, new()
    {

        ResManager.Instance.LoadAsync<TextAsset>(GetDataInfoPath(file), (textasset) =>
        {
            PersistentData<T>(textasset, dic);
            sumDataLength += file.Length;
            if (callback != null)
            {
                callback();
            }
        });
        yield return null;
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

    private void GetData(FileInfo m_fileinfo)
    {
        switch (m_fileinfo.Name)
        {
            case "TestData.csv":
                MonoManager.Instance.StartCoroutine(LoadingData<TestaDataInfo>(m_fileinfo, TestDataDic, () =>
                {
                    Debug.Log("TestData has been loaded!");

                }));
                break;
            case "LevelData.csv":
                MonoManager.Instance.StartCoroutine(LoadingData<LevelData>(m_fileinfo, LevelDataDic, () =>
                {
                    Debug.Log("LevelData has been loaded!");

                }));
                break;
            case "SoundData.csv":
                MonoManager.Instance.StartCoroutine(LoadingData<SoundDataInfo>(m_fileinfo, SoundDataDic, () =>
                {
                    Debug.Log("SoundData has been loaded!");

                }));
                break;
            case "BulletData.csv":
                MonoManager.Instance.StartCoroutine(LoadingData<BulletData>(m_fileinfo, BulletDataDic, () =>
                {
                    Debug.Log("BulletData has been loaded!");

                }));
                break;
            case "PickUpData.csv":
                MonoManager.Instance.StartCoroutine(LoadingData<PickUpData>(m_fileinfo, PickUpDataDic, () =>
                {
                    Debug.Log("PickUpData has been loaded!");

                }));
                break;
        }
    }

    private string GetDataInfoPath(FileInfo m_fileinfo)
    {
        string path = m_fileinfo.Name;
        path = path.Substring(0, path.Length - 4);
        path = "DataRes/" + path;
        return path;
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

        if(achievement != null && achievement.Length > 0)
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
    }

    private void AddItemToUnitDic(int uintID, BaseUnitConfig cfg)
    {
        if (!UnitConfigDataDic.ContainsKey(uintID))
        {
            UnitConfigDataDic.Add(uintID, cfg);
        }
    }

}
