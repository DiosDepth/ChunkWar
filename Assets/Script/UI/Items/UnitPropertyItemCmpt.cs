using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum UI_WeaponUnitPropertyType
{
    HP,
    Damage,
    DamageRatio,
    Critical,
    Range,
    CD,
    ShieldTransfixion,
    ShieldDamage,
}

public class UnitPropertyItemCmpt : MonoBehaviour,IPoolable
{
    public UI_WeaponUnitPropertyType WeaponPropertyType;


    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _valueText;

    public void Awake()
    {
        _nameText = transform.Find("Name").SafeGetComponent<TextMeshProUGUI>();
        _valueText = transform.Find("Value").SafeGetComponent<TextMeshProUGUI>();
    }

    public void SetUpWeapon(UI_WeaponUnitPropertyType type, string content)
    {
        this.WeaponPropertyType = type;
        _nameText.text = GameHelper.GetUI_WeaponUnitPropertyType(type);
        _valueText.text = content;
    }

    public void RefreshContent(string content)
    {
        _valueText.text = content;
    }


    public void PoolableDestroy()
    {

    }

    public void PoolableReset()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }


}
