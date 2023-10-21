using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipSelectionItemCmpt : EnhancedScrollerCellView, IHoverUIItem
{

    private Image _icon;
    private Image _classIcon;
    private Transform _lockTrans;
    private Transform _selectedTrans;

    protected override void Awake()
    {
        _icon = transform.Find("Content/Icon").SafeGetComponent<Image>();
        _classIcon = transform.Find("Content/ShipClassIcon").SafeGetComponent<Image>();
        _lockTrans = transform.Find("Lock");
        _selectedTrans = transform.Find("Selected");
        transform.SafeGetComponent<GeneralHoverItemControl>().item = this;
        transform.SafeGetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    public override void SetData(int index, SelectableItemBase item)
    {
        base.SetData(index, item);
    }

    public override void RefreshCellView()
    {
        base.RefreshCellView();
        SetUp(ItemUID);
    }

    private void SetUp(uint uid)
    {
        ItemUID = uid;
        var ship = DataManager.Instance.GetShipConfig((int)uid);
        if (ship == null)
            return;

        _icon.sprite = ship.GeneralConfig.IconSprite;
        var classCfg = DataManager.Instance.gameMiscCfg.GetShipClassConfig(ship.ShipClass);
        if(classCfg != null)
        {
            _classIcon.sprite = classCfg.ClassIcon;
        }

        var shipUnlock = SaveLoadManager.Instance.globalSaveData.GetShipUnlockState(ship.ID);
        _lockTrans.SafeSetActive(!shipUnlock);
    }

    protected override void SelectedChanged(bool selected)
    {
        base.SelectedChanged(selected);
        _selectedTrans.SafeSetActive(selected);
    }

    private void OnButtonClick()
    {
        var shipCfg = DataManager.Instance.GetShipConfig((int)ItemUID);
        var unlock = SaveLoadManager.Instance.globalSaveData.GetShipUnlockState(shipCfg.ID);
        if (!unlock)
            return;

        InventoryItem item = new InventoryItem(shipCfg);
        RogueManager.Instance.currentShipSelection = item;
        GeneralUIEvent.Trigger(UIEventType.ShipSelectionConfirm);
    }

    public void OnHoverEnter()
    {
        RogueManager.Instance.SetTempShipSelectionPreview((int)ItemUID);
        GeneralUIEvent.Trigger(UIEventType.ShipSelectionChange, ItemUID);
        SoundManager.Instance.PlayUISound("UI_Click_Hover");
        selected?.Invoke(this);
    }

    public void OnHoverExit()
    {

    }
}
