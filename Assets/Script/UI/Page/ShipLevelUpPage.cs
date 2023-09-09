using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipLevelUpPage : GUIBasePanel, EventListener<ShipPropertyEvent>
{
    private ShipPropertyGroupPanel _propertyPanel;
    private List<ShipLevelUpSelectItem> _items;
    private TextMeshProUGUI _rerollText;

    private static string ShipLevelUpSelectItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShipLevelUpSelectItem";
    /// <summary>
    /// ��������
    /// </summary>
    private int levelUpCount;

    protected override void Awake()
    {
        base.Awake();
        _items = new List<ShipLevelUpSelectItem>();
        _propertyPanel = transform.Find("Content/Property/PropertyGroup").SafeGetComponent<ShipPropertyGroupPanel>();
        transform.Find("Content/Property/PropertyTitle/PropertyBtn").SafeGetComponent<Button>().onClick.AddListener(SwitchPropertyGroup);
        transform.Find("Reroll/Button").SafeGetComponent<Button>().onClick.AddListener(OnRerollButtonClick);
        _rerollText = transform.Find("Reroll/Button/Content/Value").SafeGetComponent<TextMeshProUGUI>();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<ShipPropertyEvent>();
        SwitchPropertyGroup();
        RefreshRerollCost();
    }

    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
        levelUpCount = (int)param[0];
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

    private void ShowShipLevelUpItem()
    {
        if(_items.Count == 0)
        {
            var root = transform.Find("Content/SelectContent");
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
            Debug.LogError("����������ƥ��!");
            return;
        }

        for (int i = 0; i < items.Count; i++) 
        {
            var cmpt = _items[i];
            cmpt.SetUp(items[i], OnItemSelect);
        }
    }

    /// <summary>
    /// �Ƿ����
    /// </summary>
    private void OnItemSelect()
    {
        levelUpCount--;
        if(levelUpCount <= 0)
        {
            ///Close
            UIManager.Instance.HiddenUI("ShipLevelUpPage");
            InputDispatcher.Instance.ChangeInputMode("Player");
            RogueManager.Instance.Resume();
        }
        else
        {
            ///Refresh New
            RogueManager.Instance.RefreshShipLevelUpItems(false);
            ShowShipLevelUpItem();
        }
    }

    private void OnRerollButtonClick()
    {

    }

    private void SwitchPropertyGroup()
    {
        _propertyPanel.SwitchGroupType();
    }

    private void RefreshRerollCost()
    {
        _rerollText.text = RogueManager.Instance.CurrentLevelUpitemRerollCost.ToString();
    }
}