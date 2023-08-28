using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPage : GUIBasePanel, EventListener<AchievementEvent>
{

    private List<AchievementTabItemCmpt> tabCmpt = new List<AchievementTabItemCmpt>();
    private EnhancedScroller _scroller;
    private GeneralScrollerItemController _controller;

    private AchievementGroupType _currentSelectedGroupType = AchievementGroupType.NONE;
    private const string AchievementGroup_Item_PrefabPath = "Prefab/GUIPrefab/CmptItems/AchievementTabItem";
    private const string AchievementItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/AchievementItem";

    protected override void Awake()
    {
        base.Awake();
        _scroller = transform.Find("Content/Scroll View").SafeGetComponent<EnhancedScroller>();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<AchievementEvent>();
        GetGUIComponent<Button>("BackBtn").onClick.AddListener(OnBackBtnClick);
        InitAchievementController();
        InitAchievementGroup();
        SelectDefaultTab();
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
            if (type == AchievementGroupType.NONE)
                continue;

            PoolManager.Instance.GetObjectSync(AchievementGroup_Item_PrefabPath, true, (obj) =>
            {
                var cmpt = obj.transform.SafeGetComponent<AchievementTabItemCmpt>();
                cmpt.SetUp(type);
                tabCmpt.Add(cmpt);
            }, root);
        }
    }

    private void InitAchievementController()
    {
        _controller = new GeneralScrollerItemController();
        _controller.InitPrefab(AchievementItem_PrefabPath, true);
        _scroller.Delegate = _controller;
    }

    private void SelectDefaultTab()
    {
        OnGroupItemSelected(AchievementGroupType.Normal);
    }

    private void RefreshGroupSelection()
    {
        var currentAchievements = GameHelper.GetAchievementsByGroupType(_currentSelectedGroupType);
        _controller.RefreshData(currentAchievements);
        _scroller.ReloadData();
    }

    private AchievementTabItemCmpt GetTabCmptByType(AchievementGroupType type)
    {
        return tabCmpt.Find(x => x.GroupType == type);
    }

    public void OnEvent(AchievementEvent evt)
    {
        switch (evt.evtType)
        {
            case AchievementEventType.UI_GroupSelected:
                OnGroupItemSelected((AchievementGroupType)evt.param[0]);
                break;
        }
    }

    private void OnGroupItemSelected(AchievementGroupType type)
    {
        if (_currentSelectedGroupType == type)
            return;

        var oldTab = GetTabCmptByType(_currentSelectedGroupType);
        if(oldTab != null)
        {
            oldTab.SetSelected(false);
        }

        _currentSelectedGroupType = type;
        var newtab = GetTabCmptByType(_currentSelectedGroupType);
        if(newtab != null)
        {
            newtab.SetSelected(true);
            RefreshGroupSelection();
        }
    }
}
