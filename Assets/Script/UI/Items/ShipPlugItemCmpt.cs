using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipPlugItemCmpt : MonoBehaviour, IScrollGirdCmpt
{
    public SelectedDelegate selected;

    public int DataIndex { get; set; }
    public uint ItemUID { get; set; }

    private Image _icon;
    private TextMeshProUGUI _countText;

    private SelectableItemBase _item;

    public void Awake()
    {
        _icon = transform.Find("Content/Icon").SafeGetComponent<Image>();
        _countText = transform.Find("Content/Value").SafeGetComponent<TextMeshProUGUI>();
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
        var plug = RogueManager.Instance.GetShopGoodsInfo((int)uid);
        if (plug == null)
            return;

        var count = RogueManager.Instance.GetCurrentGoodsCount(plug.GoodsID);
        if(count > 1)
        {
            _countText.text = count.ToString();
            _countText.transform.SafeSetActive(true);
        }
        else
        {
            _countText.transform.SafeSetActive(false);
        }
        _icon.sprite = plug._cfg.IconSprite;
    }

    private void SelectedChanged(bool selected)
    {
        transform.Find("Selected").SafeSetActive(selected);
    }
}
