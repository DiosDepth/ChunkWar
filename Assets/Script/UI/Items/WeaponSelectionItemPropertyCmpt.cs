using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeaponSelectionItemPropertyCmpt : MonoBehaviour, IPoolable
{
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _valueText;

    public void Awake()
    {
        _nameText = transform.Find("PropertyName").SafeGetComponent<TextMeshProUGUI>();
        _valueText = transform.Find("ValueContent/PropertyValue").SafeGetComponent<TextMeshProUGUI>();
    }

    public void SetUpWeapon(UI_WeaponUnitPropertyType type, string content)
    {
        _nameText.text = GameHelper.GetUI_WeaponUnitPropertyType(type);
        _valueText.text = content;
    }

    public void SetUpDroneFactory(UI_DroneFactoryPropertyType type, string content)
    {
        _nameText.text = GameHelper.GetUI_DroneFactoryPropertyType(type);
        _valueText.text = content;
    }

    public void RefreshContent(string content)
    {
        _valueText.text = content;
    }

    public void PoolableDestroy()
    {
        PoolManager.Instance.BackObject(gameObject.name, gameObject);
    }

    public void PoolableReset()
    {
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }
}
