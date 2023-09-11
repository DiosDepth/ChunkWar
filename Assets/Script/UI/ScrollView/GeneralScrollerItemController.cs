using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralScrollerItemController : IEnhancedScrollerDelegate
{
    private EnhancedScrollerCellView _cmpt;
    private float ItemHeight;
    private List<SelectableItemBase> _itemData = new List<SelectableItemBase>();
    private GameObject obj;

    public Action<uint> OnItemSelected;

    public void RefreshData(List<uint> itemData)
    {
        this._itemData = itemData.FormatToSelectableItem<uint>();
    }

    public void Clear()
    {
        GameObject.Destroy(obj);
    }

    public void InitPrefab(string prefabPath, bool vertical)
    {
        obj = ResManager.Instance.Load<GameObject>(prefabPath);
        if (obj != null)
        {
            _cmpt = obj.transform.SafeGetComponent<EnhancedScrollerCellView>();

            if (vertical)
            {
                ItemHeight = _cmpt.transform.SafeGetComponent<RectTransform>().rect.height;
            }
            else
            {
                ItemHeight = _cmpt.transform.SafeGetComponent<RectTransform>().rect.width;
            }
        }
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        EnhancedScrollerCellView view = scroller.GetCellView(_cmpt) as EnhancedScrollerCellView;
        view.name = "Item_" + dataIndex;
        view.SetData(dataIndex, _itemData[dataIndex]);
        view.selected = ItemSelected;
        return view;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return ItemHeight;
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return _itemData.Count;
    }

    private void ItemSelected(IScrollGirdCmpt view)
    {
        if (view != null)
        {
            var cmpt = view as EnhancedScrollerCellView;
            if (cmpt != null)
            {
                var index = cmpt.DataIndex;
                for (var i = 0; i < _itemData.Count; i++)
                {
                    var selected = (index == i);
                    _itemData[i].Selected = selected;
                    if (selected)
                    {
                        OnItemSelected?.Invoke(cmpt.ItemUID);
                    }
                }
            }
        }
    }
}