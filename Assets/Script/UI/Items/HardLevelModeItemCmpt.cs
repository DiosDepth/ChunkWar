using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HardLevelModeItemCmpt : EnhancedScrollerCellView, IHoverUIItem
{
    private Image _icon;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _descText;
    private TextMeshProUGUI _propertyDescText;
    private TextMeshProUGUI _previewScoreText;
    private RectTransform _propertyRoot;

    private List<HardLevelPropertyItemCmpt> propertyCmpts = new List<HardLevelPropertyItemCmpt>();
    private static string PropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/HardLevelPropertyItem";

    private int count;
    protected override void Awake()
    {
        _propertyRoot = transform.Find("Content/Property/Viewport/Content").SafeGetComponent<RectTransform>();
        _icon = transform.Find("Content/Icon/Image").SafeGetComponent<Image>();
        _nameText = transform.Find("Content/Info/Name").SafeGetComponent<TextMeshProUGUI>();
        _descText = transform.Find("Content/Desc").SafeGetComponent<TextMeshProUGUI>();
        _previewScoreText = transform.Find("Content/Info/HardScore/Value").SafeGetComponent<TextMeshProUGUI>();
        _propertyDescText = transform.Find("Content/Property/Viewport/Content/Desc").SafeGetComponent<TextMeshProUGUI>();
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
        _propertyDescText.text = hardLevelInfo.PropertyDesc;
        _previewScoreText.text = hardLevelInfo.Cfg.PreviewScore.ToString();
        _propertyDescText.transform.SetAsLastSibling();

        LayoutRebuilder.ForceRebuildLayoutImmediate(_propertyRoot);
        InitFinishMark();
    }

    private void OnButtonClick()
    {
        (UIManager.Instance.GetGUIFromDic("HardLevelSelectPage") as HardLevelSelectPage).HardLevelGroup.interactable = false;
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
        if (propertyCmpts.Count > 0)
        {
            for (int i = propertyCmpts.Count - 1; i >= 0; i--)
            {
                propertyCmpts[i].PoolableDestroy();
            }
        }
        propertyCmpts.Clear();

        foreach(var item in dic)
        {
            PoolManager.Instance.GetObjectSync(PropertyItem_PrefabPath, true, (obj) =>
            {
                var cmpt = obj.GetComponent<HardLevelPropertyItemCmpt>();
                cmpt.SetUp(item.Key,item.Value);
                propertyCmpts.Add(cmpt);
            }, _propertyRoot);
        }
    }

    private void InitFinishMark()
    {
        var currentSelectShip = RogueManager.Instance.currentShipSelection;
        if (currentSelectShip != null)
        {
            var hardLevelSav = SaveLoadManager.Instance.globalSaveData.GetShipHardLevelSaveData(currentSelectShip.itemconfig.ID, (int)ItemUID);
            if(hardLevelSav != null)
            {
                transform.Find("Finish").SafeSetActive(hardLevelSav.Finish);
                return;
            }
        }

        transform.Find("Finish").SafeSetActive(false);
    }
}
