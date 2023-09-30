using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOver : GUIBasePanel
{
    private Transform _unitContentTrans;
    private Transform _plugContentTrans;
    private RectTransform contentRect;
    private RectTransform leftPanelRect;
    private Text _propertyBtnText;
    private TextMeshProUGUI _shipNameText;
    private Image _campIcon;
    private TextMeshProUGUI _battleResultText;
    private TextMeshProUGUI _hardLevelText;
    private TextMeshProUGUI _scoreText;

    private List<GeneralPreviewItemSlot> _items = new List<GeneralPreviewItemSlot>();
    private ShipPropertyGroupPanel _propertyGroup;

    private const string SlotPrefabPath = "Prefab/GUIPrefab/Weiget/GeneralPreviewItemSlot";
    private const string PropertyBtnSwitch_Main = "ShipMainProperty_Btn_Text";
    private const string PropertyBtnSwitch_Sub = "ShipSubProperty_Btn_Text";

    private const string GameOver_SuccessText = "GameOver_SuccessText";
    private const string GameOver_FailText = "GameOver_FailText";
    private static Color32 successColor = new Color32(255, 186, 0, 255);
    private static Color32 failColor = new Color32(255, 54, 54, 255);

    protected override void Awake()
    {
        base.Awake();
        contentRect = transform.Find("Scroll View/Viewport/Content").SafeGetComponent<RectTransform>();
        leftPanelRect = contentRect.Find("LeftPanel").SafeGetComponent<RectTransform>();
        var propertyPanelTrans = contentRect.Find("LeftPanel/PropertyPanel");
        _propertyGroup = propertyPanelTrans.Find("PropertyGroup").SafeGetComponent<ShipPropertyGroupPanel>();
        var btn = propertyPanelTrans.Find("PropertyTitle/PropertyBtn").SafeGetComponent<Button>();
        btn.onClick.AddListener(OnShipPropertySwitchClick);
        _propertyBtnText = btn.transform.Find("Text").SafeGetComponent<Text>();

        _shipNameText = contentRect.Find("LeftPanel/ShipName").SafeGetComponent<TextMeshProUGUI>();
        _unitContentTrans = contentRect.Find("RightPanel/UnitContent");
        _plugContentTrans = contentRect.Find("RightPanel/PlugContent");
        _campIcon = contentRect.Find("RightPanel/CampInfo/Icon").SafeGetComponent<Image>();
        _battleResultText = contentRect.Find("RightPanel/CampInfo/Info/Result/Text").SafeGetComponent<TextMeshProUGUI>();
        _hardLevelText = contentRect.Find("RightPanel/CampInfo/Info/Hard/Text").SafeGetComponent<TextMeshProUGUI>();
        _scoreText = contentRect.Find("RightPanel/CampInfo/Info/Score/Value").SafeGetComponent<TextMeshProUGUI>();

        GetGUIComponent<Button>("MainMenu").onClick.AddListener(OnMainMenuClick);
        GetGUIComponent<Button>("ReStart").onClick.AddListener(OnReStartClick);
        GetGUIComponent<Button>("NewGame").onClick.AddListener(OnNewGameClick);
    }

    public override void Initialization()
    {
        base.Initialization();
        SetUpInfo();
        InitContent();
        OnShipPropertySwitchClick();
    }

    private void InitContent()
    {
        ClearSlot();
        var allUnits = RogueManager.Instance.AllShipUnits;
        for (int i = 0; i < allUnits.Count; i++)
        {
            PoolManager.Instance.GetObjectSync(SlotPrefabPath, true, (obj) =>
            {
                var cmpt = obj.transform.SafeGetComponent<GeneralPreviewItemSlot>();
                cmpt.SetUp(GoodsItemType.ShipUnit, allUnits[i].UnitID);
                _items.Add(cmpt);
            }, _unitContentTrans);
        }

        var allPlugs = RogueManager.Instance.AllCurrentShipPlugs;
        for (int i = 0; i < allPlugs.Count; i++)
        {
            PoolManager.Instance.GetObjectSync(SlotPrefabPath, true, (obj) =>
            {
                var cmpt = obj.transform.SafeGetComponent<GeneralPreviewItemSlot>();
                cmpt.SetUp(GoodsItemType.ShipPlug, allPlugs[i].PlugID);
                _items.Add(cmpt);
            }, _plugContentTrans);
        }
    }

    private void ClearSlot()
    {
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            _items[i].PoolableDestroy();
        }
    }

    private void SetUpInfo()
    {
        var battleResult = RogueManager.Instance.BattleResult;
        var currentShip = RogueManager.Instance.currentShip;
        _shipNameText.text = LocalizationManager.Instance.GetTextValue(currentShip.playerShipCfg.GeneralConfig.Name);
        var currentHardLevelInfo = RogueManager.Instance.CurrentHardLevel;
        _hardLevelText.text = LocalizationManager.Instance.GetTextValue(currentHardLevelInfo.Cfg.Name);
        var shipCamp = GameHelper.GetCampCfgByShipID(currentShip.playerShipCfg.ID);
        if (shipCamp != null)
        {
            _campIcon.sprite = shipCamp.CampIcon;
        }

        _battleResultText.text = battleResult.Success ? LocalizationManager.Instance.GetTextValue(GameOver_SuccessText) : LocalizationManager.Instance.GetTextValue(GameOver_FailText);
        _battleResultText.color = battleResult.Success ? successColor : failColor;
        _scoreText.text = battleResult.Score.ToString();
    }

    private void OnShipPropertySwitchClick()
    {
        _propertyGroup.SwitchGroupTypeAndForceRebuild();
        if (_propertyGroup.CurrentGroupType == ShipPropertyGroupPanel.GroupType.Main)
        {
            _propertyBtnText.text = LocalizationManager.Instance.GetTextValue(PropertyBtnSwitch_Main);
        }
        else
        {
            _propertyBtnText.text = LocalizationManager.Instance.GetTextValue(PropertyBtnSwitch_Sub);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(leftPanelRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    private void OnMainMenuClick()
    {
        UIManager.Instance.HiddenUI("GameOver");
        GameManager.Instance.ClearBattle();
        GameEvent.Trigger(EGameState.EGameState_MainMenu);
    }

    private void OnReStartClick()
    {

    }

    private void OnNewGameClick()
    {

    }
}
