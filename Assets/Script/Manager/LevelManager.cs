﻿using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
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

public class LevelManager : Singleton<LevelManager>,EventListener<LevelEvent>, EventListener<PickableItemEvent>, EventListener<AISpawnEvent>,EventListener<ShipStateEvent>
{
    public bool isLevelUpdate;
    private AsyncOperation asy;
 
    public LevelEntity currentLevel;

    public bool needServicing = false;
    
    
    private AIFactory _lastAIfactory;

    public List<PickableItem> pickupList = new List<PickableItem>();

    #region Actions

    /* 飞船死亡 */
    public UnityAction<BaseShip, UnitDeathInfo> Action_OnShipDie;
    /* 玩家飞船运动 */
    public UnityAction<bool> OnPlayerShipMove;
    /* 玩家血量变化 */
    public UnityAction<float> OnCoreHPPercentChange;
    /* 护盾回复开始 */
    public UnityAction<uint> OnShieldRecoverStart;
    /* 护盾回复结束 */
    public UnityAction<uint> OnShieldRecoverEnd;
    /* 护盾损坏 */
    public UnityAction<uint> OnShieldBroken;
    /* Unit受击 */
    public UnityAction<HitInfo, DamageResultInfo> OnUnitHit;
    /* 敌人数量变化 */
    public UnityAction<int> OnEnemyCountChange;
    /* 武器reload */
    public UnityAction<uint, bool> OnPlayerWeaponReload;
    /* 武器发射 */
    public UnityAction<uint, int> OnPlayerWeaponFire;
    #endregion

    private BattleMiscRefreshConfig _refreshMiscConfig;

    public LevelManager()
    {
        Initialization();
        this.EventStartListening<LevelEvent>();
        this.EventStartListening<PickableItemEvent>();
        this.EventStartListening<AISpawnEvent>();
        this.EventStartListening<ShipStateEvent>();
        MonoManager.Instance.AddUpdateListener(LevelUpdate);
        MonoManager.Instance.AddFixedUpdateListener(LevelFixedUpdate);
        MonoManager.Instance.AddLaterUpdateListener(LevelLaterUpdate);
    }

    ~LevelManager()
    {
        this.EventStopListening<LevelEvent>();
        this.EventStopListening<PickableItemEvent>();
        this.EventStopListening<AISpawnEvent>();
        this.EventStopListening<ShipStateEvent>();
        MonoManager.Instance.RemoveUpdateListener(LevelUpdate);
        MonoManager.Instance.RemoveFixedUpdateListener(LevelFixedUpdate);
        MonoManager.Instance.RemoveUpdateListener(LevelLaterUpdate);
    }

    public override void Initialization()
    {
        base.Initialization();
        _refreshMiscConfig = DataManager.Instance.gameMiscCfg.RefreshConfig;
    }

    public void Clear()
    {
        pickupList.Clear();
        needServicing = false;
        isLevelUpdate = false;
    }

    public virtual void LevelUpdate()
    {
        if (!isLevelUpdate)
            return;

        if (IsBattleLevel())
        {
            RogueManager.Instance.OnUpdateBattle();
        }
    }

    public virtual void LevelFixedUpdate()
    {
        if (!isLevelUpdate)
        {
            return;
        }

    }
    
    public virtual void LevelLaterUpdate()
    {
        //steeringBehaviorJob_aiShipPos.Dispose();
    }

    public void AddAIData()
    {

    }

    public void RemoveAIData()
    {

    }

