using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardLevelSelectPage : GUIBasePanel
{
    private EnhancedScroller _hardLevelScroller;
    private GeneralScrollerItemController _hardLevelController;

    public CanvasGroup HardLevelGroup;

    private const string HardLevelItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/HardLevelModeItem";

    protected override void Awake()
    {
        base.Awake();
        _hardLevelScroller = transform.Find("Content/Scroll View").SafeGetComponent<EnhancedScroller>();
        HardLevelGroup = transform.Find("Content").SafeGetComponent<CanvasGroup>();
    }

    public override void Initialization()
    {
        base.Initialization();
        InitHardLevelController();
        RefreshHardLevelSelection();
    }

    public override void Hidden()
    {
        base.Hidden();
        _hardLevelController.Clear();
    }

    /// <summary>
    /// Ë¢ÐÂhardLevelÑ¡Ôñ
    /// </summary>
    private void RefreshHardLevelSelection()
    {
        var currentSelectShip = RogueManager.Instance.currentShipSelection;
        if (currentSelectShip == null)
            return;

        GameManager.Instance.RefreshHardLevelByShip(currentSelectShip.itemconfig.ID);

        var hardLevels = GameHelper.GetAllHardLevelsByShipID(currentSelectShip.itemconfig.ID);
        _hardLevelController.RefreshData(hardLevels);
        _hardLevelScroller.ReloadData();
    }

    private void InitHardLevelController()
    {
        _hardLevelController = new GeneralScrollerItemController();
        _hardLevelController.InitPrefab(HardLevelItem_PrefabPath, false);
        _hardLevelController.OnItemSelected = OnHardLevelSelect;
        _hardLevelScroller.Delegate = _hardLevelController;
    }

    private void OnHardLevelSelect(uint uid)
    {

    }
}
