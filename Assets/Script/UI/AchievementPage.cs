using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPage : GUIBasePanel, EventListener<AchievementEvent>
{
    private const string AchievementGroup_Item_PrefabPath = "Prefab/GUIPrefab/CmptItems/AchievementTabItem";

    private List<AchievementTabItemCmpt> tabCmpt = new List<AchievementTabItemCmpt>(); 

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<AchievementEvent>();
        GetGUIComponent<Button>("BackBtn").onClick.AddListener(OnBackBtnClick);
        InitAchievementGroup();
    }

    public override void Hidden()
    {
        base.Hidden();
        this.EventStopListening<AchievementEvent>();
    }

    protected void OnBackBtnClick()
    {
        UIManager.Instance.HiddenUI("AchievementPage");
    }

    /// <summary>
    /// ≥ı ºªØ“≥«©
    /// </summary>
    private void InitAchievementGroup()
    {
        var root = transform.Find("Content/Tab");
        foreach(AchievementGroupType type in System.Enum.GetValues(typeof(AchievementGroupType)))
        {
            PoolManager.Instance.GetObjectSync(AchievementGroup_Item_PrefabPath, true, (obj) =>
            {
                var cmpt = obj.transform.SafeGetComponent<AchievementTabItemCmpt>();
                cmpt.SetUp(type);
                tabCmpt.Add(cmpt);
            }, root);
        }
    }

    public void OnEvent(AchievementEvent evt)
    {

    }
}