    public void ClearAIData()
    {

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
        if(pickedItem is PickUpWaste)
        {
            var gold = pickedItem as PickUpWaste;
            RogueManager.Instance.AddDropWasteCount(gold.WasteGain);
            RogueManager.Instance.AddEXP(gold.EXPGain);
        }
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

    public async void LoadLevel(string levelname, UnityAction<LevelEntity> callback = null)
    {
        LevelData data = null;
        //加载关卡
        if (currentLevel == null)
        {
            DataManager.Instance.LevelDataDic.TryGetValue(levelname, out data);
            if (data != null)
            {
                await ResManager.Instance.LoadAsync<GameObject>(data.LevelPrefabPath, (obj) =>
                {
                    currentLevel = obj.GetComponent<LevelEntity>();
                    currentLevel.Initialization();
                    callback?.Invoke(currentLevel);
                });
            }
        }
        else
        {
            currentLevel.Initialization();
            callback?.Invoke(currentLevel);
        }
    }

    public void LevelActive()
    {

        isLevelUpdate = true;
    }

    public void UnloadCurrentLevel( )
    {
        if(currentLevel == null) { return; }
        isLevelUpdate = false;
        currentLevel.Unload();
        GameObject.Destroy(currentLevel.gameObject);
        currentLevel = null;
    }



    public void GameOver()
    {
        isLevelUpdate = false;
        CameraManager.Instance.SetCameraUpdate(false);
    }

    public void LevelReset()
    {

    }

    public void LevelStop()
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

    public void OnEvent(ShipStateEvent evt)
    {
        switch (evt.conditionState)
        {
            case ShipConditionState.Normal:
                break;
            case ShipConditionState.Freeze:
                break;
            case ShipConditionState.Immovable:
                break;
            case ShipConditionState.Death:
                if (evt.IsPlayer)
                {
                    LevelStop();
                }
                else
                {
                    OnEnemyShipDie(evt.Ship, evt.KillInfo);
                }
                break;
        }

        if(evt.Ship is PlayerShip)
        {
            OnPlayerShipStateChange(evt);
        }
    }

    /// <summary>
    /// 敌人死亡
    /// </summary>
    /// <param name="ship"></param>
    private void OnEnemyShipDie(BaseShip ship, UnitDeathInfo info)
    {
        Action_OnShipDie?.Invoke(ship, info);
        AchievementManager.Instance.Trigger<BaseShip>(AchievementWatcherType.EnemyKill, ship);
    }

    private void OnPlayerShipStateChange(ShipStateEvent state)
    {
        if (state.MovementChange)
        {
            OnPlayerShipMove?.Invoke(state.movementState == ShipMovementState.Move);
        }
    }

    public void OnPlayerShipCoreHPPercentChange(float percent)
    {
        OnCoreHPPercentChange?.Invoke(percent);
    }

    #region Battle Misc

    private static string Harbor_Teleport_Path = "Prefab/PickUps/HarborTeleportPickup";
    private static string Shop_Teleport_Path = "Prefab/PickUps/ShopTeleportPickup";

    /// <summary>
    /// 是否战斗场景
    /// </summary>
    /// <returns></returns>
    public bool IsBattleLevel()
    {
        return currentLevel is BattleLevel;
    }

    /// <summary>
    /// 拾取所有可吸取物件
    /// </summary>
    public void CollectAllPickUps()
    {
        var picker = RogueManager.Instance.currentShip.gameObject;
        for (int i = 0; i < pickupList.Count; i++) 
        {
            var pick = pickupList[i];
            if (pick.CanAutoPickUp)
            {
                pick.PickUp(picker);
            }
        }
    }

    /// <summary>
    /// 创建太空站
    /// </summary>
    public void CreateHarborPickUp()
    {
        var playerShipPosition = GameHelper.GetPlayerShipPosition();
        var targetPos = MathExtensionTools.GetRadomPosFromOutRange(_refreshMiscConfig.Harbor_Teleport_RandomRangeMin, _refreshMiscConfig.Harbor_Teleport_RandomRangeMax, playerShipPosition);

        PoolManager.Instance.GetObjectSync(Harbor_Teleport_Path, true, (obj)=> 
        {
            obj.transform.position = targetPos;
        });
    }

    public void CreateShopPickUp()
    {
        var playerShipPosition = GameHelper.GetPlayerShipPosition();
        var targetPos = MathExtensionTools.GetRadomPosFromOutRange(_refreshMiscConfig.Shop_Teleport_RandomRangeMin, _refreshMiscConfig.Shop_Teleport_RandomRangeMax, playerShipPosition);
        Debug.Log("Create Shop Teleport");
        PoolManager.Instance.GetObjectSync(Shop_Teleport_Path, true, (obj) =>
        {
            obj.transform.position = targetPos;
            obj.transform.SafeGetComponent<ShopTeleport>().Initialization();
        });
        RogueEvent.Trigger(RogueEventType.ShopTeleportSpawn);
    }

    public void ShieldRecoverStart(uint targetUnitID)
    {
        OnShieldRecoverStart?.Invoke(targetUnitID);
    }

    public void ShieldRecoverEnd(uint targetUnitID)
    {
        OnShieldRecoverEnd?.Invoke(targetUnitID);
    }

    public void ShieldBroken(uint targetUnitID)
    {
        OnShieldBroken?.Invoke(targetUnitID);
    }

    public void UnitHit(HitInfo info, DamageResultInfo damageInfo)
    {
        OnUnitHit?.Invoke(info, damageInfo);
    }

    /// <summary>
    /// 玩家武器CD
    /// </summary>
    /// <param name="weaponUID"></param>
    /// <param name="isReloadStart"></param>
    public void PlayerWeaponReload(uint weaponUID, bool isReloadStart)
    {
        OnPlayerWeaponReload?.Invoke(weaponUID, isReloadStart);
    }

    public void PlayerWeaponFire(uint weaponUID, int count)
    {
        OnPlayerWeaponFire?.Invoke(weaponUID, count);
    }

    /// <summary>
    /// 敌人数量变更
    /// </summary>
    /// <param name="count"></param>
    public void EnemyShipCountChange(int count)
    {
        OnEnemyCountChange?.Invoke(count);
    }

    #endregion
}
