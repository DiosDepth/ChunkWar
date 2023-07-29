using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;

public class DataManager : Singleton<DataManager>
{
    public FileInfo[] fileinfo;
    public long sumDataLength;
    public long totalDataLength;
    public Dictionary<string, TestaDataInfo> TestDataDic = new Dictionary<string, TestaDataInfo>();
    public Dictionary<string, SoundDataInfo> SoundDataDic = new Dictionary<string, SoundDataInfo>();
    public Dictionary<string, BaseUnitConfig> UnitConfigDataDic = new Dictionary<string, BaseUnitConfig>();
    public Dictionary<string, ShipConfig> ShipConfigDic = new Dictionary<string, ShipConfig>();
    public Dictionary<string, LevelData> LevelDataDic = new Dictionary<string, LevelData>();
    public Dictionary<string, BulletData> BulletDataDic = new Dictionary<string, BulletData>();

    private Dictionary<int, ShopGoodsItemConfig> _shopGoodsDic = new Dictionary<int, ShopGoodsItemConfig>();
    public BattleMainConfig battleCfg;
    public ShopMainConfig shopCfg;

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

        if(shopCfg != null)
        {
            var goodLst = shopCfg.Goods;
            for(int i = 0; i < goodLst.Count; i++)
            {
                if (!_shopGoodsDic.ContainsKey(goodLst[i].GoodID))
                {
                    _shopGoodsDic.Add(goodLst[i].GoodID, goodLst[i]);
                }
            }
        }
    }

    public ShopGoodsItemConfig GetShopGoodsCfg(int goodsID)
    {
        ShopGoodsItemConfig result = null;
        _shopGoodsDic.TryGetValue(goodsID, out result);
        Debug.Assert(result != null, "GetShopGoodsCfg Null! ID= " + goodsID);
        return result;
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
            case "UnitConfigData.csv":
                 Dictionary<string, UnitConfigData> tempDic = new Dictionary<string, UnitConfigData>();
                MonoManager.Instance.StartCoroutine(LoadingData<UnitConfigData>(m_fileinfo, tempDic, () =>
                {
                    BaseUnitConfig unitconfig;
                    foreach (KeyValuePair<string,UnitConfigData> kv in tempDic)
                    {
                        unitconfig = ResManager.Instance.Load<BaseUnitConfig>(kv.Value.ConfigPath);
                        UnitConfigDataDic.Add(unitconfig.UnitName, unitconfig);
                    }
                    Debug.Log("UnitConfigData has been loaded!");

                }));
                break;
            case "ShipConfigData.csv":
                Dictionary<string, ShipConfigData> shipDic = new Dictionary<string, ShipConfigData>();
                MonoManager.Instance.StartCoroutine(LoadingData<ShipConfigData>(m_fileinfo, shipDic, () =>
                {
                    ShipConfig shipconfig;
                    foreach (KeyValuePair<string, ShipConfigData> kv in shipDic)
                    {
                        shipconfig = ResManager.Instance.Load<ShipConfig>(kv.Value.ConfigPath);
                        ShipConfigDic.Add(shipconfig.UnitName, shipconfig);
                    }
                    Debug.Log("ShipConfigData has been loaded!");

                }));
                break;
            case "BulletData.csv":
                MonoManager.Instance.StartCoroutine(LoadingData<BulletData>(m_fileinfo, BulletDataDic, () =>
                {
                    Debug.Log("BulletData has been loaded!");

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



}
