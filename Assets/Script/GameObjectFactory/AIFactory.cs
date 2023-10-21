using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum Shape
{
    Rectangle,
    Circle,
    Line,
    Arch,
}

public class ExtraSpawnInfo
{
    public int ID;
    public WaveEnemySpawnConfig Cfg;
    public AISkillShip ownerShip;
    public AttachPointConfig PointCfg;
}

[System.Serializable]
public class RectAISpawnSetting
{
    public Shape spawnShape = Shape.Rectangle;
    public int spawnUnitID;
    public int totalCount;
    public int maxRowCount;
    public Vector2 sizeInterval;
    public float spawnIntervalTime;

    public RectAISpawnSetting()
    {
        spawnUnitID = 1;
        totalCount = 16;
        maxRowCount = 4;
        sizeInterval = new Vector2(0.5f, 0.5f);
        spawnIntervalTime = 0f;
    }


    public RectAISpawnSetting(int m_spawnunitid, int m_totalcont, int m_maxrowcount, Vector2 m_sizeinterval, float m_spawnintervaltime = 0)
    {
        spawnUnitID = m_spawnunitid;
        totalCount = m_totalcont;
        maxRowCount = m_maxrowcount;
        sizeInterval = m_sizeinterval;
        spawnIntervalTime = m_spawnintervaltime;
    }

    public RectAISpawnSetting(int m_spawnunitid, int m_totalcont, int m_maxrowcount , float m_spawnintervaltime = 0)
    {
        spawnUnitID = m_spawnunitid;
        totalCount = m_totalcont;
        maxRowCount = m_maxrowcount;
        sizeInterval = new Vector2(0.5f, 0.5f);
        spawnIntervalTime = m_spawnintervaltime;
    }
}

public class AIFactory : MonoBehaviour,IPoolable
{
    public Shape spawnShape = Shape.Rectangle;

    //Rectangle settings
    public RectAISpawnSetting aiSpawnSetting = new RectAISpawnSetting();
    public Transform target;

    private Vector2Int _rectanglematirx;
    private List<Vector2> _formposlist = new List<Vector2>();
    private List<AIShip> _shiplist = new List<AIShip>();

    private Vector2 _spawnreferencedir;
    private AIShipConfig _shipconfig;

    private const string EntitySpawnEffect = "Battle/Enemy_SpawnEffect";

