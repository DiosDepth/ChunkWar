using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pause : GUIBasePanel
{
    private Transform _unitContentTrans;
    private Transform _plugContentTrans;
    private Text _propertyBtnText;

    private RectTransform contentRect;
    private ShipPropertyGroupPanel _propertyGroup;
    private List<GeneralPreviewItemSlot> _items = new List<GeneralPreviewItemSlot>();

    private const string SlotPrefabPath = "Prefab/GUIPrefab/Weiget/GeneralPreviewItemSlot";
    private const string PropertyBtnSwitch_Main = "ShipMainProperty_Btn_Text";
    private const string PropertyBtnSwitch_Sub = "ShipSubProperty_Btn_Text";

    protected override void Awake()
    {
        base.Awake();
        var btn = transform.Find("Content/PropertyPanel/PropertyTitle/PropertyBtn").SafeGetComponent<Button>();
        btn.onClick.AddListener(OnShipPropertySwitchClick);
        _propertyBtnText = btn.transform.Find("Text").SafeGetComponent<Text>();
        _propertyGroup = transform.Find("Content/PropertyPanel/PropertyGroup").SafeGetComponent<ShipPropertyGroupPanel>();
        contentRect = transform.Find("Content/SlotContent/Viewport/Content").SafeGetComponent<RectTransform>();
        _unitContentTrans = contentRect.Find("UnitContent");
        _plugContentTrans = contentRect.Find("ItemContent");
    }

    public override void Initialization()
    {
        base.Initialization();
        ClearSlot();
        InitContent();
        OnShipPropertySwitchClick();
    }

    public override void Hidden()
    {
        ClearSlot();
        base.Hidden();
        GameManager.Instance.UnPauseGame();
    }

    private async void InitContent()
    {
        var allUnits = RogueManager.Instance.AllShipUnits;
        for(int i = 0; i < allUnits.Count; i++)
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
        await UniTask.NextFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    private void ClearSlot()
    {
        for(int i = _items.Count - 1; i >= 0; i--)
        {
            _items[i].PoolableDestroy();
        }
    }

    private void OnShipPropertySwitchClick()
    {
        _propertyGroup.SwitchGroupType();
        if (_propertyGroup.CurrentGroupType == ShipPropertyGroupPanel.GroupType.Main)
        {
            _propertyBtnText.text = LocalizationManager.Instance.GetTextValue(PropertyBtnSwitch_Main);
        }
        else
        {
            _propertyBtnText.text = LocalizationManager.Instance.GetTextValue(PropertyBtnSwitch_Sub);
        }
    }
}
