using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetailHoverItemBase : GUIBasePanel, IPoolable
{
    private CanvasGroup _mainCanvas;
    protected RectTransform _contentRect;
    protected RectTransform _mainRect;
    private Transform _unitInfoRoot;
    private Transform _propertyModifyRoot;
    private RectTransform _tagRoot;

    protected Image _icon;
    protected Image _rarityBG;
    protected TextMeshProUGUI _nameText;
    protected TextMeshProUGUI _descText;

    private List<ItemPropertyModifyCmpt> ModifyCmpts = new List<ItemPropertyModifyCmpt>();
    private List<ItemTagCmpt> tagCmpt = new List<ItemTagCmpt>();

    private bool _enable = false;
    private const string UnitInfo_PropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/UnitPropertyItem";
    private const string ShopPropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShopItemProperty";
    private const string ItemTag_PrefabPath = "Prefab/GUIPrefab/PoolUI/ItemTagCmpt";

    private float RectWidth;
    private float RectHeight;
    private UICornerData.CornerType currentCornerType = UICornerData.CornerType.NONE;

    protected override void Awake()
    {
        base.Awake();
        _mainCanvas = transform.SafeGetComponent<CanvasGroup>();
        _mainRect = transform.SafeGetComponent<RectTransform>();
        _contentRect = transform.Find("Content").SafeGetComponent<RectTransform>();
        _unitInfoRoot = _contentRect.Find("UnitInfo");
        _tagRoot = _contentRect.Find("Info/Detail/TypeInfo").SafeGetComponent<RectTransform>();
        _propertyModifyRoot = _contentRect.Find("PropertyModify");
        _icon = _contentRect.Find("Info/Icon/Image").SafeGetComponent<Image>();
        _rarityBG = _contentRect.Find("Info/Icon/BG").SafeGetComponent<Image>();
        _nameText = _contentRect.Find("Info/Detail/Name").GetComponent<TextMeshProUGUI>();
        _descText = _contentRect.Find("Desc").SafeGetComponent<TextMeshProUGUI>();
    }

    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
        int UnitID = (int)param[0];
        SetUp(UnitID);
    }

    public void Update()
    {
        if (_enable)
        {
            UpdatePosition();
        }
    }

    protected virtual void SetUp(int id)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRect);
        _enable = true;

        RectWidth = _contentRect.rect.width * _contentRect.localScale.x;
        RectHeight = _contentRect.rect.height * _contentRect.localScale.y;

        UpdatePosition();
        _mainCanvas.ActiveCanvasGroup(true);
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(transform.name, gameObject);
    }

    public void PoolableReset()
    {
        _enable = false;
        _mainCanvas.ActiveCanvasGroup(false);
    }

    public void PoolableSetActive(bool isactive = true)
    {

    }

    protected void SetUpUintInfo(BaseUnitConfig cfg)
    {
        _unitInfoRoot.Pool_BackAllChilds(UnitInfo_PropertyItem_PrefabPath);
        if (cfg.unitType == UnitType.Weapons || cfg.unitType == UnitType.MainWeapons)
        {
            WeaponConfig weaponCfg = cfg as WeaponConfig;
            if (weaponCfg == null)
                return;

            foreach (UI_WeaponUnitPropertyType type in System.Enum.GetValues(typeof(UI_WeaponUnitPropertyType)))
            {
                if (type == UI_WeaponUnitPropertyType.ShieldTransfixion)
                    continue;

                if (!weaponCfg.UseDamageRatio && type == UI_WeaponUnitPropertyType.DamageRatio)
                    continue;

                PoolManager.Instance.GetObjectSync(UnitInfo_PropertyItem_PrefabPath, true, (obj) =>
                {
                    string content = GameHelper.GetWeaponPropertyDescContent(type, weaponCfg);
                    var cmpt = obj.GetComponent<UnitPropertyItemCmpt>();
                    cmpt.SetUpWeapon(type, content);
                }, _unitInfoRoot);
            }
        }
    }

    protected void SetUpProperty(ShipPlugItemConfig cfg)
    {
        for(int i = ModifyCmpts.Count - 1; i >= 0; i--)
        {
            ModifyCmpts[i].PoolableDestroy();
        }
        ModifyCmpts.Clear();

        SetUpPropertyCmpt(cfg.PropertyModify, false);
        SetUpPropertyCmpt(cfg.PropertyPercentModify, true);
        var rect = _propertyModifyRoot.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

    protected void SetUpItemTag(BaseUnitConfig cfg)
    {
        for (int i = tagCmpt.Count - 1; i >= 0; i--) 
        {
            tagCmpt[i].PoolableDestroy();
        }
        tagCmpt.Clear();

        foreach (ItemTag tag in System.Enum.GetValues(typeof(ItemTag)))
        {
            if (cfg.HasUnitTag(tag))
            {
                PoolManager.Instance.GetObjectSync(ItemTag_PrefabPath, true, (obj) =>
                {
                    var cmpt = obj.transform.SafeGetComponent<ItemTagCmpt>();
                    cmpt.SetUp(tag);
                    tagCmpt.Add(cmpt);
                }, _tagRoot);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_tagRoot);
    }

    private void SetUpPropertyCmpt(PropertyModifyConfig[] cfgs, bool modifyPercent)
    {
        if (cfgs == null || cfgs.Length <= 0)
            return;

        var index = 0;
        for (int i = 0; i < cfgs.Length; i++)
        {
            if (cfgs[i].BySpecialValue)
                continue;

            PoolManager.Instance.GetObjectSync(ShopPropertyItem_PrefabPath, true, (obj) =>
            {
                var cmpt = obj.GetComponent<ItemPropertyModifyCmpt>();
                cmpt.SetUp(cfgs[index].ModifyKey, cfgs[index].Value, modifyPercent);
                ModifyCmpts.Add(cmpt);
                index++;
            }, _propertyModifyRoot);
        }
    }


    #region Position
    private void UpdatePosition()
    {
        transform.localPosition = UIManager.Instance.GetUIPosByMousePos();
        CheckBounds();
    }

    /// <summary>
    /// 检测是否超出屏幕
    /// </summary>
    private void CheckBounds()
    {
        float windowWidth = Screen.width;
        float windowHeight = Screen.height;
        var _mousePosition = UIManager.Instance.CurrentMousePosition;
        ///超出右边界，设置锚点为右
        if (_mousePosition.x + RectWidth > windowWidth)
        {
            ///超出顶部，设置锚点右上
            if (_mousePosition.y + RectHeight > windowHeight)
            {
                SetCorner(UICornerData.CornerType.RightTop);
            }
            else if (_mousePosition.y - RectHeight < 0) 
            {
                SetCorner(UICornerData.CornerType.RightDown);
            }
        }
        ///超出左边界，设置锚点为左
        else if (_mousePosition.x - RectWidth < 0)
        {
            if (_mousePosition.y + RectHeight > windowHeight)
            {
                SetCorner(UICornerData.CornerType.LeftTop);
            }
            else if(_mousePosition.y - RectHeight < 0)
            {
                SetCorner(UICornerData.CornerType.LeftDown);
            }
        }
        ///超出上边界，设置锚点为上
        else if (_mousePosition.y + RectHeight > windowHeight)
        {
            ///超出右边界
            if (_mousePosition.x + RectWidth > windowWidth)
            {
                SetCorner(UICornerData.CornerType.RightTop);
            }
            else if(_mousePosition.x - RectWidth < 0)
            {
                SetCorner(UICornerData.CornerType.LeftTop);
            }
        }
        ///超出下边界，设置锚点为下
        else if (_mousePosition.y - RectHeight < 0)
        {
            ///超出右边界
            if (_mousePosition.x + RectWidth > windowWidth)
            {
                SetCorner(UICornerData.CornerType.RightDown);
            }
            else
            {
                SetCorner(UICornerData.CornerType.LeftDown);
            }
        }
        else if (_mousePosition.y - RectHeight >= 0 && _mousePosition.y + RectHeight <= windowHeight &&
            _mousePosition.x - RectWidth >= 0 && _mousePosition.x + RectWidth <= windowWidth) 
        {
            SetCorner(UICornerData.CornerType.LeftTop);
        }
    }

    private void SetCorner(UICornerData.CornerType type)
    {
        if (currentCornerType == type)
            return;

        currentCornerType = type;
        var cornerData = Utility.GetCornerData(type);
        if (cornerData == null)
            return;

        _mainRect.pivot = new Vector2(cornerData.PivotX, cornerData.PivotY);
        _contentRect.pivot = new Vector2(cornerData.PivotX, cornerData.PivotY);
        _contentRect.transform.localPosition = Vector2.zero;
    }

    #endregion
}
