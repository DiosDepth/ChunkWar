using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum SpawnShape
{
    Point,
    Rectangle,
    Circle,
    Line,
    Arch,
}

[Serializable]
public class SpawnShapeSetting
{
    public int totalCount;
    public float spawnIntervalTime;


    public virtual List<Vector2> CalculateShapePosList(Vector2 referencepos, Vector2 dir, Vector2 mapSize) { return null; }
}

[System.Serializable]
public class RectSpawnSetting : SpawnShapeSetting
{
    public int maxRowCount;
    public Vector2 sizeInterval;


    //private Vector2Int _rectanglematirx;
    //private List<Vector2> _formposlist = new List<Vector2>();

    public RectSpawnSetting()
    {
        totalCount = 16;
        maxRowCount = 4;
        sizeInterval = new Vector2(0.5f, 0.5f);
        spawnIntervalTime = 0f;
    }
    public RectSpawnSetting(int m_totalcont, int m_maxrowcount, Vector2 m_sizeinterval, float m_spawnintervaltime = 0)
    {
        totalCount = m_totalcont;
        maxRowCount = m_maxrowcount;
        sizeInterval = m_sizeinterval;
        spawnIntervalTime = m_spawnintervaltime;
    }

    public RectSpawnSetting(int m_totalcont, int m_maxrowcount, float m_spawnintervaltime = 0)
    {
        totalCount = m_totalcont;
        maxRowCount = m_maxrowcount;
        sizeInterval = new Vector2(0.5f, 0.5f);
        spawnIntervalTime = m_spawnintervaltime;
    }

    public Vector2Int GetRectSize()
    {
        Vector2Int tempmatrix = Vector2Int.zero;
        tempmatrix.x = maxRowCount;
        tempmatrix.y = Mathf.CeilToInt((float)totalCount / (float)maxRowCount);
        return tempmatrix;
    }

    /// <summary>
    /// map size 可以取shipconfig 里面的getmapsize
    /// dir 是创建的Rect方向，一般情况下是创建Agent 和目标之间的相对方向
    /// </summary>
    /// <param name="referencepos"></param>
    /// <param name="mapSize"></param>
    /// <returns></returns>
    public override List<Vector2> CalculateShapePosList(Vector2 referencepos, Vector2 dir, Vector2 mapSize )
    {
        Vector2 interval = new Vector2(sizeInterval.x + mapSize.x, sizeInterval.y + mapSize.y);
        Vector2 offset = Vector2.zero;
        Vector2Int rect = GetRectSize();

        if (rect.x.IsEven())
        {
            offset.x = interval.x * 0.5f;
        }
        if (rect.y.IsEven())
        {
            offset.y = interval.y * -0.5f;
        }
        if (rect.x == 0 || rect.y == 0)
        {
            Debug.LogError(" private _rectanglematirx is 0");
            return null;
        }
        Vector2Int posscaler = Vector2Int.zero;

        //按照方向创建矩阵
        Matrix4x4 matrix4X4 = Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.forward, dir));
        Vector3 pos;

        //计算原始坐标
        List<Vector2> shapeposlist = new List<Vector2>(); 
        for (int y = 0; y < rect.y; y++)
        {
            for (int x = 0; x < rect.x; x++)
            {
                posscaler = new Vector2Int(x - Mathf.FloorToInt(rect.x * 0.5f), Mathf.FloorToInt(rect.y * 0.5f) - y);

                pos = new Vector2(posscaler.x * interval.x, posscaler.y * interval.y) + offset;
                //左乘矩阵
                pos = matrix4X4 * pos;
                shapeposlist.Add(pos + referencepos.ToVector3());
                if (shapeposlist.Count == totalCount)
                {
                    break;
                }
            }
        }
        return shapeposlist;
    }


}

[Serializable]
public class ShipSpawnInfo
{
    public int ID;
    public Transform referenceTarget;
    public Vector2 referencePos;
    public BaseShipConfig shipConfig;
    public UnityAction<BaseShip> action;

    public ShipSpawnInfo(int m_id, Vector2 m_referencepos, Transform m_referencetarget, UnityAction<BaseShip> m_action)
    {
        ID = m_id;
        referencePos = m_referencepos;
        referenceTarget = m_referencetarget;
        action = m_action;
    }

}

[Serializable]
public class AIShipSpawnInfo : ShipSpawnInfo
{
    public int hardLevelID;
    //public SpawnShape spawnShape;
    public SpawnShapeSetting shapeSetting;
    //public AIShipConfig AIShipConfig;
    public AIShipSpawnInfo(int m_id, Vector2 m_referencepos, Transform m_referencetarget, int m_hardlevelid, SpawnShapeSetting m_shapesetting, UnityAction<BaseShip> m_action) : base(m_id, m_referencepos, m_referencetarget, m_action)
    {
        hardLevelID = m_hardlevelid;
        shapeSetting = m_shapesetting;
        action = m_action;
        shipConfig = DataManager.Instance.GetAIShipConfig(ID);
    }
}
[Serializable]
public class DroneSpawnInfo : ShipSpawnInfo
{
    public DroneSpawnInfo (int m_id, Vector2 m_referencepos, Transform m_referencetarget, UnityAction<BaseShip> m_action) : base(m_id, m_referencepos, m_referencetarget, m_action)
    {
        action = m_action;
        shipConfig = DataManager.Instance.GetDroneConfig(ID);
    }
}

public class ShipSpawnAgent : MonoBehaviour, IPoolable
{

    protected const string EntitySpawnEffect = "Battle/Enemy_SpawnEffect";
    public ShipSpawnInfo spawnInfo;

    

    protected BaseShipConfig _shipconfig;
    public virtual void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public virtual void PoolableReset()
    {
        
    }

    public virtual void PoolableSetActive(bool isactive = true)
    {
       
    }

    public virtual void Initialization()
    {

    }


    public virtual async void StartSpawn(ShipSpawnInfo m_spawnInfo)
    {
        spawnInfo = m_spawnInfo;
        await Spawn(spawnInfo);
    }

    private async UniTask Spawn(ShipSpawnInfo spawnInfo)
    {
        //实例化Ship
        //await UniTask.Delay((int)aiSpawnSetting.spawnIntervalTime * 1000);
        CreateEntity(spawnInfo);


        await UniTask.Delay(1000);
        //Debug.Log(string.Format("Create Enemy Success! UnitID = {0} , Count = {1}", aiSpawnSetting.spawnUnitID, _shiplist.Count));
        PoolableDestroy();
    }

    public void StopSpawn()
    {
        PoolableDestroy();
    }

    private async void CreateEntity(ShipSpawnInfo spawnInfo)
    {
        ///创建特效
        EffectManager.Instance.CreateEffect(EntitySpawnEffect, spawnInfo.referencePos);
        SoundManager.Instance.PlayBattleSound("Ship/Ship_SpawnEffect", transform);
        await UniTask.Delay(1000);

        if (!RogueManager.Instance.IsLevelSpawnVaild())
            return;

        PoolManager.Instance.GetObjectSync(GameGlobalConfig.AIShipPath + spawnInfo.shipConfig.GetPrefabName(), true, (obj) =>
        {
            obj.transform.position = spawnInfo.referencePos;
            if (spawnInfo.referenceTarget != null)
            {
                obj.transform.rotation = Quaternion.LookRotation(Vector3.forward, obj.transform.position.DirectionToXY(spawnInfo.referenceTarget.position));
            }
            var tempship = obj.GetComponent<BaseShip>();
            tempship.Initialization();
            spawnInfo.action.Invoke(tempship);
        }, LevelManager.AIPool);
    }
}
