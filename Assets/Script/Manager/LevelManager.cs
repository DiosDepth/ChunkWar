using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public enum LevelEventType
{
    LevelLoading,
    LevelLoaded,
    LevelActive,

}

public struct LevelEvent
{
    public LevelEventType evtType;
    public int levelIndex;
    public AsyncOperation asy;

    public LevelEvent(LevelEventType m_type, int m_index, AsyncOperation m_asy )
    {
        evtType = m_type;
        levelIndex = m_index;
        asy = m_asy;
    }
    private static LevelEvent e;

    public static void Trigger(LevelEventType m_type, int m_index, AsyncOperation m_asy)
    {
        e.evtType = m_type;
        e.levelIndex = m_index;
        e.asy = m_asy;
        EventCenter.Instance.TriggerEvent<LevelEvent>(e);
    }
}


public enum AISpawnEventType
{
    SpawnRequest,
    StopRequest,
}

public struct AISpawnEvent
{
    public AISpawnEventType evtType;
    public int waveCfgID;

    public AISpawnEvent(AISpawnEventType m_type, int m_waveCfgID)
    {
        evtType = m_type;
        waveCfgID = m_waveCfgID;

    }
    private static AISpawnEvent e;

    public static void Trigger(AISpawnEventType m_type, int m_waveCfgID)
    {
        e.evtType = m_type;
        e.waveCfgID = m_waveCfgID;
        EventCenter.Instance.TriggerEvent<AISpawnEvent>(e);
    }
}

public enum AvaliableLevel
{
    Harbor,
    BattleLevel_001,
    ShipSelectionLevel,
}

public class LevelManager : Singleton<LevelManager>,EventListener<LevelEvent>, EventListener<PickableItemEvent>, EventListener<AISpawnEvent>
{

    private AsyncOperation asy;
 



 

    public LevelEntity currentLevel;
    public LevelDataInfo LevelInfo;

    public bool needServicing = false;
    
    
    private AIFactory _lastAIfactory;
    public List<AIShip> aiShipList = new List<AIShip>();

    public int levelWaveIndex = 0;
    public LevelManager()
    {
        Initialization();
        this.EventStartListening<LevelEvent>();
        this.EventStartListening<PickableItemEvent>();
        this.EventStartListening<AISpawnEvent>();
    }

    public override void Initialization()
    {
        base.Initialization();
    }
    public void OnEvent(LevelEvent evt)
    {
        switch (evt.evtType)
        {
            case LevelEventType.LevelLoading:
                break;
            case LevelEventType.LevelLoaded:
                //run in the LoadingScreen.cs

                break;
            case LevelEventType.LevelActive:
                break;
        }
    }

    /// <summary>
    /// 拾取物Event
    /// </summary>
    /// <param name="evt"></param>
    public void OnEvent(PickableItemEvent evt)
    {
        var pickedItem = evt.PickedItem;
        if(pickedItem is PickUpGold)
        {
            var gold = pickedItem as PickUpGold;
            RogueManager.Instance.AddCurrency(gold.CurrencyGain);
            RogueManager.Instance.AddEXP(gold.EXPGain);
        }
    }





    public void StopAISpawn()
    {

    }


 

    public PlayerShip SpawnShipAtPos(GameObject ship, Vector3 pos, Quaternion rot, bool isactive)
    {

        return SpawnActorAtPos(ship, pos, rot, isactive).GetComponentInChildren<PlayerShip>();
    }
    public GameObject SpawnActorAtPos(string prefabpath,Vector3 pos, Quaternion rot, bool isactive = true)
    {
        var obj = ResManager.Instance.Load<GameObject>(prefabpath);
        obj.SetActive(isactive);
        obj.transform.position = pos;
        obj.transform.rotation = rot;

        return obj;
    }

    public GameObject SpawnActorAtPos(GameObject prefab, Vector3 pos, Quaternion rot, bool isactive = true)
    {
        var obj = new GameObject();
        obj.transform.position = pos;
        obj.name = "ShipContainer";

        var ship = GameObject.Instantiate(prefab);
        ship.GetComponent<PlayerShip>().container = obj;
        ship.SetActive(isactive);
        ship.transform.position = pos;
        ship.transform.rotation = rot;
        ship.transform.parent = obj.transform;

        return ship;
    }

    public IEnumerator LoadScene(int levelindex,UnityAction<AsyncOperation> callback)
    {
        asy = SceneManager.LoadSceneAsync(levelindex);
       // asy.allowSceneActivation = false;

        while (asy.progress < 0.9f)
        {
            yield return null;
        }
        //asy.allowSceneActivation = true;
        
        while(!asy.isDone)
        {
            yield return null;
        }
        callback?.Invoke(asy);

        yield return null;
    }

    public T GetCurrentLevelEntity<T>() where T : LevelEntity
    {
        if (currentLevel == null)
            return default(T);

        return currentLevel as T;
    }

    public IEnumerator LoadLevel(string levelname,UnityAction<LevelEntity> callback = null)
    {

        LevelData data = null;
        //加载关卡
        if (currentLevel == null)
        {
            DataManager.Instance.LevelDataDic.TryGetValue(levelname, out data);
            if( data != null)
            {
                LevelInfo = LevelDataInfo.CreateInfo(data);
                GameObject obj = ResManager.Instance.Load<GameObject>(data.LevelPrefabPath);
                currentLevel = obj.GetComponent<LevelEntity>();
                currentLevel.Initialization();
                callback?.Invoke(currentLevel);
            }
        }
        else
        {
            currentLevel.Initialization();
            callback?.Invoke(currentLevel);
        }

        yield return null;
    }


    public void UnloadCurrentLevel( )
    {
        if(currentLevel == null) { return; }
        currentLevel.Unload();
        levelWaveIndex = 0;

        if(aiShipList != null)
        {
            for (int i = 0; i < aiShipList.Count; i++)
            {
                aiShipList[i].PoolableDestroy();
            }
            aiShipList.Clear();
        }


        var types = UnityEngine.MonoBehaviour.FindObjectsOfType<MonoBehaviour>().OfType<IPoolable>();
        foreach (IPoolable t in types)
        {
            t.PoolableDestroy();
        }
       

        GameObject.Destroy(currentLevel.gameObject);
        currentLevel = null;

    }

    public void GameOver()
    {
        StopAISpawn();
        CameraManager.Instance.SetCameraUpdate(false);
    }

    public void LevelReset()
    {

    }

    public void OnEvent(AISpawnEvent evt)
    {
        switch (evt.evtType)
        {
            case AISpawnEventType.SpawnRequest:
                break;
            case AISpawnEventType.StopRequest:
                break;
        }
    }
}
