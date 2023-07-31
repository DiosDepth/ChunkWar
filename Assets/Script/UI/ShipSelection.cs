using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipSelection : GUIBasePanel, EventListener<GeneralUIEvent>
{
    public RectTransform slotGroup;
    private Image _shipIcon;

    private EnhancedScroller _selectionScroller;
    private GeneralScrollerGirdItemController _selectionController;

    private const string ShipSelectionGridItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShipSelectionGroupItem";

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<GeneralUIEvent>();
        _selectionScroller = transform.Find("uiGroup/SlotPanel/Scroll View").SafeGetComponent<EnhancedScroller>();
        _shipIcon = transform.Find("uiGroup/ShipIconPanel/Image").SafeGetComponent<Image>();
        GetGUIComponent<Button>("Apply").onClick.AddListener(ApplyBtnPressed);
        GetGUIComponent<Button>("Back").onClick.AddListener(BackBtnPressed);
        InitPlugController();
    }



    public void ApplyBtnPressed()
    {
        GameStateTransitionEvent.Trigger(EGameState.EGameState_GamePrepare);
    }

    public void BackBtnPressed()
    {
        GameStateTransitionEvent.Trigger(EGameState.EGameState_MainMenu);
    }

    public override void Hidden( )
    {
        base.Hidden();
        this.EventStopListening<GeneralUIEvent>();
    }

    public void AddSlot(List<ItemGUISlot> list, InventoryItem m_item, RectTransform m_slotgroup)
    {
        var obj = ResManager.Instance.Load<GameObject>(UIManager.Instance.resPath + "CmptItems/ShipBuildingSlot");
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(m_slotgroup);
        rect.localScale = Vector3.one;
        

        var slotinfo = obj.GetComponent<ItemGUISlot>();
        list.Add(slotinfo);
        slotinfo.Initialization(list.Count - 1, uiGroup.GetComponent<RectTransform>(), m_item);
    }
    public void RemoveSlot(int m_index)
    {

    }

    private void InitPlugController()
    {
        _selectionController = new GeneralScrollerGirdItemController();
        _selectionController.numberOfCellsPerRow = 11;
        _selectionController.InitPrefab(ShipSelectionGridItem_PrefabPath, true);
        _selectionController.OnItemSelected += OnShipSelect;
        _selectionScroller.Delegate = _selectionController;
        RefreshShipSelectionContent();
    }


    /// <summary>
    /// 刷新插件物品栏
    /// </summary>
    private void RefreshShipSelectionContent()
    {
        var items = GameHelper.GetAllShipIDs();
        _selectionController.RefreshData(items);
        _selectionScroller.ReloadData();
    }

    /// <summary>
    /// 刷新舰船显示
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="dataIndex"></param>
    private void OnShipSelect(uint uid, int dataIndex)
    {
        ///RefreshInfo
        var shipCfg = DataManager.Instance.GetShipConfig((int)uid);
        if (shipCfg == null)
            return;

        _shipIcon.sprite = shipCfg.GeneralConfig.IconSprite;
    }

    public void OnEvent(GeneralUIEvent evt)
    {
        switch (evt.type)
        {
            case UIEventType.ShipSelectionChange:
                uint uid = (uint)evt.param[0] ;
                int dataIndex = (int)evt.param[1];
                OnShipSelect(uid, dataIndex);
                break;
        }
    }
}