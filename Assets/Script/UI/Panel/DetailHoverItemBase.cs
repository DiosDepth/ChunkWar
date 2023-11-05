using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetailHoverItemBase : GUIBasePanel, IPoolable, IUIHoverPanel
{
    private CanvasGroup _mainCanvas;
    protected RectTransform _contentRect;
    protected RectTransform _mainRect;
    private Transform _unitInfoRoot;
    private Transform _propertyModifyRoot;
    private Transform _droneFactoryRoot;
    private RectTransform _tagRoot;

    protected Image _icon;
    protected Image _rarityBG;
    protected TextMeshProUGUI _nameText;
    protected TextMeshProUGUI _descText;
    protected TextMeshProUGUI _effectDesc1;
    protected TextMeshProUGUI _effectDesc2;

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
        _droneFactoryRoot = _contentRect.Find("DroneInfo");
        _unitInfoRoot = _contentRect.Find("UnitInfo");
        _tagRoot = _contentRect.Find("Info/Detail/TypeInfo").SafeGetComponent<RectTransform>();
        _propertyModifyRoot = _contentRect.Find("PropertyModify");
        _icon = _contentRect.Find("Info/Icon/Image").SafeGetComponent<Image>();
        _rarityBG = _contentRect.Find("Info/Icon/BG").SafeGetComponent<Image>();
        _nameText = _contentRect.Find("Info/Detail/Name").GetComponent<TextMeshProUGUI>();
        _descText = _contentRect.Find("Desc").SafeGetComponent<TextMeshProUGUI>();
        _effectDesc1 = _contentRect.Find("EffectDesc").SafeGetComponent<TextMeshProUGUI>();
        _effectDesc2 = _contentRect.Find("EffectDesc/Desc").SafeGetComponent<TextMeshProUGUI>();
    }

    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
        int UnitID = (int)param[0];
        SetUp(UnitID);
    }

    public virtual void Update()
    {
        if (_enable)
        {
            UpdatePosition();
        }
    }

    protected virtual void SetUp(int id, bool updatePosition = true)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRect);
        _enable = true;

        RectWidth = _contentRect.rect.width * _contentRect.localScale.x;
        RectHeight = _contentRect.rect.height * _contentRect.localScale.y;

        if (updatePosition)
        {
            UpdatePosition();
        }
        
        _mainCanvas.ActiveCanvasGroup(true);
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        UIManager.Instance.BackPoolerUI(transform.name, gameObject);
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
        _droneFactoryRoot.SafeSetActive(false);
        _unitInfoRoot.Pool_BackAllChilds(UnitInfo_PropertyItem_PrefabPath);
        if (cfg.unitType == UnitType.Weapons || cfg.unitType == UnitType.MainWeapons)
        {
            WeaponConfig weaponCfg = cfg as WeaponConfig;
            if (weaponCfg == null)
                return;

            foreach (UI_WeaponUnitPropertyType type in System.Enum.GetValues(typeof(UI_WeaponUnitPropertyType)))
            {
                if (type == UI_WeaponUnitPropertyType.ShieldTransfixion || type == UI_WeaponUnitPropertyType.NONE)
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
        else if (cfg.unitType == UnitType.MainBuilding || cfg.unitType == UnitType.Buildings)
        {
            BuildingConfig buildingCfg = cfg as BuildingConfig;

            ///BuildingGeneral
            foreach (UI_BuildingBasePropertyType type in System.Enum.GetValues(typeof(UI_BuildingBasePropertyType)))
            {
                ///Drone单独处理
                if ((buildingCfg is DroneFactoryConfig && type == UI_BuildingBasePropertyType.HP) || type == UI_BuildingBasePropertyType.NONE)
                    continue;

                PoolManager.Instance.GetObjectSync(UnitInfo_PropertyItem_PrefabPath, true, (obj) =>
                {
                    string content = GameHelper.GetBuildingBasePropertyDescContent(type, buildingCfg);
                    var cmpt = obj.GetComponent<UnitPropertyItemCmpt>();
                    cmpt.SetUpBuilding(type, content);
                }, _unitInfoRoot);
            }

            ///护盾描述
            if (buildingCfg.ShieldConfig.GenerateShield)
            {
                foreach (UI_ShieldGeneratorPropertyType type in System.Enum.GetValues(typeof(UI_ShieldGeneratorPropertyType)))
                {
                    if (type == UI_ShieldGeneratorPropertyType.NONE)
                        continue;

                    PoolManager.Instance.GetObjectSync(UnitInfo_PropertyItem_PrefabPath, true, (obj) =>
                    {
                        string content = GameHelper.GetShieldGeneratorPropertyDescContent(type, buildingCfg);
                        var cmpt = obj.GetComponent<UnitPropertyItemCmpt>();
                        cmpt.SetUpShield(type, content);
                    }, _unitInfoRoot);
                }
            }

            if(buildingCfg is DroneFactoryConfig)
            {
                _droneFactoryRoot.SafeSetActive(true);
                _droneFactoryRoot.Pool_BackAllChilds(UnitInfo_PropertyItem_PrefabPath);
                var droneFactoryCfg = buildingCfg as DroneFactoryConfig;
                int orderIndex = 0;
                foreach (UI_DroneFactoryPropertyType type in System.Enum.GetValues(typeof(UI_DroneFactoryPropertyType)))
                {
                    if (type == UI_DroneFactoryPropertyType.NONE)
                        continue;

                    PoolManager.Instance.GetObjectSync(UnitInfo_PropertyItem_PrefabPath, true, (obj) =>
                    {
                        string content = GameHelper.GetDroneFactoryPropertyDescContent(type, droneFactoryCfg);
                        var cmpt = obj.GetComponent<UnitPropertyItemCmpt>();
                        cmpt.SetUpDroneFactory(type, content);
                        obj.transform.SetSiblingIndex(orderIndex);
                        orderIndex++;
                    }, _droneFactoryRoot);
                }
                
                ///DroneWeapon
                var droneCfg = DataManager.Instance.GetDroneConfig(droneFactoryCfg.DroneID);
                if(droneCfg != null)
                {
                    var name = _droneFactoryRoot.transform.Find("DroneName").SafeGetComponent<TextMeshProUGUI>();
                    name.text = LocalizationManager.Instance.GetTextValue(droneCfg.GeneralConfig.Name);
                    name.color = GameHelper.GetRarityColor(droneCfg.GeneralConfig.Rarity);

                    var droneWeaponCfg = DataManager.Instance.GetUnitConfig(droneCfg.PreviewWeaponID);
                    if (droneWeaponCfg != null)
                    {
                        WeaponConfig cfg_w = droneWeaponCfg as WeaponConfig;
                        ///DroneWeapon
                        foreach (UI_WeaponUnitPropertyType type in System.Enum.GetValues(typeof(UI_WeaponUnitPropertyType)))
                        {
                            if (type == UI_WeaponUnitPropertyType.ShieldTransfixion || type == UI_WeaponUnitPropertyType.NONE)
                                continue;

                            if (!cfg_w.UseDamageRatio && type == UI_WeaponUnitPropertyType.DamageRatio)
                                continue;

                            PoolManager.Instance.GetObjectSync(UnitInfo_PropertyItem_PrefabPath, true, (obj) =>
                            {
                                string content = GameHelper.GetWeaponPropertyDescContent(type, cfg_w);
                                var cmpt = obj.GetComponent<UnitPropertyItemCmpt>();
                                cmpt.SetUpWeapon(type, content);
                            }, _unitInfoRoot);
                        }
                    }
                }
            }
        }
    }

    protected void SetUpProperty(ShipPlugDataItemConfig cfg)
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
