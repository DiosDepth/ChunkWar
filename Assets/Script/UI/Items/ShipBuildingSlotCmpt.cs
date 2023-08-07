using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ShipBuildingSlotCmpt : MonoBehaviour
{
    public byte Index;

    public int UnitID
    {
        get;
        private set;
    }

    private Image _icon;

    public bool IsEmpty
    {
        get { return _isEmpty; }
    }

    private bool _isEmpty = true;

    private void Awake()
    {
        _icon = transform.Find("Image").SafeGetComponent<Image>();
        transform.Find("Slot_Btn").SafeGetComponent<Button>().onClick.AddListener(OnBuildingSlotSelected);
        SetEmpty();
    }

    public void SetEmpty()
    {
        _isEmpty = true;
        UnitID = 0;
        _icon.transform.SafeSetActive(!IsEmpty);
    }

    public void SetUp(int unitID)
    {
        var unitcfg = DataManager.Instance.GetUnitConfig(unitID);
        if(unitcfg == null)
        {
            SetEmpty();
            return;
        }
        _icon.sprite = unitcfg.GeneralConfig.IconSprite;
        _isEmpty = false;
        _icon.transform.SafeSetActive(!IsEmpty);
        this.UnitID = unitID;
    }

    private void OnBuildingSlotSelected()
    {
        if (_isEmpty)
            return;

        var cfg = DataManager.Instance.GetUnitConfig(UnitID);
        var shipBuilder = ShipBuilder.instance;
        if (shipBuilder == null || cfg == null)
            return;

        RogueManager.Instance.CurrentSelectedHarborSlotIndex = Index;
        shipBuilder.currentInventoryItem = new InventoryItem(cfg);
        shipBuilder.SetBrushSprite();
    }
}
