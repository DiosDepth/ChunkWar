using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemCreateType
{
    Achievement,
}

public class ItemCreateEditor : OdinEditorWindow
{
    [HideInInspector]
    private ItemCreateType CreateType;

    private bool GetErrorState()
    {
        return _itemCreateError;
    }

    [InfoBox("ID重复!", InfoMessageType.Error, "GetErrorState")]
    [Button("自动获取ID", ButtonSizes.Large)]
    private void AutoGetIndex()
    {
        if (CreateType == ItemCreateType.Achievement)
        {
            CurrentCreateItemID = AchievementDataOverview.Instance.GetNextIndex();
        }
        OnItemIDChange();
    }


    [LabelText("ID")]
    [OnValueChanged("OnItemIDChange")]
    [ReadOnly]
    public int CurrentCreateItemID = 0;

    [OnInspectorInit]
    private void OnItemIDChange()
    {
        if (CreateType == ItemCreateType.Achievement)
        {
            _itemCreateError = CurrentCreateItemID <= 0 || AchievementDataOverview.Instance.IsAchievementIDExists(CurrentCreateItemID);
        }
    }
    [HideInEditorMode]
    public bool _itemCreateError = true;

    [DisableIf("_itemCreateError")]
    [Button("创建", ButtonSizes.Large, Style = ButtonStyle.CompactBox, ButtonHeight = 80), GUIColor(0, 1, 0)]
    private void CrateNewItem()
    {
        if (_itemCreateError)
            return;

        if (CreateType == ItemCreateType.Achievement)
        {
            CreateStorageItem();
        }
        Close();
    }

    private void CreateStorageItem()
    {
        string fileName = string.Format("Achievement_{0}", CurrentCreateItemID);
        var itemAssets = SimEditorUtility.CreateAssets<AchievementItemConfig>("Assets/Resources/Configs/Achievement", fileName, obj =>
        {
            obj.AchievementID = CurrentCreateItemID;
            AchievementDataOverview.Instance.AddInfoToRefDic(CurrentCreateItemID, obj);
            var parentWin = GetWindow<AchievementEditor>();
            parentWin.TrySelectMenuItemWithObject(obj);
        });
        Debug.Log("创建成功");
    }

    public static void OpenCreateWindow(ItemCreateType createType)
    {
        var window = GetWindow<ItemCreateEditor>("配置创建");
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(400, 600);
        window.maxSize = new Vector2(400, 600);
        window.CreateType = createType;
        window.Show();
    }

}