    /// <summary>
    /// 创建数量
    /// </summary>
    private int spawnCount;
    private void CalculateSpawnCount()
    {
        if (_shipconfig.EnemyCountModify)
        {
            var enemyCountModify = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.EnemyCount);
            var tempCount = aiSpawnSetting.totalCount * (1 + enemyCountModify / 100f);
            int countRow = Mathf.CeilToInt(tempCount);

            if(tempCount - countRow > 0)
            {
                bool addOne = Utility.CalculateRate100((tempCount - countRow) * 100);
                if (addOne)
                    countRow += 1;
            }

            spawnCount = Mathf.Min(countRow, GameGlobalConfig.AIShipFactory_SpawnMaxCount);
        }
        else
        {
            spawnCount = aiSpawnSetting.totalCount;
        }
    }

    public virtual void Initialization()
    {

    }

    public async void StartSpawn(Vector2 referencepos, RectAISpawnSetting spawnsettings, int overrideHardlevelID = -1, UnityAction<AIShip> callback = null)
    {
        spawnShape = spawnsettings.spawnShape;
        aiSpawnSetting = spawnsettings;
        target = RogueManager.Instance.currentShip.transform;
        _shipconfig = DataManager.Instance.GetAIShipConfig(aiSpawnSetting.spawnUnitID);
        CalculateSpawnCount();
        await Spawn(referencepos, _shipconfig.Prefab.name, callback, overrideHardlevelID);
    }

    private async UniTask Spawn(Vector2 referencepos, string name, UnityAction<AIShip> callback, int overrideHardlevelID = -1)
    {
        _rectanglematirx = GetRectangleMatrix();
        _spawnreferencedir = GetSpawnReferenceDir();

        CalculateFormPos(referencepos);

        for (int i = 0; i < _formposlist.Count; i++)
        {
            //实例化所有的配置敌人ＡＩ
            await UniTask.Delay((int)aiSpawnSetting.spawnIntervalTime * 1000);
            CreateEntity(_formposlist[i], name, callback, overrideHardlevelID);
        }

        await UniTask.Delay(1200);
        Debug.Log(string.Format("Create Enemy Success! UnitID = {0} , Count = {1}", aiSpawnSetting.spawnUnitID, _shiplist.Count));
        PoolableDestroy();
    }

    public void StopSpawn(UnityAction<List<AIShip>> callback)
    {
        callback?.Invoke(_shiplist);
        PoolableDestroy();
    }

    public Vector2 GetSpawnReferenceDir()
    {
        _spawnreferencedir = this.transform.position.DirectionToXY(RogueManager.Instance.currentShip.transform.position).ToVector2();
        return _spawnreferencedir;
    }

    public virtual Vector2Int GetRectangleMatrix()
    {
        Vector2Int tempmatrix = Vector2Int.zero;
        tempmatrix.x = aiSpawnSetting.maxRowCount;
        tempmatrix.y = Mathf.CeilToInt( spawnCount / (float)aiSpawnSetting.maxRowCount);
        return tempmatrix;
    }

    public List<Vector2> CalculateFormPos(Vector2 referencepos)
    {
        Vector2 interval = new Vector2(aiSpawnSetting.sizeInterval.x + _shipconfig.GetMapSize().x, aiSpawnSetting.sizeInterval.y + _shipconfig.GetMapSize().y);
        Vector2 offset = Vector2.zero;
        if (_rectanglematirx.x.IsEven())
        {
            offset.x = interval.x * 0.5f;
        }
        if(_rectanglematirx.y.IsEven())
        {
            offset.y = interval.y * -0.5f;
        }

        Vector2Int posscaler = Vector2Int.zero;
 
        //按照方向创建矩阵
        Vector3 dir = this.transform.position.DirectionToXY(target.position);
        Matrix4x4 matrix4X4 = Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.forward, dir));
        Vector3 pos;

        if (spawnShape == Shape.Rectangle)
        {
            //计算原始坐标
            if (_rectanglematirx.x==0 || _rectanglematirx.y ==0)
            {
                Debug.LogError(this.gameObject.name + " private _rectanglematirx is 0");
            }
            for (int y = 0;  y <_rectanglematirx.y; y++)
            {
                for (int x = 0; x < _rectanglematirx.x; x++)
                {
                    posscaler = new Vector2Int(x - Mathf.FloorToInt(_rectanglematirx.x * 0.5f), Mathf.FloorToInt(_rectanglematirx.y * 0.5f) - y);

                    pos = new Vector2(posscaler.x * interval.x, posscaler.y * interval.y) + offset;
                    //左乘矩阵
                    pos = matrix4X4 * pos;
                    _formposlist.Add(pos + referencepos.ToVector3());
                    if(_formposlist.Count == spawnCount)
                    {
                        break;
                    }
                }
            }
            return _formposlist;
        }

        return null;
    }

    public void PoolableReset()
    {
        aiSpawnSetting = null;
        _shipconfig = null;
        _formposlist.Clear();
        _shiplist.Clear();
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }

    private async void CreateEntity(Vector2 position, string name, UnityAction<AIShip> callback, int overrideHardlevelID = -1)
    {
        ///创建特效
        EffectManager.Instance.CreateEffect(EntitySpawnEffect, position);
        await UniTask.Delay(1000);

        if (!RogueManager.Instance.IsLevelSpawnVaild())
            return;

        PoolManager.Instance.GetObjectSync(GameGlobalConfig.AIShipPath + name, true, (obj) =>
        {
            obj.transform.position = position;
            if(target != null)
            {
                obj.transform.rotation = Quaternion.LookRotation(Vector3.forward, obj.transform.position.DirectionToXY(target.position));
            }
            var tempship = obj.GetComponent<AIShip>();
            tempship.OverrideHardLevelID = overrideHardlevelID;
            tempship.Initialization();
            _shiplist.Add(tempship);
            callback?.Invoke(tempship);
        }, LevelManager.AIPool);
    }
}
