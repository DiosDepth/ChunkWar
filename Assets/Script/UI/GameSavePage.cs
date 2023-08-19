using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSavePage : GUIBasePanel
{

    private EnhancedScroller _scrollerView;
    private GeneralScrollerItemController _itemController;
    private CanvasGroup _emptyCanvas;
    private CanvasGroup _rightPanelCanvas;
    private CanvasGroup _rightPanelEmptyCanvas;

    private const string SaveItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/GameSaveItem";

    protected override void Awake()
    {
        base.Awake();
        _scrollerView = transform.Find("Content/LeftPanel/Scroll View").SafeGetComponent<EnhancedScroller>();
        _emptyCanvas = transform.Find("Content/LeftPanel/Empty").SafeGetComponent<CanvasGroup>();
        _rightPanelCanvas = transform.Find("Content/RightPanel").SafeGetComponent<CanvasGroup>();
        _rightPanelEmptyCanvas = transform.Find("Content/Empty").SafeGetComponent<CanvasGroup>();
    }

    public override void Initialization()
    {
        base.Initialization();
        GetGUIComponent<Button>("Delete").onClick.AddListener(DeleteCurrentSaveClick);
        GetGUIComponent<Button>("Load").onClick.AddListener(LoadCurrentSaveClick);
        GetGUIComponent<Button>("BackBtn").onClick.AddListener(OnBackBtnClick);
        InitPage();
    }

    private void InitPage()
    {
        IniSaveController();
        RefreshSaveSelection();
    }

    private void IniSaveController()
    {
        _itemController = new GeneralScrollerItemController();
        _itemController.InitPrefab(SaveItem_PrefabPath, true);
        _itemController.OnItemSelected = OnSaveItemSelect;
        _scrollerView.Delegate = _itemController;
    }

    /// <summary>
    /// Ë¢ÐÂ´æµµÑ¡Ôñ
    /// </summary>
    private void RefreshSaveSelection()
    {
        var saves = GameHelper.GetAllSaveIDs();
        bool empty = saves == null || saves.Count <= 0;
        _emptyCanvas.ActiveCanvasGroup(empty);
        _scrollerView.transform.SafeSetActive(!empty);
        _rightPanelEmptyCanvas.ActiveCanvasGroup(empty);
        _rightPanelCanvas.ActiveCanvasGroup(!empty);

        if (!empty)
        {
            _itemController.RefreshData(saves);
            _scrollerView.ReloadData();
            SetFirstChoose();
        }
    }

    private void OnSaveItemSelect(uint uid)
    {

    }

    private void SetFirstChoose()
    {
        var item = _scrollerView.GetCellViewAtDataIndex(0);
        if (item != null && item is GameSaveItemCmpt)
        {
            var saveItem = item as GameSaveItemCmpt;
            saveItem.SetSelected();
        }
    }

    private void DeleteCurrentSaveClick()
    {

    }

    private void LoadCurrentSaveClick()
    {

    }

    private void OnBackBtnClick()
    {
        UIManager.Instance.HiddenUI("GameSavePage");
    }


}
