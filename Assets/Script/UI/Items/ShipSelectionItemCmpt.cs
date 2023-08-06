using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipSelectionItemCmpt : MonoBehaviour, IScrollGirdCmpt, IHoverUIItem
{
    public SelectedDelegate selected;

    public int DataIndex { get; set; }
    public uint ItemUID { get; set; }

    private Image _icon;

    private SelectableItemBase _item;

    public void Awake()
    {
        _icon = transform.Find("Content/Icon").SafeGetComponent<Image>();
        transform.SafeGetComponent<GeneralHoverItemControl>().item = this;
        transform.SafeGetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    public void SetDataGrid(int dataIndex, SelectableItemBase item, SelectedDelegate selected)
    {
        this.selected = selected;
        transform.SafeGetComponent<CanvasGroup>().ActiveCanvasGroup(item != null);
        if (item != null)
        {
            uint itemUID = (uint)item.content;
            SetUp(itemUID);
        }

        if (_item != null)
        {
            _item.selectedChanged -= SelectedChanged;
        }
        DataIndex = dataIndex;
        _item = item;
        if (item != null)
        {
            _item.selectedChanged -= SelectedChanged;
            _item.selectedChanged += SelectedChanged;
            SelectedChanged(_item.Selected);
        }
    }

    private void SetUp(uint uid)
    {
        ItemUID = uid;
        var ship = DataManager.Instance.GetShipConfig((int)uid);
        if (ship == null)
            return;

        _icon.sprite = ship.GeneralConfig.IconSprite;
    }

    private void OnButtonClick()
    {
        var shipCfg = DataManager.Instance.GetShipConfig((int)ItemUID);
        InventoryItem item = new InventoryItem(shipCfg);
        RogueManager.Instance.currentShipSelection = item;
        GeneralUIEvent.Trigger(UIEventType.ShipSelectionConfirm);
    }

    private void SelectedChanged(bool select)
    {
        transform.Find("Selected").SafeSetActive(select);
    }

    public void OnHoverEnter()
    {
        GeneralUIEvent.Trigger(UIEventType.ShipSelectionChange, ItemUID, DataIndex);
        selected?.Invoke(this);
    }

    public void OnHoverExit()
    {

    }
}
