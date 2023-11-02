using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipSelection : GUIBasePanel, EventListener<GeneralUIEvent>
{
    public enum ShipSelectionPhase
    {
        NONE,
        ShipSelection,
        WeaponSelection
    }

    private Image _shipIcon;
    private Image _weaponIcon;
    private Text _nameText;
    private TextMeshProUGUI _weaponNameText;
    private Image _classIcon;
    private TextMeshProUGUI _className;
    private TextMeshProUGUI _descText;
    private Image _campIcon;
    private TextMeshProUGUI _propertyDescText;
    private TextMeshProUGUI _weaponPropertyDescText;

    private EnhancedScroller _selectionScroller;
    private GeneralScrollerItemController _selectionController;
    private EnhancedScroller _weaponSelectionScroller;
    private GeneralScrollerItemController _weaponSelectionController;

    private CanvasGroup _weaponGroup;
    private CanvasGroup _shipSelectionGroup;
    private CanvasGroup _shipSelectionScroCanvas;
    private CanvasGroup _weaponSelectionScroCanvas;
    private CanvasGroup _campTabGroup;

    private RectTransform _infoGroupRect;
    private RectTransform _weaponInfoGroupRect;
    private RectTransform _propertyRect;
    private RectTransform _weaponPropertyRect;
    private List<CampSelectionTabCmpt> _campTabCmpts;
    private List<ShipSelectionItemPropertyCmpt> propertyCmpts = new List<ShipSelectionItemPropertyCmpt>();
    private List<WeaponSelectionItemPropertyCmpt> weaponPropertyCmpts = new List<WeaponSelectionItemPropertyCmpt>();

    private const string ShipSelectionGridItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShipSelectionItem";
    private const string WeaponSelectionItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/WeaponSelectionItem";
    private const string CampSelectionTabItem = "Prefab/GUIPrefab/CmptItems/CampSelectionTab";
    private const string ShipProperty_ItemPrefabPath = "Prefab/GUIPrefab/CmptItems/ShipSelectionItemProperty";
    private const string WeaponPropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/WeaponSelectionItemProperty";

    private const string ShipSelectionProeprty_Unknow = "ShipSelectionProeprty_Unknow";
    private const string ShipSelection_Button_WeaponSelect = "ShipSelection_Button_WeaponSelect";
    private const string ShipSelection_Button_HardLevelSelect = "ShipSelection_Button_HardLevelSelect";

    private uint currentSelectShipID = 0;
    private uint currentSelectWeaponID = 0;
    private int currentSelectCampID = 0;

    private ShipSelectionPhase currentPhase = ShipSelectionPhase.NONE;

    protected override void Awake()
    {
        base.Awake();
        _campTabCmpts = new List<CampSelectionTabCmpt>();

        _weaponGroup = transform.Find("uiGroup/Content/Top/ShipInfo/WeaponGroup").SafeGetComponent<CanvasGroup>();
        _shipSelectionGroup = transform.Find("uiGroup/Content/Top/ShipInfo/Group").SafeGetComponent<CanvasGroup>();
        _campTabGroup = transform.Find("uiGroup/Content/CampTab").SafeGetComponent<CanvasGroup>();

        _weaponIcon = _weaponGroup.transform.Find("WeaponInfo/Icon").SafeGetComponent<Image>();
        _weaponNameText = _weaponGroup.transform.Find("WeaponInfo/Info/Name").SafeGetComponent<TextMeshProUGUI>();

        var shipInfoTrans = transform.Find("uiGroup/Content/Top/ShipInfo");
        _infoGroupRect = shipInfoTrans.Find("Group").SafeGetComponent<RectTransform>();
        _weaponInfoGroupRect = shipInfoTrans.Find("WeaponGroup").SafeGetComponent<RectTransform>();
        _propertyRect = transform.Find("uiGroup/Content/Top/PropertyInfo/PropertyContent/Viewport/Content").SafeGetComponent<RectTransform>();
        _weaponPropertyRect = _weaponGroup.transform.Find("PropertyContent/Viewport/Content").SafeGetComponent<RectTransform>();
        _nameText = shipInfoTrans.Find("Group/ShipClassInfo/Name").SafeGetComponent<Text>();
        _classIcon = shipInfoTrans.Find("Group/ShipClassInfo/ClassInfo/ClassIcon").SafeGetComponent<Image>();
        _className = shipInfoTrans.Find("Group/ShipClassInfo/ClassInfo/ClassName").SafeGetComponent<TextMeshProUGUI>();
        _descText = shipInfoTrans.Find("Group/Desc").SafeGetComponent<TextMeshProUGUI>();
        _campIcon = transform.Find("uiGroup/Content/Top/ShipInfo/CampBG").SafeGetComponent<Image>();
        _propertyDescText = _propertyRect.Find("DescContent").SafeGetComponent<TextMeshProUGUI>();
        _weaponPropertyDescText = _weaponPropertyRect.Find("Desc").SafeGetComponent<TextMeshProUGUI>();

        _selectionScroller = transform.Find("uiGroup/Content/Selection/ShipSelection").SafeGetComponent<EnhancedScroller>();
        _shipSelectionScroCanvas = _selectionScroller.transform.SafeGetComponent<CanvasGroup>();
        _weaponSelectionScroller = transform.Find("uiGroup/Content/Selection/WeaponSelection").SafeGetComponent<EnhancedScroller>();
        _weaponSelectionScroCanvas = _weaponSelectionScroller.transform.SafeGetComponent<CanvasGroup>();
        _shipIcon = transform.Find("uiGroup/Content/Top/ShipIconPanel/Icon").SafeGetComponent<Image>();
        GetGUIComponent<Button>("GeneralBackBtn").onClick.AddListener(BackBtnPressed);
        GetGUIComponent<Button>("NextBtn").onClick.AddListener(NextPhaseBtnPressed);
        InitSelectionController();
        InitWeaponSelectionController();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<GeneralUIEvent>();
        currentPhase = ShipSelectionPhase.NONE;
        SwitchToNextProgress();
    }

    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
        this.EventStartListening<GeneralUIEvent>();
        ShipSelectionPhase phase = (ShipSelectionPhase)param[0];
        currentPhase = phase;
        SwitchToNextProgress();

        ///Refresh Ship Choose
        var currentSelectShip = RogueManager.Instance.currentShipSelection;
        if(currentSelectShip != null)
        {
            var id = (uint)currentSelectShip.itemconfig.ID;
            OnShipSelect(id);
            currentSelectShipID = id;
        }
    }

    private void BackBtnPressed()
    {
        if(currentPhase == ShipSelectionPhase.NONE || currentPhase == ShipSelectionPhase.ShipSelection)
        {
            GameStateTransitionEvent.Trigger(EGameState.EGameState_MainMenu);
        }
        else if( currentPhase == ShipSelectionPhase.WeaponSelection)
        {
            currentPhase = ShipSelectionPhase.NONE;
            SwitchToNextProgress();
        }
    }

    private void NextPhaseBtnPressed()
    {
        if(currentPhase == ShipSelectionPhase.NONE || currentPhase == ShipSelectionPhase.ShipSelection)
        {

        }
        else if (currentPhase == ShipSelectionPhase.WeaponSelection)
        {

        }
    }

    public override void Hidden( )
    {
        base.Hidden();
        this.EventStopListening<GeneralUIEvent>();
        _selectionController.Clear();
        _weaponSelectionController.Clear();
    }

    /// <summary>
    /// ÏÂÒ»½×¶Î
    /// </summary>
    private void SwitchToNextProgress()
    {
        if(currentPhase == ShipSelectionPhase.NONE)
        {
            _campTabGroup.ActiveCanvasGroup(true);
            _shipSelectionGroup.ActiveCanvasGroup(true);
            _weaponGroup.ActiveCanvasGroup(false);
            _shipSelectionScroCanvas.ActiveCanvasGroup(true);
            _weaponSelectionScroCanvas.ActiveCanvasGroup(false);
            currentPhase = ShipSelectionPhase.ShipSelection;
            InitCampTab();

        }
        else if(currentPhase == ShipSelectionPhase.ShipSelection)
        {
            _campTabGroup.ActiveCanvasGroup(false);
            _weaponGroup.ActiveCanvasGroup(true);
            _shipSelectionGroup.ActiveCanvasGroup(false);
            _shipSelectionScroCanvas.ActiveCanvasGroup(false);
            _weaponSelectionScroCanvas.ActiveCanvasGroup(true);
            RefreshWeaponSelectionContent();
            currentPhase = ShipSelectionPhase.WeaponSelection;
        }
        RefreshButtonText();
    }

    private void InitCampTab()
    {
        for (int i = _campTabCmpts.Count - 1; i >= 0; i--) 
        {
            _campTabCmpts[i].PoolableDestroy();
        }

        var allCamp = DataManager.Instance.GetAllCampConfigs();
        for(int i = 0; i < allCamp.Count; i++)
        {
            PoolManager.Instance.GetObjectSync(CampSelectionTabItem, true, (obj) =>
            {
                var cmpt = obj.transform.SafeGetComponent<CampSelectionTabCmpt>();
                cmpt.SetUp(allCamp[i].CampID);
                _campTabCmpts.Add(cmpt);
            }, _campTabGroup.transform);
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

    private void InitWeaponSelectionController()
    {
        _weaponSelectionController = new GeneralScrollerItemController();
        _weaponSelectionController.InitPrefab(WeaponSelectionItem_PrefabPath, true);
        _weaponSelectionController.OnItemSelected = OnWeaponSelect;
        _weaponSelectionScroller.Delegate = _weaponSelectionController;
    }

    /// <summary>
    /// Ë¢ÐÂ½¢´¬À¸
    /// </summary>
    private void RefreshShipSelectionContent(List<uint> shipIDs)
    {
        _selectionController.RefreshData(shipIDs);
        _selectionScroller.ReloadData();
    }

    private void RefreshWeaponSelectionContent()
    {
        var allMainWeapons = GameHelper.GetAllMainWeaponItems();
        _weaponSelectionController.RefreshData(allMainWeapons);
        _weaponSelectionScroller.ReloadData();

        ///SelectFirst
        SetFirstWeapon();
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
                SwitchToNextProgress();
                break;

            case UIEventType.ShipSelection_CampSelect:
                int campID = (int)evt.param[0];
                SelectCamp(campID);
                break;
        }
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

    private void OnWeaponSelect(uint uid)
    {
        if (currentSelectWeaponID == uid)
            return;

        currentSelectWeaponID = uid;
        var unitCfg = DataManager.Instance.GetUnitConfig((int)uid);
        if (unitCfg == null)
            return;

        _weaponIcon.sprite = unitCfg.GeneralConfig.IconSprite;
        _weaponNameText.text = LocalizationManager.Instance.GetTextValue(unitCfg.GeneralConfig.Name);

        if(unitCfg is WeaponConfig)
        {
            SetUpWeaponPropertyInfo(unitCfg as WeaponConfig);
        }
        else if(unitCfg is DroneFactoryConfig)
        {
            SetUpDroneFactoryInfo(unitCfg as DroneFactoryConfig);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(_weaponInfoGroupRect);
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

    private void SetFirstWeapon()
    {
        var item = _weaponSelectionScroller.GetCellViewAtDataIndex(0);
        if (item != null && item is WeaponSelectionItem)
        {
            var group = item as WeaponSelectionItem;
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

    private void SetUpWeaponPropertyInfo(WeaponConfig cfg)
    {
        _weaponPropertyRect.Find("DroneGroup").SafeSetActive(false);

        if (cfg == null)
            return;

        var root = _weaponPropertyRect.transform;

        if (weaponPropertyCmpts.Count > 0)
        {
            for (int i = weaponPropertyCmpts.Count - 1; i >= 0; i--)
            {
                weaponPropertyCmpts[i].PoolableDestroy();
            }
        }
        weaponPropertyCmpts.Clear();

        var weaponUnlock = SaveLoadManager.Instance.globalSaveData.GetUnitUnlockState(cfg.ID);

        foreach (UI_WeaponUnitPropertyType type in System.Enum.GetValues(typeof(UI_WeaponUnitPropertyType)))
        {
            if (type == UI_WeaponUnitPropertyType.ShieldTransfixion || type == UI_WeaponUnitPropertyType.NONE)
                continue;

            if (!cfg.UseDamageRatio && type == UI_WeaponUnitPropertyType.DamageRatio)
                continue;

            PoolManager.Instance.GetObjectSync(WeaponPropertyItem_PrefabPath, true, (obj) =>
            {
                string content = weaponUnlock ? GameHelper.GetWeaponPropertyDescContent(type, cfg) : 
                LocalizationManager.Instance.GetTextValue(ShipSelectionProeprty_Unknow);
                var cmpt = obj.GetComponent<WeaponSelectionItemPropertyCmpt>();
                cmpt.SetUpWeapon(type, content);
                weaponPropertyCmpts.Add(cmpt);
            }, root);
        }
        var desc = LocalizationManager.Instance.GetTextValue(cfg.ProertyDescText);
        _weaponPropertyDescText.text = desc;
        _weaponPropertyDescText.transform.SetAsLastSibling();

        LayoutRebuilder.ForceRebuildLayoutImmediate(_weaponPropertyRect);
    }

    private void SetUpDroneFactoryInfo(DroneFactoryConfig cfg)
    {
        if (cfg == null)
            return;

        var root = _weaponPropertyRect.transform;

        if (weaponPropertyCmpts.Count > 0)
        {
            for (int i = weaponPropertyCmpts.Count - 1; i >= 0; i--)
            {
                weaponPropertyCmpts[i].PoolableDestroy();
            }
        }
        weaponPropertyCmpts.Clear();
        var droneUnlock = SaveLoadManager.Instance.globalSaveData.GetUnitUnlockState(cfg.ID);

        int orderIndex = 0;
        ///Drone Factory
        foreach (UI_DroneFactoryPropertyType type in System.Enum.GetValues(typeof(UI_DroneFactoryPropertyType)))
        {
            if (type == UI_DroneFactoryPropertyType.NONE)
                continue;

            PoolManager.Instance.GetObjectSync(WeaponPropertyItem_PrefabPath, true, (obj) =>
            {
                string content = droneUnlock ? GameHelper.GetDroneFactoryPropertyDescContent(type, cfg) :
                LocalizationManager.Instance.GetTextValue(ShipSelectionProeprty_Unknow);
                var cmpt = obj.GetComponent<WeaponSelectionItemPropertyCmpt>();
                cmpt.SetUpDroneFactory(type, content);
                weaponPropertyCmpts.Add(cmpt);
            }, root);
            orderIndex++;
        }

        var desc = LocalizationManager.Instance.GetTextValue(cfg.ProertyDescText);
        _weaponPropertyDescText.text = desc;
       
        ///Drone
        var droneCfg = DataManager.Instance.GetDroneConfig(cfg.DroneID);
        if (droneCfg == null)
            return;

        var droneGroup = _weaponPropertyRect.Find("DroneGroup");
        droneGroup.SafeSetActive(true);
        var nameText = droneGroup.Find("DroneName").SafeGetComponent<TextMeshProUGUI>();
        nameText.text = LocalizationManager.Instance.GetTextValue(droneCfg.GeneralConfig.Name);
        nameText.color = GameHelper.GetRarityColor(droneCfg.GeneralConfig.Rarity);
 
        var droneWeaponCfg = DataManager.Instance.GetUnitConfig(droneCfg.PreviewWeaponID);
        if(droneWeaponCfg != null)
        {
            WeaponConfig cfg_w = droneWeaponCfg as WeaponConfig;
            ///DroneWeapon
            foreach (UI_WeaponUnitPropertyType type in System.Enum.GetValues(typeof(UI_WeaponUnitPropertyType)))
            {
                if (type == UI_WeaponUnitPropertyType.ShieldTransfixion || type == UI_WeaponUnitPropertyType.NONE)
                    continue;

                if (!cfg_w.UseDamageRatio && type == UI_WeaponUnitPropertyType.DamageRatio)
                    continue;

                PoolManager.Instance.GetObjectSync(WeaponPropertyItem_PrefabPath, true, (obj) =>
                {
                    string content = droneUnlock ? GameHelper.GetWeaponPropertyDescContent(type, cfg_w) :
                    LocalizationManager.Instance.GetTextValue(ShipSelectionProeprty_Unknow);
                    var cmpt = obj.GetComponent<WeaponSelectionItemPropertyCmpt>();
                    cmpt.SetUpWeapon(type, content);
                    weaponPropertyCmpts.Add(cmpt);
                }, root);
            }
        }

        _weaponPropertyDescText.transform.SetSiblingIndex(0);
        droneGroup.transform.SetSiblingIndex(orderIndex + 1);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_weaponPropertyRect);
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
        propertyCmpts.Clear();

        var shipUnlock = SaveLoadManager.Instance.globalSaveData.GetShipUnlockState(cfg.ID);

        var plugCfg = DataManager.Instance.GetShipPlugItemConfig(cfg.CorePlugID);
        if (plugCfg != null)
        {
            foreach (var property in plugCfg.PropertyModify)
            {
                if (property.Value == 0 || property.BySpecialValue) 
                    continue;

                PoolManager.Instance.GetObjectSync(ShipProperty_ItemPrefabPath, true, (obj) =>
                {
                    var cmpt = obj.GetComponent<ShipSelectionItemPropertyCmpt>();
                    cmpt.SetUp(property.ModifyKey, property.Value, false, shipUnlock);
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
                    cmpt.SetUp(property.ModifyKey, property.Value, true, shipUnlock);
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

    private void RefreshButtonText()
    {
        GetGUIComponent<Button>("GeneralBackBtn").transform.Find("Text").SafeGetComponent<TextMeshProUGUI>().text =
               LocalizationManager.Instance.GetTextValue(GameHelper.GeneralButton_Back_Text);
        if (currentPhase == ShipSelectionPhase.ShipSelection)
        {
            GetGUIComponent<Button>("NextBtn").transform.Find("Text").SafeGetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetTextValue(ShipSelection_Button_WeaponSelect);
        }
        else if(currentPhase == ShipSelectionPhase.WeaponSelection)
        {
            GetGUIComponent<Button>("NextBtn").transform.Find("Text").SafeGetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetTextValue(ShipSelection_Button_HardLevelSelect);
        }
    }
}