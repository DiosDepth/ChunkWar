using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if GMDEBUG && UNITY_EDITOR
public class LTP_EnemyInfoGroup : MonoBehaviour
{

    private List<LTP_EnemyItem> EnemyItems = new List<LTP_EnemyItem>();

    public GameObject EnmeyItem_Prefab;

    private WaveEnemySpawnConfig cfg;
    private AIShipConfig _shipCfg;
    private int totalTime;

    public void SetUp(int totalTime, WaveEnemySpawnConfig spawnCfg)
    {
        this.cfg = spawnCfg;
        this.totalTime = totalTime;
        _shipCfg = DataManager.Instance.GetAIShipConfig(cfg.AITypeID);

        transform.DestroyAllChildren();
        EnemyItems.Clear();

        int totalLoop = cfg.LoopCount;
        for (int i = 0; i < totalTime; i++) 
        {
            var obj = GameObject.Instantiate(EnmeyItem_Prefab);
            obj.transform.SetParent(transform, false);
            var cmpt = obj.transform.SafeGetComponent<LTP_EnemyItem>();
            EnemyItems.Add(cmpt);

            if (cfg.StartTime > i)
            {
                cmpt.SetEmpty();
                continue;
            }
            else if (cfg.LoopCount == -1 && (i - cfg.StartTime) % cfg.DurationDelta == 0)
            {
                cmpt.SetUp(cfg.TotalCount, cfg.TotalCount * _shipCfg.SectorThreadValue,  cfg.EditorPreviewColor);
            }
            else if (cfg.LoopCount > 0 && (i - cfg.StartTime) % cfg.DurationDelta == 0 && totalLoop > 0)  
            {
                totalLoop--;
                cmpt.SetUp(cfg.TotalCount, cfg.TotalCount * _shipCfg.SectorThreadValue, cfg.EditorPreviewColor);
            }
            else
            {
                cmpt.SetEmpty();
            }
        }
    }

    /// <summary>
    /// 生成总计统计
    /// </summary>
    /// <param name="totalTime"></param>
    /// <param name="spawnCfgs"></param>
    public void SetUpTotal(int totalTime, List<WaveEnemySpawnConfig> spawnCfgs)
    {
        for (int i = 0; i < totalTime; i++)
        {
            var obj = GameObject.Instantiate(EnmeyItem_Prefab);
            obj.transform.SetParent(transform, false);
            var cmpt = obj.transform.SafeGetComponent<LTP_EnemyItem>();
            EnemyItems.Add(cmpt);

            int totalCount = 0;
            float totalThread = 0;
            for (int j = 0; j < spawnCfgs.Count; j++) 
            {
                var cfg = spawnCfgs[j];
                int totalLoop = cfg.LoopCount;
                if (cfg.StartTime > i)
                {
                    continue;
                }
                else if (cfg.LoopCount == -1 && (i - cfg.StartTime) % cfg.DurationDelta == 0)
                {
                    _shipCfg = DataManager.Instance.GetAIShipConfig(cfg.AITypeID);
                    totalCount += cfg.TotalCount;
                    totalThread += cfg.TotalCount * _shipCfg.SectorThreadValue;
                }
                else if (cfg.LoopCount > 0 && (i - cfg.StartTime) % cfg.DurationDelta == 0 && totalLoop > 0)
                {
                    _shipCfg = DataManager.Instance.GetAIShipConfig(cfg.AITypeID);
                    totalLoop--;
                    totalCount += cfg.TotalCount;
                    totalThread += cfg.TotalCount * _shipCfg.SectorThreadValue;
                }
            }

            if(totalCount == 0 && totalThread == 0)
            {
                cmpt.SetEmpty();
            }
            else
            {
                cmpt.SetUp(totalCount, totalThread, Color.grey);
            }
        }
    }
}
#endif