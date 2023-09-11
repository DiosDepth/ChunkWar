using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CampDetailPage : GUIBasePanel, EventListener<RogueEvent>
{
    private CampData _campData;

    private EnhancedScroller _buffScroller;
    private EnhancedScroller _levelScroller;

    private GeneralScrollerItemController _buffController;
    private GeneralScrollerItemController _levelController;

    private TextMeshProUGUI _currentScoreText;

    private static string BuffItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/CampBuffItem";
    private static string LevelItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/CampLevelItem";

    private RectTransform _scoreRect;

    protected override void Awake()
    {
        base.Awake();
        GetGUIComponent<Button>("GeneralBackBtn").onClick.AddListener(OnBackBtnClick);
        _buffScroller = transform.Find("Content/MidPanel/BuffContent/Content/Scroll View").SafeGetComponent<EnhancedScroller>();
        _levelScroller = transform.Find("Content/DownPanel/Content/Scroll View").SafeGetComponent<EnhancedScroller>();
        _currentScoreText = transform.Find("Content/TopPanel/ScoreContent/ScoreValue").SafeGetComponent<TextMeshProUGUI>();
        _scoreRect = transform.Find("Content/TopPanel/ScoreContent").SafeGetComponent<RectTransform>();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<RogueEvent>();
    }

    public override void Hidden()
    {
        base.Hidden();
        this.EventStopListening<RogueEvent>();
        _buffController.Clear();
        _levelController.Clear();
    }

    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
        int campID = (int)param[0];
        _campData = GameManager.Instance.GetCampDataByID(campID);
        Init();
        InitController();
        RefreshCampContent();
    }

    private void Init()
    {
        if (_campData == null)
            return;

        var campContent = transform.Find("Content/MidPanel/CampDetail/Content");
        campContent.Find("Icon").SafeGetComponent<Image>().sprite = _campData.Config.CampIcon;
        campContent.Find("CampName").SafeGetComponent<TextMeshProUGUI>().text = _campData.CampName;
        campContent.Find("CampDesc").SafeGetComponent<TextMeshProUGUI>().text = _campData.CampDesc;
        campContent.Find("ScoreContent/Score").SafeGetComponent<TextMeshProUGUI>().text = _campData.TotalScore.ToString();
        _currentScoreText.text = _campData.CurrentRemainScore.ToString();
        transform.Find("Content/TopPanel/ScoreContent/LevelText").SafeGetComponent<TextMeshProUGUI>().text = string.Format("LV.{0}", _campData.GetCampLevel);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_scoreRect);
    }

    public void OnEvent(RogueEvent evt)
    {
        switch (evt.type)
        {
            case RogueEventType.CampBuffUpgrade:
                OnCampBuffUpgrade();
                break;
        }
    }

    private void InitController()
    {
        _buffController = new GeneralScrollerItemController();
        _buffController.InitPrefab(BuffItem_PrefabPath, false);
        _buffController.OnItemSelected = OnBuffItemSelect;
        _buffScroller.Delegate = _buffController;

        _levelController = new GeneralScrollerItemController();
        _levelController.InitPrefab(LevelItem_PrefabPath, false);
        _levelController.OnItemSelected = OnLevelItemSelect;
        _levelScroller.Delegate = _levelController;
    }

    private void RefreshCampContent()
    {
        var allBuffItems = _campData.GenerateBuffItemUIDs();
        _buffController.RefreshData(allBuffItems);
        _buffScroller.ReloadData();

        var allLevelItems = _campData.GenerateLevelItemUIDs();
        _levelController.RefreshData(allLevelItems);
        _levelScroller.ReloadData();
    }

    private void OnBackBtnClick()
    {
        UIManager.Instance.HiddenUI("CampDetailPage");
    }

    private void OnBuffItemSelect(uint uid)
    {

    }

    private void OnLevelItemSelect(uint uid)
    {

    }

    /// <summary>
    /// BuffÉý¼¶Ë¢ÐÂ
    /// </summary>
    private void OnCampBuffUpgrade()
    {
        _currentScoreText.text = _campData.CurrentRemainScore.ToString();
        _buffScroller.ReloadData();
    }
}
