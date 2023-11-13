using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if GMDEBUG
using GM_Observer;
#endif

public enum SectorThreadSortType
{
    /// <summary>
    /// 高威胁度优先
    /// </summary>
    HighSector,
    LowSector,
    /// <summary>
    /// 平衡
    /// </summary>
    Balance
}

/*
 * 敌人生成扇区管理
 */
public class LevelSpawnSector 
{
    /// <summary>
    /// 相邻扇区权重
    /// </summary>
    private float _globalAdjacentSectorWeight = 0;
    /// <summary>
    /// 上一次生成的sectorIndex
    /// </summary>
    private byte _lastSpawnSectorIndex;


    private Dictionary<byte, LevelSectorData> _sectorDic = new Dictionary<byte, LevelSectorData>();

    private float _enemyGenerate_Inner_Range;
    private float _enemyGenerate_Outer_Range;

    public void Init()
    {
        _enemyGenerate_Inner_Range = DataManager.Instance.battleCfg.EntitySpawnConfig.EnemyGenerate_Inner_Range;
        _enemyGenerate_Outer_Range = DataManager.Instance.battleCfg.EntitySpawnConfig.EnemyGenerate_Outer_Range;
        ///Init Origin
        _lastSpawnSectorIndex = (byte)UnityEngine.Random.Range(0, GameGlobalConfig.LevelSector_Count + 1);
        var sectorCount = GameGlobalConfig.LevelSector_Count;
        float anglePer = 360 / sectorCount;

        for (byte i = 0; i < sectorCount; i++) 
        {
            LevelSectorData data = new LevelSectorData
            {
                SectorIndex = i,
                SectorAngleStart = anglePer * i,
                SectorAngleEnd = anglePer * (i + 1)
            };

            _sectorDic.Add(i, data);
        }
    }

    public void SetGlobalAdjacentSectorWeight(float value)
    {
        _globalAdjacentSectorWeight = value;
    }

    /// <summary>
    /// 刷新
    /// </summary>
    public void RefreshAndClear()
    {
        _lastSpawnSectorIndex = (byte)UnityEngine.Random.Range(0, GameGlobalConfig.LevelSector_Count + 1);
        foreach(var sector in _sectorDic.Values)
        {
            sector.Clear();
        }
    }

    /// <summary>
    /// 获取创建点
    /// </summary>
    /// <returns></returns>
    public Vector2 GetAIShipSpawnPosition(float Rangemax = -1)
    {
        var currentShip = RogueManager.Instance.currentShip;
        if (currentShip == null)
            return Vector2.positiveInfinity;

        float targetRangeDistance = 0;
        if (Rangemax != -1)
        {
            targetRangeDistance = UnityEngine.Random.Range(_enemyGenerate_Inner_Range, Rangemax);
        }
        else
        {
            targetRangeDistance = UnityEngine.Random.Range(_enemyGenerate_Inner_Range, _enemyGenerate_Outer_Range);
        }

        bool inAdjacentSector = Utility.CalculateRate100(_globalAdjacentSectorWeight);
        var vaildSector = GetVaildSectorList(inAdjacentSector);
        if (vaildSector.Count <= 0)
            return Vector2.positiveInfinity;

        var sectorSortType = RogueManager.Instance.GetCurrentWaveSectorSortType();
        byte targetSectorIndex = 0;
        vaildSector.OrderBy(x => GetSectorDataByIndex(x).ThreadCacheValue);
        switch (sectorSortType)
        {
            case SectorThreadSortType.Balance:
                ///去除最高和最低
                vaildSector.RemoveAt(vaildSector.Count - 1);
                vaildSector.RemoveAt(0);
                targetSectorIndex = vaildSector[UnityEngine.Random.Range(0, vaildSector.Count)];
                break;

            case SectorThreadSortType.HighSector:
                targetSectorIndex = vaildSector[vaildSector.Count - 1];
                break;

            case SectorThreadSortType.LowSector:
                targetSectorIndex = vaildSector[0];
                break;
        }

        var sector = GetSectorDataByIndex(targetSectorIndex);
        if(sector == null)
        {
            Debug.LogError("Sector Spawn Null! ID = " + targetSectorIndex);
            return Vector2.zero;
        }
        _lastSpawnSectorIndex = targetSectorIndex;
        ///RandomAngle
        float randomAngle = sector.GetRandomAngle();
        float x = currentShip.transform.position.x + targetRangeDistance * Mathf.Cos(randomAngle * Mathf.PI / 180f);
        float y = currentShip.transform.position.y + targetRangeDistance * Mathf.Sin(randomAngle * Mathf.PI / 180f);
        return new Vector2(x, y);
    }

