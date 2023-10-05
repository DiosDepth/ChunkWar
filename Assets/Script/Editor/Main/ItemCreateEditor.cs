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
    LevelPreset,
    Bullet
}

public enum BulletCreateType
{
    Beam,
    ChainBeam,
    InstanceHit,
    Projectile
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
        else if (CreateType == ItemCreateType.LevelPreset)
        {
            CurrentCreateItemID = LevelPresetDataOverview.Instance.GetNextIndex();
        }
        OnItemIDChange();
    }


    [LabelText("ID")]
    [OnValueChanged("OnItemIDChange")]
    public int CurrentCreateItemID = 0;

    [ShowIf("CreateType", ItemCreateType.Bullet)]
    public BulletCreateType BulletType;

    [OnInspectorInit]
    private void OnItemIDChange()
    {
        if (CreateType == ItemCreateType.Achievement)
        {
            _itemCreateError = CurrentCreateItemID <= 0 || AchievementDataOverview.Instance.IsAchievementIDExists(CurrentCreateItemID);
        }
        else if (CreateType == ItemCreateType.LevelPreset)
        {
            _itemCreateError = CurrentCreateItemID <= 0 || LevelPresetDataOverview.Instance.IsLevelPresetIDExists(CurrentCreateItemID);
        }
        else if(CreateType == ItemCreateType.Bullet)
        {
            _itemCreateError = CurrentCreateItemID <= 0 || BulletDataOverview.Instance.IsBulletIDExists(CurrentCreateItemID);
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
        else if (CreateType == ItemCreateType.LevelPreset)
        {
            CreateLevelPreset();
        }
        else if(CreateType == ItemCreateType.Bullet)
        {
            CreateBullet();
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

    private void CreateBullet()
    {
        string fileName = string.Format("Bullet_{0}", CurrentCreateItemID);
        if(BulletType == BulletCreateType.Beam)
        {
            SimEditorUtility.CreateAssets<BulletBeamConfig>("Assets/Resources/Configs/Bullets", fileName, obj =>
            {
                OnBulletCreate(obj);
            });
        }
        else if(BulletType == BulletCreateType.ChainBeam)
        {
            SimEditorUtility.CreateAssets<BulletChainBeamConfig>("Assets/Resources/Configs/Bullets", fileName, obj =>
            {
                OnBulletCreate(obj);
            });
        }
        else if (BulletType == BulletCreateType.InstanceHit)
        {
            SimEditorUtility.CreateAssets<BulletInstanceHitConfig>("Assets/Resources/Configs/Bullets", fileName, obj =>
            {
                OnBulletCreate(obj);
            });
        }
        else if (BulletType == BulletCreateType.Projectile)
        {
            SimEditorUtility.CreateAssets<BulletProjectileConfig>("Assets/Resources/Configs/Bullets", fileName, obj =>
            {
                OnBulletCreate(obj);
            });
        }

        Debug.Log("创建成功");
    }

    private void OnBulletCreate(BulletConfig cfg)
    {
        cfg.ID = CurrentCreateItemID;
        BulletDataOverview.Instance.AddInfoToRefDic(CurrentCreateItemID, cfg);
        var parentWin = GetWindow<BulletEditor>();
        parentWin.TrySelectMenuItemWithObject(cfg);
    }

    private void CreateLevelPreset()
    {
        string fileName = string.Format("LevelPreset_{0}", CurrentCreateItemID);
        var itemAssets = SimEditorUtility.CreateAssets<LevelSpawnConfig>("Assets/Resources/Configs/Levels", fileName, obj =>
        {
            obj.LevelPresetID = CurrentCreateItemID;
            LevelPresetDataOverview.Instance.AddInfoToRefDic(CurrentCreateItemID, obj);
            var parentWin = GetWindow<LevelPresetEditor>();
            parentWin.TrySelectMenuItemWithObject(obj);
        });
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
