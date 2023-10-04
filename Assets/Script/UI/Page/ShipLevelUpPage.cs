using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipLevelUpPage : GUIBasePanel, EventListener<ShipPropertyEvent>, EventListener<RogueEvent>
{
    private ShipPropertyGroupPanel _propertyPanel;
    private List<ShipLevelUpSelectItem> _items;
    private TextMeshProUGUI _rerollText;
    private TextMeshProUGUI _currencyText;
    private TextMeshProUGUI _levelText; 
    private RectTransform _currencyRect;
    private RectTransform _rerollRect;

    private static string ShipLevelUpSelectItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShipLevelUpSelectItem";
    /// <summary>
    /// 升级次数
    /// </summary>
    private int levelUpCount;
    private byte oldLevel;

    protected override void Awake()
    {
        base.Awake();
        _items = new List<ShipLevelUpSelectItem>();
        _propertyPanel = transform.Find("Content/Property/PropertyGroup").SafeGetComponent<ShipPropertyGroupPanel>();
        transform.Find("Content/Property/PropertyTitle/PropertyBtn").SafeGetComponent<Button>().onClick.AddListener(SwitchPropertyGroup);
        _currencyRect = transform.Find("Content/SelectContent/Title/Currency").SafeGetComponent<RectTransform>();
        _currencyText = _currencyRect.Find("Value").SafeGetComponent<TextMeshProUGUI>();
        _levelText = transform.Find("Content/SelectContent/Title/Level/Level").SafeGetComponent<TextMeshProUGUI>();
        var rerollBtn = transform.Find("Content/SelectContent/Reroll/Button").SafeGetComponent<Button>();
        _rerollRect = rerollBtn.transform.Find("Content").SafeGetComponent<RectTransform>();
        rerollBtn.onClick.AddListener(OnRerollButtonClick);
        _rerollText = rerollBtn.transform.Find("Content/Value").SafeGetComponent<TextMeshProUGUI>();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<ShipPropertyEvent>();
        SwitchPropertyGroup();
        RefreshRerollCost();
        RefreshCurrencyText();
    }

    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
        levelUpCount = (int)param[1];
        oldLevel = (byte)param[0];
        ShowShipLevelUpItem();
    }

    public override void Hidden()
    {
        this.EventStopListening<ShipPropertyEvent>();
        base.Hidden();
    }

    public void OnEvent(ShipPropertyEvent evt)
    {
        switch (evt.type)
        {
            case ShipPropertyEventType.MainPropertyValueChange:
                var modifyKey = (PropertyModifyKey)evt.param[0];
                _propertyPanel.RefreshPropertyByKey(modifyKey);
                break;
        }
    }

    public void OnEvent(RogueEvent evt)
    {
        switch (evt.type)
        {
            case RogueEventType.CurrencyChange:
                RefreshCurrencyText();
                break;
        }
    }

    public void AddUpgradeLevelCount(int count)
    {
        levelUpCount += count;
    }

    private void ShowShipLevelUpItem()
    {
        if(_items.Count == 0)
        {
            var root = transform.Find("Content/SelectContent/Content");
            ///InitItems
            var count = DataManager.Instance.battleCfg.ShipLevelUp_GrowthItem_Count;
            for(int i = 0; i < count; i++)
            {
                PoolManager.Instance.GetObjectSync(ShipLevelUpSelectItem_PrefabPath, true, (obj) =>
                {
                    var cmpt = obj.transform.SafeGetComponent<ShipLevelUpSelectItem>();
                    _items.Add(cmpt);
                }, root);
            }
        }

        var items = RogueManager.Instance.CurrentShipLevelUpItems;
        if(items.Count > _items.Count)
        {
            Debug.LogError("升级数量不匹配!");
            return;
        }

        for (int i = 0; i < items.Count; i++) 
        {
            var cmpt = _items[i];
            cmpt.SetUp(items[i], OnItemSelect);
        }
        _levelText.text = string.Format("LV.{0}", oldLevel);
    }

    /// <summary>
    /// 是否完成
    /// </summary>
    private void OnItemSelect()
    {
        levelUpCount--;
        if(levelUpCount <= 0)
        {
            ///Close
            UIManager.Instance.HiddenUI("ShipLevelUpPage");
            InputDispatcher.Instance.ChangeInputMode("Player");
            GameManager.Instance.UnPauseGame();
            RogueManager.Instance.IsShowingShipLevelUp = false;
        }
        else
        {
            oldLevel++;
            ///Refresh New
            RogueManager.Instance.RefreshShipLevelUpItems(false);
            ShowShipLevelUpItem();
        }
    }

    private void OnRerollButtonClick()
    {
        RogueManager.Instance.RefreshShipLevelUpItems(true);
        ShowShipLevelUpItem();
        RefreshRerollCost();
    }

    private void SwitchPropertyGroup()
    {
        _propertyPanel.SwitchGroupType();
    }

    /// <summary>
    /// 刷新
    /// </summary>
    private void RefreshRerollCost()
    {
        _rerollText.text = string.Format("-{0}", RogueManager.Instance.CurrentLevelUpitemRerollCost);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rerollRect);
    }

    private void RefreshCurrencyText()
    {
        _currencyText.text = RogueManager.Instance.CurrentCurrency.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_currencyRect);
    }
}
