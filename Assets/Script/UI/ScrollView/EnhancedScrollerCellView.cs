using UnityEngine;
using System;
using System.Collections.Generic;

public interface IScrollGirdCmpt
{
    public int DataIndex { get; set; }
    public uint ItemUID { get; set; }
}

public delegate void SelectedDelegate(IScrollGirdCmpt cellView);

public class EnhancedScrollerCellView :MonoBehaviour, IScrollGirdCmpt
{
    /// <summary>
    /// The cellIdentifier is a unique string that allows the scroller
    /// to handle different types of cells in a single list. Each type
    /// of cell should have its own identifier
    /// </summary>
    public string cellIdentifier;

    /// <summary>
    /// Index
    /// </summary>
    [NonSerialized]
    public int cellIndex;

    /// <summary>
    /// The data index of the cell view
    /// </summary>
    public int DataIndex { get; set; }

    public uint ItemUID { get; set; }

    /// <summary>
    /// Whether the cell is active or recycled
    /// </summary>
    [NonSerialized]
    public bool active;

    public SelectedDelegate selected;

    protected SelectableItemBase _item;

    protected virtual void Awake()
    {

    }

    /// <summary>
    /// This method is called by the scroller when the RefreshActiveCellViews is called on the scroller
    /// You can override it to update your cell's view ItemUID
    /// </summary>
    public virtual void RefreshCellView()
    {
    }

    public virtual void SetSelected()
    {
        if (selected != null)
            selected.Invoke(this);
    }

    public virtual void SetData(int index, SelectableItemBase item)
    {
        this._item = item;
        DataIndex = index;
        ItemUID = (uint)item.content;

        if (_item != null) //Remove Old
            _item.selectedChanged -= SelectedAction;

        _item.selectedChanged -= SelectedAction;
        _item.selectedChanged += SelectedAction;
        SelectedAction(_item.Selected);
        RefreshCellView();
    }

    public virtual void SetGridData(ref List<SelectableItemBase> data, int startingIndex, SelectedDelegate selected)
    {

    }

    private void SelectedAction(bool selected)
    {
        SelectedChanged(selected);
    }

    protected virtual void SelectedChanged(bool selected)
    {

    }
}