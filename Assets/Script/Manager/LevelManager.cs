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
    public LevelDataInfo LevelInfo;

    public bool needServicing = false;
    
    
    private AIFactory _lastAIfactory;

    public List<PickableItem> pickupList = new List<PickableItem>();

    public AIRuntimeData airuntimedata = new AIRuntimeData();

    public IBoid targetBoid;


    public NativeArray<float3> steeringBehaviorJob_aiShipPos;
    public NativeArray<float3> steeringBehaviorJob_aiShipVelocity;
    public NativeArray<float> steeringBehaviorJob_aiShipRadius;



    public NativeList<JobHandle> jobhandleList;

    NativeArray<bool>[] arrive_isVelZero;
    public NativeArray<float3>[] arrivebehavior_steeringlinear;
    public NativeArray<float3>[] facebehavior_steeringangle;
    public NativeArray<float3>[] cohesionbehavior_steeringlinear;
    public NativeArray<float3>[] separationbehavior_steeringlinear;
    public NativeArray<float3>[] alignmentbehavior_steeringlinear;



    public int levelWaveIndex = 0;
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
    }

    public virtual void LevelUpdate()
    {

    }

    public virtual void LevelFixedUpdate()
    {
        if (!isLevelUpdate)
        {
            return;
        }
        if (airuntimedata == null || airuntimedata.aiShipList == null)
        {
            return;
        }

        UpdateJobData();



        arrivebehavior_steeringlinear = new NativeArray<float3>[airuntimedata.Count];
        facebehavior_steeringangle = new NativeArray<float3>[airuntimedata.Count];
        cohesionbehavior_steeringlinear = new NativeArray<float3>[airuntimedata.Count];
        separationbehavior_steeringlinear = new NativeArray<float3>[airuntimedata.Count];
        alignmentbehavior_steeringlinear = new NativeArray<float3>[airuntimedata.Count];



        arrive_isVelZero = new NativeArray<bool>[airuntimedata.Count];
        jobhandleList = new NativeList<JobHandle>(Allocator.TempJob);

        for (int i = 0; i < airuntimedata.Count; i++)
        {
            arrive_isVelZero[i] = new NativeArray<bool>(1, Allocator.TempJob);

            arrivebehavior_steeringlinear[i] = new NativeArray<float3>(1, Allocator.TempJob);

            if (airuntimedata.aiSteeringBehaviorControllerList[i].arriveBehavior)
            {
                ArriveBehavior.ArriveBehaviorJob arriveBehaviorJob = new ArriveBehavior.ArriveBehaviorJob
                {
                    job_maxAcceleration = airuntimedata.aiSteeringBehaviorControllerList[i].maxAcceleration,
                    job_selfPos = steeringBehaviorJob_aiShipPos[i],
                    job_selfVel = steeringBehaviorJob_aiShipVelocity[i],
                    job_slowRadius = airuntimedata.arriveBehavior[i].slowRadius,
                    job_targetPos = targetBoid.GetPosition(),
                    job_targetRadius = targetBoid.GetRadius(),
                    //return value
                    JRD_isvelzero = arrive_isVelZero[i],
                    JRD_linear = arrivebehavior_steeringlinear[i],
                };

                JobHandle jobhandle = arriveBehaviorJob.Schedule();
                jobhandleList.Add(jobhandle);
            }

        }

        //Wait to complate Jobs
        JobHandle.CompleteAll(jobhandleList);

        // apply steering data to ai ship
        float3 accelaration;
        float angle;
        Vector3 deltamovement;

        // loop through all airuntimedata 
        for (int i = 0; i < airuntimedata.Count; i++)
        {
            if (arrive_isVelZero[i][0])
            {
                airuntimedata.aiShipBoidList[i].SetVelocity(Vector3.zero);
            }



            accelaration = float3.zero;
            angle = 0;
            deltamovement = Vector3.zero;

            accelaration += arrivebehavior_steeringlinear[i][0] * airuntimedata.arriveBehavior[i].GetWeight();
            //add other behavior steeringdata
            //angle += facebehavior_steeringangle[i][0] * airuntimedata.arriveBehavior[i].GetWeight();

            //make sure the length of accelearation in every frame dosen't great than max acceleration 
            if (math.length(accelaration) > airuntimedata.aiSteeringBehaviorControllerList[i].maxAcceleration)
            {
                accelaration = math.normalize(accelaration);
                accelaration *= airuntimedata.aiSteeringBehaviorControllerList[i].maxAcceleration;
            }

            //Calculate movement delta
            deltamovement = (airuntimedata.aiSteeringBehaviorControllerList[i].velocity + accelaration.ToVector3() * 0.5f * Time.fixedDeltaTime * airuntimedata.aiSteeringBehaviorControllerList[i].drag);
            airuntimedata.aiSteeringBehaviorControllerList[i].rb.MovePosition(airuntimedata.aiShipList[i].transform.position + deltamovement * Time.fixedDeltaTime);
            //rb.AddForce(accelaration);

            //Update Boiddata;
            airuntimedata.aiSteeringBehaviorControllerList[i].UpdateIBoid();

            //Rotate ship if angle != 0;
            if (angle != 0)
            {
                airuntimedata.aiShipList[i].controller.rb.rotation = angle; //Quaternion.Euler(0, rotation, 0);
            }
            //Dispose all job data
            arrive_isVelZero[i].Dispose();
            arrivebehavior_steeringlinear[i].Dispose();
        }



        //Dispose all Boid data at the end of frame
        DisposeJobData();


    }
    
    public virtual void LevelLaterUpdate()
    {
        //steeringBehaviorJob_aiShipPos.Dispose();
    }

    public virtual void UpdateJobData()
    {
        steeringBehaviorJob_aiShipPos = new NativeArray<float3>(airuntimedata.aiShipBoidList.Count, Allocator.TempJob);
        steeringBehaviorJob_aiShipVelocity = new NativeArray<float3>(airuntimedata.aiShipBoidList.Count, Allocator.TempJob);
        steeringBehaviorJob_aiShipRadius = new NativeArray<float>(airuntimedata.aiShipBoidList.Count, Allocator.TempJob);

        for (int i = 0; i < airuntimedata.aiShipBoidList.Count; i++)
        {
            steeringBehaviorJob_aiShipPos[i] = airuntimedata.aiShipBoidList[i].GetPosition();
            steeringBehaviorJob_aiShipVelocity[i] = airuntimedata.aiShipBoidList[i].GetVelocity();
            steeringBehaviorJob_aiShipRadius[i] = airuntimedata.aiShipBoidList[i].GetRadius();
        }
    }

    public virtual void DisposeJobData()
    {
        jobhandleList.Dispose();
        steeringBehaviorJob_aiShipPos.Dispose();
        steeringBehaviorJob_aiShipVelocity.Dispose();
        steeringBehaviorJob_aiShipRadius.Dispose();
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
        if(pickedItem is PickUpGold)
        {
            var gold = pickedItem as PickUpGold;
            RogueManager.Instance.AddCurrency(gold.CurrencyGain);
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

    public void LevelActive()
    {
        targetBoid = RogueManager.Instance.currentShip.GetComponent<IBoid>();
        isLevelUpdate = true;
    }

    public void UnloadCurrentLevel( )
    {
        if(currentLevel == null) { return; }
        isLevelUpdate = false;
        currentLevel.Unload();
        levelWaveIndex = 0;

        if(airuntimedata != null && airuntimedata.aiShipList != null)
        {
            for (int i = 0; i < airuntimedata.aiShipList.Count; i++)
            {
                airuntimedata.aiShipList[i].PoolableDestroy();
            }
            if(airuntimedata.aiShipList.Count >0)
            {
                airuntimedata.ClearAIData();
            }
           
        }


        PoolManager.Instance.Recycle();
        GameObject.Destroy(currentLevel.gameObject);
        currentLevel = null;

    }



    public void GameOver()
    {
        isLevelUpdate = false;

        PoolManager.Instance.Recycle();
        CameraManager.Instance.SetCameraUpdate(false);
    }

    public void LevelReset()
    {

    }

    public void LevelStop()
    {
        for (int i = 0; i < airuntimedata.aiShipList.Count; i++)
        {
            for (int n = 0; n < airuntimedata.aiShipList[i].UnitList.Count; n++)
            {
                airuntimedata.aiShipList[i].UnitList[n].SetUnitProcess(false);
            }
            airuntimedata.aiShipList[i].controller.SetControllerUpdate(false);
        }
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
                    OnEnemyShipDie(evt.Ship);
                }
                break;
        }
    }

    /// <summary>
    /// 敌人死亡
    /// </summary>
    /// <param name="ship"></param>
    private void OnEnemyShipDie(BaseShip ship)
    {
        AchievementManager.Instance.Trigger<BaseShip>(AchievementWatcherType.EnemyKill, ship);
    }
}
