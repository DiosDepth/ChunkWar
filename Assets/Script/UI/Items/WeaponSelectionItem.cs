using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelectionItem : EnhancedScrollerCellView, IHoverUIItem
{

    private Image _icon;
    private Transform _lockTrans;
    private Transform _selectedTrans;

    protected override void Awake()
    {
        _icon = transform.Find("Content/Icon").SafeGetComponent<Image>();
        _lockTrans = transform.Find("Lock");
        _selectedTrans = transform.Find("Selected");
        transform.SafeGetComponent<GeneralHoverItemControl>().item = this;
        transform.SafeGetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        var unlock = SaveLoadManager.Instance.globalSaveData.GetUnitUnlockState((int)ItemUID);
        if (!unlock)
            return;

        var unitCfg = DataManager.Instance.GetUnitConfig((int)ItemUID);
        if (unitCfg == null)
            return;

        InventoryItem weaponItem = new InventoryItem(unitCfg);
        RogueManager.Instance.currentWeaponSelection = weaponItem;

        UIManager.Instance.HiddenUI("ShipSelection");
        UIManager.Instance.ShowUI<HardLevelSelectPage>("HardLevelSelectPage", E_UI_Layer.Mid, null, (panel) =>
        {
            panel.Initialization();
        });
    }

    public override void RefreshCellView()
    {
        base.RefreshCellView();
        SetUp(ItemUID);
    }

    private void SetUp(uint uid)
    {
        ItemUID = uid;
        var weapon = DataManager.Instance.GetUnitConfig((int)uid);
        if (weapon == null)
            return;

        _icon.sprite = weapon.GeneralConfig.IconSprite;

        var unlock = SaveLoadManager.Instance.globalSaveData.GetUnitUnlockState(weapon.ID);
        _lockTrans.SafeSetActive(!unlock);
    }

    public override void SetData(int index, SelectableItemBase item)
    {
        base.SetData(index, item);
    }

    protected override void SelectedChanged(bool selected)
    {
        base.SelectedChanged(selected);
        _selectedTrans.SafeSetActive(selected);
    }

    public void OnHoverEnter()
    {
        SoundManager.Instance.PlayUISound("UI_Click_Hover");
        selected?.Invoke(this);
    }

    public void OnHoverExit()
    {

    }
}
