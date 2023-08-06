using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HardLevelModeItemCmpt : EnhancedScrollerCellView, IHoverUIItem
{
    private Image _icon;
    private Text _nameText;
    private TextMeshProUGUI _descText;
    private Transform _propertyRoot;

    private static string PropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/HardLevelPropertyItem";

    protected override void Awake()
    {
        _propertyRoot = transform.Find("Content/PropertyContent");
        _icon = transform.Find("Content/Icon/Image").SafeGetComponent<Image>();
        _nameText = transform.Find("Content/Name").SafeGetComponent<Text>();
        _descText = transform.Find("Content/Desc").SafeGetComponent<TextMeshProUGUI>();
        transform.SafeGetComponent<GeneralHoverItemControl>().item = this;
        transform.SafeGetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    public override void SetData(int index, SelectableItemBase item)
    {
        base.SetData(index, item);
    }

    public override void RefreshCellView()
    {
        var hardLevelInfo = GameManager.Instance.GetHardLevelInfoByID((int)ItemUID);
        if (hardLevelInfo == null)
            return;

        SetUpProperty(hardLevelInfo.Cfg.ModifyDic);
        _nameText.text = hardLevelInfo.HardLevelName;
        _descText.text = hardLevelInfo.HardLevelDesc;
        _icon.sprite = hardLevelInfo.Cfg.Icon;
    }

    private void OnButtonClick()
    {
        GameStateTransitionEvent.Trigger(EGameState.EGameState_GamePrepare);
        var hardLevelInfo = GameManager.Instance.GetHardLevelInfoByID((int)ItemUID);
        RogueManager.Instance.SetCurrentHardLevel(hardLevelInfo);
    }


    protected override void SelectedChanged(bool selected)
    {
        base.SelectedChanged(selected);
        transform.Find("Select").SafeSetActive(selected);
    }

    public void OnHoverEnter()
    {
        SelectedChanged(true);
    }

    public void OnHoverExit()
    {
        SelectedChanged(false);
    }

    private void SetUpProperty(Dictionary<HardLevelModifyType, float> dic)
    {
        _propertyRoot.Pool_BackAllChilds(PropertyItem_PrefabPath);
        foreach(var item in dic)
        {
            PoolManager.Instance.GetObjectSync(PropertyItem_PrefabPath, true, (obj) =>
            {
                var cmpt = obj.GetComponent<HardLevelPropertyItemCmpt>();
                cmpt.SetUp(item.Key,item.Value);
            }, _propertyRoot);
        }
    }
}
