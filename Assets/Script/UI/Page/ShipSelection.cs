using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipSelection : GUIBasePanel, EventListener<GeneralUIEvent>
{
    private Image _shipIcon;
    private Text _nameText;
    private Image _classIcon;
    private TextMeshProUGUI _className;
    private TextMeshProUGUI _descText;
    private Image _campIcon;
    private TextMeshProUGUI _propertyDescText;

    private EnhancedScroller _selectionScroller;
    private GeneralScrollerItemController _selectionController;

    private EnhancedScroller _hardLevelScroller;
    private GeneralScrollerItemController _hardLevelController;

    private CanvasGroup _mainGroup;
    public  CanvasGroup _hardLevelGroup;
    private RectTransform _infoGroupRect;
    private RectTransform _propertyRect;
    private List<CampSelectionTabCmpt> _campTabCmpts;
    private List<ShipSelectionItemPropertyCmpt> propertyCmpts = new List<ShipSelectionItemPropertyCmpt>();

    private const string ShipSelectionGridItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShipSelectionItem";
    private const string HardLevelItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/HardLevelModeItem";
    private const string CampSelectionTabItem = "Prefab/GUIPrefab/CmptItems/CampSelectionTab";
    private const string ShipProperty_ItemPrefabPath = "Prefab/GUIPrefab/CmptItems/ShipSelectionItemProperty";

    private int currentSelectIndex = 0;
    private uint currentSelectShipID = 0;
    private int currentSelectCampID = 0;

    protected override void Awake()
    {
        base.Awake();
        _campTabCmpts = new List<CampSelectionTabCmpt>();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<GeneralUIEvent>();
        _mainGroup = transform.Find("uiGroup").SafeGetComponent<CanvasGroup>();
        _hardLevelGroup = transform.Find("HardLevelGroup").SafeGetComponent<CanvasGroup>();

        var shipInfoTrans = transform.Find("uiGroup/Content/Top/ShipInfo");
        _infoGroupRect = shipInfoTrans.Find("Group").SafeGetComponent<RectTransform>();
        _propertyRect = transform.Find("uiGroup/Content/Top/PropertyInfo/PropertyContent/Viewport/Content").SafeGetComponent<RectTransform>();
        _nameText = shipInfoTrans.Find("Group/ShipClassInfo/Name").SafeGetComponent<Text>();
        _classIcon = shipInfoTrans.Find("Group/ShipClassInfo/ClassInfo/ClassIcon").SafeGetComponent<Image>();
        _className = shipInfoTrans.Find("Group/ShipClassInfo/ClassInfo/ClassName").SafeGetComponent<TextMeshProUGUI>();
        _descText = shipInfoTrans.Find("Group/Desc").SafeGetComponent<TextMeshProUGUI>();
        _campIcon = transform.Find("uiGroup/Content/Top/ShipInfo/CampBG").SafeGetComponent<Image>();
        _propertyDescText = _propertyRect.Find("DescContent").SafeGetComponent<TextMeshProUGUI>();

        _selectionScroller = transform.Find("uiGroup/Content/Selection/Scroll View").SafeGetComponent<EnhancedScroller>();
        _hardLevelScroller = transform.Find("HardLevelGroup/Content/Scroll View").SafeGetComponent<EnhancedScroller>();
        _shipIcon = transform.Find("uiGroup/Content/Top/ShipIconPanel/Icon").SafeGetComponent<Image>();
        GetGUIComponent<Button>("GeneralBackBtn").onClick.AddListener(BackBtnPressed);

        InitSelectionController();
        InitHardLevelController();
        InitCampTab();
        SwitchToMainGroup();
    }

    public void BackBtnPressed()
    {
        GameStateTransitionEvent.Trigger(EGameState.EGameState_MainMenu);
    }

    public override void Hidden( )
    {
        base.Hidden();
        this.EventStopListening<GeneralUIEvent>();
        _selectionController.Clear();
        _hardLevelController.Clear();
    }

    private void InitCampTab()
    {
        var allCamp = DataManager.Instance.GetAllCampConfigs();
        var root = transform.Find("uiGroup/Content/CampTab");
        for(int i = 0; i < allCamp.Count; i++)
        {
            PoolManager.Instance.GetObjectSync(CampSelectionTabItem, true, (obj) =>
            {
                var cmpt = obj.transform.SafeGetComponent<CampSelectionTabCmpt>();
                cmpt.SetUp(allCamp[i].CampID);
                _campTabCmpts.Add(cmpt);
            }, root);
        }

        ///Try Select First CampTab
        for (int i = 0; i < allCamp.Count; i++) 
        {
            var campID = allCamp[i].CampID;
            var allShips = GameHelper.GetAllShipsByCampID(campID);
            if (allShips.Count > 0) 
            {
                currentSelectCampID = campID;
                var cmpt = GetCampSelectionCmpt(campID);
                if(cmpt != null)
                {
                    cmpt.OnSelected(true);
                }
                RefreshShipSelectionContent(allShips);
                SetFirstChooseShip();
                break;
            }
        }
    }

    private void InitSelectionController()
    {
        _selectionController = new GeneralScrollerItemController();
        _selectionController.InitPrefab(ShipSelectionGridItem_PrefabPath, true);
        _selectionController.OnItemSelected = OnShipSelect;
        _selectionScroller.Delegate = _selectionController;
    }

    private void InitHardLevelController()
    {
        _hardLevelController = new GeneralScrollerItemController();
        _hardLevelController.InitPrefab(HardLevelItem_PrefabPath, false);
        _hardLevelController.OnItemSelected = OnHardLevelSelect;
        _hardLevelScroller.Delegate = _hardLevelController;
    }

    /// <summary>
    /// Ë¢ÐÂ½¢´¬À¸
    /// </summary>
    private void RefreshShipSelectionContent(List<uint> shipIDs)
    {
        _selectionController.RefreshData(shipIDs);
        _selectionScroller.ReloadData();
    }

    /// <summary>
    /// Ë¢ÐÂhardLevelÑ¡Ôñ
    /// </summary>
    private void RefreshHardLevelSelection()
    {
        var currentSelectShip = RogueManager.Instance.currentShipSelection;
        if (currentSelectShip == null)
            return;

        GameManager.Instance.RefreshHardLevelByShip(currentSelectShip.itemconfig.ID);

        var hardLevels = GameHelper.GetAllHardLevels();
        _hardLevelController.RefreshData(hardLevels);
        _hardLevelScroller.ReloadData();
    }

   
    private void OnHardLevelSelect(uint uid)
    {

    }


    public void OnEvent(GeneralUIEvent evt)
    {
        switch (evt.type)
        {
            case UIEventType.ShipSelectionChange:
                uint uid = (uint)evt.param[0] ;
                OnShipSelect(uid);
                break;

            case UIEventType.ShipSelectionConfirm:
                SwitchToHardLevelGroup();
                break;

            case UIEventType.ShipSelection_CampSelect:
                int campID = (int)evt.param[0];
                SelectCamp(campID);
                break;
        }
    }

    private void SwitchToMainGroup()
    {
        currentSelectIndex = 0;
        _mainGroup.ActiveCanvasGroup(true);
        _hardLevelGroup.ActiveCanvasGroup(false);
    }

    private void SwitchToHardLevelGroup()
    {
        currentSelectIndex = 0;
        RefreshHardLevelSelection();
        _mainGroup.ActiveCanvasGroup(false);
        _hardLevelGroup.ActiveCanvasGroup(true);
    }

    #region Ship Selection

    /// <summary>
    /// Ë¢ÐÂ½¢´¬ÏÔÊ¾
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="dataIndex"></param>
    private void OnShipSelect(uint uid)
    {
        ///RefreshInfo
        if (currentSelectShipID == uid)
            return;

        currentSelectShipID = uid;
        var shipCfg = DataManager.Instance.GetShipConfig((int)uid);
        if (shipCfg == null)
            return;

        _shipIcon.sprite = shipCfg.GeneralConfig.IconSprite;
        _nameText.text = LocalizationManager.Instance.GetTextValue(shipCfg.GeneralConfig.Name);
        _descText.text = LocalizationManager.Instance.GetTextValue(shipCfg.GeneralConfig.Desc);
        var classCfg = DataManager.Instance.gameMiscCfg.GetShipClassConfig(shipCfg.ShipClass);
        if (classCfg != null)
        {
            _classIcon.sprite = classCfg.ClassIcon;
            _className.text = LocalizationManager.Instance.GetTextValue(classCfg.ClassName);
        }

        SetUpShipPropertyInfo(shipCfg);
        var campInfo = GameManager.Instance.GetCampDataByID(shipCfg.PlayerShipCampID);
        if(campInfo != null)
        {
            _campIcon.sprite = campInfo.Config.CampIcon;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_infoGroupRect);
        
    }

    /// <summary>
    /// Ñ¡Ôñ½¢´¬
    /// </summary>
    /// <param name="dataIndex"></param>
    private void SetFirstChooseShip()
    {
        var item = _selectionScroller.GetCellViewAtDataIndex(0);
        if (item != null && item is ShipSelectionItemCmpt)
        {
            var group = item as ShipSelectionItemCmpt;
            group.OnHoverEnter();
        }
    }

    private void SelectCamp(int campID)
    {
        if (currentSelectCampID == campID)
            return;

        var oldCmpt = GetCampSelectionCmpt(currentSelectCampID);
        if(oldCmpt != null)
        {
            oldCmpt.OnSelected(false);
        }

        currentSelectCampID = campID;
        var allShipIDs = GameHelper.GetAllShipsByCampID(campID);
        RefreshShipSelectionContent(allShipIDs);
    }

    private CampSelectionTabCmpt GetCampSelectionCmpt(int campID)
    {
        return _campTabCmpts.Find(x => x.CampID == campID);
    }

    private void SetUpShipPropertyInfo(PlayerShipConfig cfg)
    {
        var root = _propertyRect.transform;

        if(propertyCmpts.Count > 0)
        {
            for (int i = propertyCmpts.Count - 1; i >= 0; i--) 
            {
                propertyCmpts[i].PoolableDestroy();
            }
        }

        var plugCfg = DataManager.Instance.GetShipPlugItemConfig(cfg.CorePlugID);
        if (plugCfg != null)
        {
            foreach (var property in plugCfg.PropertyModify)
            {
                if (property.Value == 0)
                    continue;

                PoolManager.Instance.GetObjectSync(ShipProperty_ItemPrefabPath, true, (obj) =>
                {
                    var cmpt = obj.GetComponent<ShipSelectionItemPropertyCmpt>();
                    cmpt.SetUp(property.ModifyKey, property.Value, false);
                    propertyCmpts.Add(cmpt);
                }, root);
            }

            foreach (var property in plugCfg.PropertyPercentModify)
            {
                if (property.Value == 0)
                    continue;

                PoolManager.Instance.GetObjectSync(ShipProperty_ItemPrefabPath, true, (obj) =>
                {
                    var cmpt = obj.GetComponent<ShipSelectionItemPropertyCmpt>();
                    cmpt.SetUp(property.ModifyKey, property.Value, true);
                    propertyCmpts.Add(cmpt);
                }, root);
            }
        }

        var desc = LocalizationManager.Instance.GetTextValue(cfg.ShipPropertyDesc);
        _propertyDescText.text = desc;
        _propertyDescText.transform.Find("Text").SafeGetComponent<TextMeshProUGUI>().text = desc;
        _propertyDescText.transform.SetAsLastSibling();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_propertyRect);
    }

    #endregion
}