using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipPlugItemCmpt : MonoBehaviour, IScrollGirdCmpt, IHoverUIItem
{
    public SelectedDelegate selected;

    public int DataIndex { get; set; }
    public uint ItemUID { get; set; }

    private Image _icon;
    private Image _rarityBG;
    private TextMeshProUGUI _countText;

    private SelectableItemBase _item;
    private DetailHoverItemBase _hoverItem;

    public void Awake()
    {
        _rarityBG = transform.Find("BG").SafeGetComponent<Image>();
        _icon = transform.Find("Content/Icon").SafeGetComponent<Image>();
        _countText = transform.Find("Content/Value").SafeGetComponent<TextMeshProUGUI>();
        transform.Find("BG").SafeGetComponent<GeneralHoverItemControl>().item = this;
    }

    public void SetDataGrid(int dataIndex, SelectableItemBase item, SelectedDelegate selected)
    {
        this.selected = selected;
        _item = item;
        transform.SafeGetComponent<CanvasGroup>().ActiveCanvasGroup(item != null);
        if (item == null)
            return;

        ItemUID = (uint)item.content;
        SetUp(ItemUID);
        _item.selectedChanged -= SelectedChanged;

        DataIndex = dataIndex;

        _item.selectedChanged -= SelectedChanged;
        _item.selectedChanged += SelectedChanged;
        SelectedChanged(_item.Selected);
    }

    private void SetUp(uint uid)
    {
        var count = RogueManager.Instance.GetCurrentPlugCount((int)uid);
        _countText.transform.SafeSetActive(count > 1);
        _countText.text = count.ToString();

        var plugCfg = DataManager.Instance.GetShipPlugItemConfig((int)uid);
        if (plugCfg != null) 
        {
            _icon.sprite = plugCfg.GeneralConfig.IconSprite;
            _rarityBG.sprite = GameHelper.GetRarityBGSprite(plugCfg.GeneralConfig.Rarity);
        }
    }

    private void SelectedChanged(bool selected)
    {
        transform.Find("Selected").SafeSetActive(selected);
    }

    public void OnHoverEnter()
    {
        if (_item == null)
            return;

        SoundManager.Instance.PlayUISound(SoundEventStr.Mouse_PointOver_2);
        UIManager.Instance.CreatePoolerUI<PlugDetailHover>("PlugDetailHover", true, E_UI_Layer.Top, null, (panel) =>
        {
            panel.Initialization((int)ItemUID);
            _hoverItem = panel;
        });
    }

    public void OnHoverExit()
    {
        if (_hoverItem != null)
        {
            _hoverItem.PoolableDestroy();
        }
    }

    private void OnDestroy()
    {
        if (_hoverItem != null)
        {
            _hoverItem.PoolableDestroy();
        }
    }
}
