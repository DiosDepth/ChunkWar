using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PunishHoverItem : GUIBasePanel, IPoolable, IUIHoverPanel
{
    private CanvasGroup _mainCanvas;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _descText;
    private bool _enable = false;

    private Transform _punishGroup;
    private Transform _punishNone;

    private ShipPropertySliderCmpt.SliderPropertyType sliderType;

    private static string BuffPunishItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/BuffPunishItem";

    protected override void Awake()
    {
        _mainCanvas = transform.SafeGetComponent<CanvasGroup>();
        _nameText = transform.Find("Content/Title").SafeGetComponent<TextMeshProUGUI>();
        _descText = transform.Find("Content/Content").SafeGetComponent<TextMeshProUGUI>();
        _punishGroup = transform.Find("Content/BuffPunishItem/PunishGroup");
        _punishNone = transform.Find("Content/BuffPunishItem/Punish_None");
    }

    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
        sliderType = (ShipPropertySliderCmpt.SliderPropertyType)param[0];
        UpdatePosition();
        SetUp();
        _enable = true;
    }

    void SetUp()
    {
        _nameText.text = GameHelper.GetShipPropertySliderName(sliderType);
        _descText.text = GameHelper.GetShipPropertySliderDesc(sliderType);

        bool isPunish = false;

        if (sliderType == ShipPropertySliderCmpt.SliderPropertyType.Energy)
        {
            isPunish = GameHelper.GetPlayerShipEnergyCostPercent() > 100;
        }
        else if(sliderType == ShipPropertySliderCmpt.SliderPropertyType.Load)
        {
            isPunish = RogueManager.Instance.WreckageLoadPercent > 100;
        }

        SetUpPunish(isPunish);
    }

    public void Update()
    {
        if (_enable)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        transform.localPosition = UIManager.Instance.GetUIPosByMousePos();
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

    private void SetUpPunish(bool punish)
    {
        _punishGroup.SafeSetActive(punish);
        _punishNone.SafeSetActive(!punish);

        if (punish)
        {
            _punishGroup.Pool_BackAllChilds(BuffPunishItem_PrefabPath);
            if(sliderType == ShipPropertySliderCmpt.SliderPropertyType.Energy)
            {
                var energyBuff = RogueManager.Instance.GetGlobalPropertyModifySpelcialData(GameGlobalConfig.PropertyModifyUID_EnergyOverload_GlobalBuff);
                for (int i = 0; i < energyBuff.Length; i++) 
                {
                    var propertyKey = energyBuff[i].Config.ModifyKey;
                    var disPlayCfg = DataManager.Instance.battleCfg.GetPropertyDisplayConfig(propertyKey);
                    float value = RogueManager.Instance.MainPropertyData.GetValueByModifyUID(propertyKey, PropertyModifyType.Modify, GameGlobalConfig.PropertyModifyUID_EnergyOverload_GlobalBuff);
                    PoolManager.Instance.GetObjectSync(BuffPunishItem_PrefabPath, true, (obj) =>
                    {
                        var cmpt = obj.transform.SafeGetComponent<BuffPunishItem>();
                        cmpt.SetUp(disPlayCfg, value);
                    }, _punishGroup);
                }
            }
            else if(sliderType == ShipPropertySliderCmpt.SliderPropertyType.Load)
            {
                var loadBuff = RogueManager.Instance.GetGlobalPropertyModifySpelcialData(GameGlobalConfig.PropertyModifyUID_WreckageOverload_GlobalBuff);
                for (int i = 0; i < loadBuff.Length; i++)
                {
                    var propertyKey = loadBuff[i].Config.ModifyKey;
                    var disPlayCfg = DataManager.Instance.battleCfg.GetPropertyDisplayConfig(propertyKey);
                    float value = RogueManager.Instance.MainPropertyData.GetValueByModifyUID(propertyKey, PropertyModifyType.Modify, GameGlobalConfig.PropertyModifyUID_WreckageOverload_GlobalBuff);
                    PoolManager.Instance.GetObjectSync(BuffPunishItem_PrefabPath, true, (obj) =>
                    {
                        var cmpt = obj.transform.SafeGetComponent<BuffPunishItem>();
                        cmpt.SetUp(disPlayCfg, value);
                    }, _punishGroup);
                }
            }

        }
    }

}
