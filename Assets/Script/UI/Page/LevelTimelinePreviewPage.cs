using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if GMDEBUG && UNITY_EDITOR
public class LevelTimelinePreviewPage : GUIBasePanel
{

    public GameObject TimeLineNode_Prefab;
    public GameObject EnemyTitle_Prefab;
    public GameObject EnmeyDeital_Prefab;

    public Transform timeLineRoot;
    public Transform enemyTitleRoot;
    public Transform enemyGroupContent;
    public Transform mainContentRoot;

    private List<LTP_TimeLineNode> TimeLineNodes = new List<LTP_TimeLineNode>();
    private List<LTP_EnemyInfoGroup> EnemyItems = new List<LTP_EnemyInfoGroup>();
    private List<LTP_EnemyTitle> EnemyTitle = new List<LTP_EnemyTitle>();

    private WaveConfig _spawnCfg;
    protected override void Awake()
    {
        base.Awake();

    }

    public override void Initialization()
    {
        base.Initialization();
        DataManager.Instance.LoadAIShipConfig_Editor();
    }

    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
        _spawnCfg = (WaveConfig)param[0];
        SetUp();
    }

    private async void SetUp()
    {
        if (_spawnCfg == null)
            return;

        timeLineRoot.DestroyAllChildren();
        for(int i = EnemyTitle.Count - 1; i >= 0; i--)
        {
            GameObject.Destroy(EnemyTitle[i].gameObject);
        }
        enemyGroupContent.DestroyAllChildren();

        TimeLineNodes.Clear();
        EnemyTitle.Clear();
        EnemyItems.Clear();

        for (int i = 0; i < _spawnCfg.DurationTime; i++) 
        {
            var obj = GameObject.Instantiate(TimeLineNode_Prefab);
            obj.transform.SetParent(timeLineRoot, false);
            var cmpt = obj.transform.SafeGetComponent<LTP_TimeLineNode>();
            cmpt.SetUp(i + 1);
            TimeLineNodes.Add(cmpt);
        }

        ///InitEnemyTitle
        var allEnemyItem = _spawnCfg.SpawnConfig;
        for (int i = 0; i < allEnemyItem.Count; i++) 
        {
            var item = allEnemyItem[i];
            var typeCfg = DataManager.Instance.GetAIShipConfig(item.AITypeID);
            var obj = GameObject.Instantiate(EnemyTitle_Prefab);
            obj.transform.SetParent(enemyTitleRoot, false);
            var cmpt = obj.transform.SafeGetComponent<LTP_EnemyTitle>();
            if(typeCfg != null)
            {
                cmpt.SetUp(LocalizationManager.Instance.GetTextValue(typeCfg.GeneralConfig.Name), item.EditorPreviewColor);
            }
            EnemyTitle.Add(cmpt);


            var groupObj = GameObject.Instantiate(EnmeyDeital_Prefab);
            var groupCmpt = groupObj.transform.SafeGetComponent<LTP_EnemyInfoGroup>();
            groupCmpt.SetUp(_spawnCfg.DurationTime, item);
            groupCmpt.transform.SetParent(enemyGroupContent, false);
            EnemyItems.Add(groupCmpt);
        }
        ///生成累计
        GenerateTotal();

        await UniTask.NextFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(enemyTitleRoot.SafeGetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(mainContentRoot.SafeGetComponent<RectTransform>());
    }

    private void GenerateTotal()
    {
        var obj = GameObject.Instantiate(EnemyTitle_Prefab);
        obj.transform.SetParent(enemyTitleRoot, false);
        var cmpt = obj.transform.SafeGetComponent<LTP_EnemyTitle>();
        cmpt.SetUp("累计", Color.grey);
        EnemyTitle.Add(cmpt);

        var groupObj = GameObject.Instantiate(EnmeyDeital_Prefab);
        var groupCmpt = groupObj.transform.SafeGetComponent<LTP_EnemyInfoGroup>();
        groupCmpt.SetUpTotal(_spawnCfg.DurationTime, _spawnCfg.SpawnConfig);
        groupCmpt.transform.SetParent(enemyGroupContent, false);
        EnemyItems.Add(groupCmpt);
    }
}

#endif