using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum UI_WeaponUnitPropertyType
{
    NONE,
    HP,
    Damage,
    DamageRatio,
    Critical,
    Range,
    CD,
    ShieldTransfixion,
    ShieldDamage,
}

public enum UI_DroneFactoryPropertyType
{
    NONE,
    HP,
    DroneCount,
    DroneRange,
}

public enum UI_ShieldGeneratorPropertyType
{
    NONE,
    ShieldHP,
    ShieldRatio
}

public class UnitPropertyItemCmpt : MonoBehaviour,IPoolable
{
    public UI_WeaponUnitPropertyType WeaponPropertyType = UI_WeaponUnitPropertyType.NONE;
    public UI_DroneFactoryPropertyType DroneFactoryProeprtyType = UI_DroneFactoryPropertyType.NONE;
    public UI_ShieldGeneratorPropertyType ShieldPropertyType = UI_ShieldGeneratorPropertyType.NONE;

    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _valueText;

    public void Awake()
    {
        _nameText = transform.Find("NameBG/Name").SafeGetComponent<TextMeshProUGUI>();
        _valueText = transform.Find("Value").SafeGetComponent<TextMeshProUGUI>();
    }

    public void SetUpWeapon(UI_WeaponUnitPropertyType type, string content)
    {
        this.WeaponPropertyType = type;
        _nameText.text = GameHelper.GetUI_WeaponUnitPropertyType(type);
        _valueText.text = content;
    }

    public void SetUpDroneFactory(UI_DroneFactoryPropertyType type, string content)
    {
        this.DroneFactoryProeprtyType = type;
        _nameText.text = GameHelper.GetUI_DroneFactoryPropertyType(type);
        _valueText.text = content;
    }

    public void SetUpShield(UI_ShieldGeneratorPropertyType type, string content)
    {
        this.ShieldPropertyType = type;
        _nameText.text = GameHelper.GetUI_ShieldPropertyType(type);
        _valueText.text = content;
    }

    public void RefreshContent(string content)
    {
        _valueText.text = content;
    }


    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableReset()
    {
        ShieldPropertyType = UI_ShieldGeneratorPropertyType.NONE;
        WeaponPropertyType = UI_WeaponUnitPropertyType.NONE;
        DroneFactoryProeprtyType = UI_DroneFactoryPropertyType.NONE;
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }


}
