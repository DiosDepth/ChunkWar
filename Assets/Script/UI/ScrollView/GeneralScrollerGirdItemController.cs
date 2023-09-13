using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralScrollerGirdItemController : IEnhancedScrollerDelegate
{
    private EnhancedScrollerCellView _cmpt;
    private float ItemHeight;
    private List<SelectableItemBase> _itemData = new List<SelectableItemBase>();

    public int numberOfCellsPerRow = 3;

    public Action<uint, int> OnItemSelected;

    private GameObject obj;

    public void Clear()
    {
        GameObject.Destroy(obj);
    }

    public void RefreshData(List<uint> itemData)
    {
        this._itemData = itemData.FormatToSelectableItem<uint>();
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
        var di = dataIndex * numberOfCellsPerRow;

        view.name = "Cell " + (di).ToString() + " to " + ((di) + numberOfCellsPerRow - 1).ToString();
        view.SetGridData(ref _itemData, di, ItemSelected);
        return view;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return ItemHeight;
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return Mathf.CeilToInt((_itemData.Count / (float)numberOfCellsPerRow));
    }

    public void CalcelSelectAll()
    {
        for (var i = 0; i < _itemData.Count; i++)
        {
            _itemData[i].Selected = false;
        }
    }

    private void ItemSelected(IScrollGirdCmpt view)
    {
        if (view != null)
        {
            int dataIndex = view.DataIndex;
            uint uid = view.ItemUID;

            if (dataIndex == -1)
                return;

            for (var i = 0; i < _itemData.Count; i++)
            {
                var selected = (dataIndex == i);
                _itemData[i].Selected = selected;
                if (selected)
                {
                    OnItemSelected?.Invoke(uid, dataIndex);
                }
            }
        }
    }
}