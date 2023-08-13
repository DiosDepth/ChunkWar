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
[System.Serializable]
public class RectAISpawnSetting
{
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
    private Coroutine _spawncorotine;

    private Vector2 _spawnreferencedir;
    private AIShipConfig _shipconfig;

    



    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual void Initialization()
    {

    }

    public void StartSpawn(Vector2 referencepos, RectAISpawnSetting spawnsettings, UnityAction<List<AIShip>> callback = null)
    {
        aiSpawnSetting = spawnsettings;
        target = RogueManager.Instance.currentShip.transform;
        _spawncorotine = StartCoroutine(Spawn(referencepos, callback));
    }

    public IEnumerator Spawn(Vector2 referencepos, UnityAction<List<AIShip>> callback)
    {
        AIShip tempship;

        _shipconfig = DataManager.Instance.GetAIShipConfig(aiSpawnSetting.spawnUnitID);
        _rectanglematirx = GetRectangleMatrix();
        _spawnreferencedir = GetSpawnReferenceDir();

        CalculateFormPos(referencepos);

        for (int i = 0; i < _formposlist.Count; i++)
        {
            //实例化所有的配置敌人ＡＩ
            yield return new WaitForSeconds (aiSpawnSetting.spawnIntervalTime);
            PoolManager.Instance.GetObjectSync(GameGlobalConfig.AIShipPath + _shipconfig.Prefab.name, true, (obj) => 
            {
                obj.transform.position = _formposlist[i];
                obj.transform.rotation = Quaternion.LookRotation(Vector3.forward, this.transform.position.DirectionToXY(target.position));
                tempship = obj.GetComponent<AIShip>();
                tempship.Initialization();
                _shiplist.Add(tempship);

            }, (LevelManager.Instance.currentLevel as BattleLevel).AIPool.transform);
        }
        if(callback != null)
        {
            callback.Invoke(_shiplist);
        }
        PoolableDestroy();
    }

    public void StopSpawn(UnityAction<List<AIShip>> callback)
    {
        if(callback != null)
        {
            callback.Invoke(_shiplist);
        }
        StopCoroutine(_spawncorotine);
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
        tempmatrix.y = Mathf.CeilToInt( (float)aiSpawnSetting.totalCount / (float)aiSpawnSetting.maxRowCount);
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
                    if(_formposlist.Count == aiSpawnSetting.totalCount)
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
        StopCoroutine(_spawncorotine);
       // throw new System.NotImplementedException();
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
}
