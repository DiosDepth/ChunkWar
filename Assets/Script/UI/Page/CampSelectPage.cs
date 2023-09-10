using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampSelectPage : GUIBasePanel
{
    private EnhancedScroller _contentScroller;
    private GeneralScrollerItemController _controller;

    private static string CampItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/CampSelectItem";

    protected override void Awake()
    {
        base.Awake();
        _contentScroller = transform.Find("Content/Scroll View").SafeGetComponent<EnhancedScroller>();
        InitController();
    }

    public override void Initialization()
    {
        base.Initialization();
        GetGUIComponent<Button>("BackBtn").onClick.AddListener(OnBackBtnClick);
        RefreshCampContent();
    }

    private void InitController()
    {
        _controller = new GeneralScrollerItemController();
        _controller.InitPrefab(CampItem_PrefabPath, false);
        _controller.OnItemSelected = OnCampItemSelect;
        _contentScroller.Delegate = _controller;
    }

    /// <summary>
    /// ˢ�²����Ʒ��
    /// </summary>
    private void RefreshCampContent()
    {
        var items = GameHelper.GetAllCampIDs();
        _controller.RefreshData(items);
        _contentScroller.ReloadData();
    }

    private void OnBackBtnClick()
    {
        UIManager.Instance.HiddenUI("CampSelectPage");
    }

    private void OnCampItemSelect(uint uid)
    {

    }
}
