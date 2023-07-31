using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSelectionGroupItemCmpt : EnhancedScrollerCellView
{
    public ShipSelectionItemCmpt[] items;

    protected override void Awake()
    {
        base.Awake();
        items = transform.GetComponentsInChildren<ShipSelectionItemCmpt>();
    }

    public override void SetGridData(ref List<SelectableItemBase> data, int startingIndex, SelectedDelegate selected)
    {
        base.SetGridData(ref data, startingIndex, selected);
        for (int i = 0; i < items.Length; i++)
        {
            var dataIndex = startingIndex + i;

            // if the sub cell is outside the bounds of the data, we pass null to the sub cell
            items[i].SetDataGrid(dataIndex, dataIndex < data.Count ? data[dataIndex] : null, selected);
        }
    }
}