    public void RefreshSectorThreadCache()
    {
        var currentShip = RogueManager.Instance.currentShip;
        if (currentShip == null)
            return;

        foreach (var sector in _sectorDic.Values)
        {
            sector.Clear();
        }

        Vector2 pos = currentShip.transform.position;
        var allAIShip = ECSManager.Instance.activeAIAgentData.shipList;
        for (int i = 0; i < allAIShip.Count; i++) 
        {
            var targetShip = allAIShip[i];
            if (targetShip == null)
                continue;

            float angle = Vector2.Angle(targetShip.transform.position, pos);
            var sector = GetSectorDataByAngle(angle);
            if (sector == null)
                continue;

            sector.AddThread((targetShip as AIShip).AIShipCfg.SectorThreadValue);
        }

#if GMDEBUG
        List<SectorObserverData> ob_Data = new List<SectorObserverData>();
        foreach(var sector in _sectorDic.Values)
        {
            SectorObserverData data = new SectorObserverData
            {
                Index = sector.SectorIndex,
                Thread = sector.ThreadCacheValue
            };
            ob_Data.Add(data);
        }

        BattleObserver.Instance.SectorData = ob_Data;
#endif
    }

    private List<byte> GetVaildSectorList(bool inAdjacentSector)
    {
        List<byte> result = new List<byte>();
        if (inAdjacentSector)
        {
            var count = GameGlobalConfig.LevelSector_AdjacentCount;
            for (int i = -count; i <= count; i++) 
            {
                if (i == 0)
                    continue;

                var newIndex = i + _lastSpawnSectorIndex;
                if (newIndex < 0) 
                {
                    newIndex += GameGlobalConfig.LevelSector_Count - 1;
                }
                else if(newIndex > GameGlobalConfig.LevelSector_Count - 1)
                {
                    newIndex -= GameGlobalConfig.LevelSector_Count;
                }

                result.Add((byte)newIndex);
            }
        }
        else
        {
            result = _sectorDic.Keys.ToList();
            result.Remove(_lastSpawnSectorIndex);
        }

        return result;
    }

    private LevelSectorData GetSectorDataByIndex(byte index)
    {
        if (_sectorDic.ContainsKey(index))
            return _sectorDic[index];

        return null;
    }

    private LevelSectorData GetSectorDataByAngle(float angle)
    {
        float anglePer = 360 / GameGlobalConfig.LevelSector_Count;
        var index = Mathf.RoundToInt(angle / anglePer);
        return GetSectorDataByIndex((byte)index);
    }
}

public class LevelSectorData
{
    public int SectorIndex;
    public float SectorAngleStart;
    public float SectorAngleEnd;

    /// <summary>
    /// 扇区强度缓存
    /// </summary>
    public float ThreadCacheValue
    {
        get;
        private set;
    }

    public void AddThread(float value)
    {
        ThreadCacheValue += value;
    }

    public float GetRandomAngle()
    {
        return UnityEngine.Random.Range(SectorAngleStart, SectorAngleEnd);
    }

    public void Clear()
    {
        ThreadCacheValue = 0;
    }
}
