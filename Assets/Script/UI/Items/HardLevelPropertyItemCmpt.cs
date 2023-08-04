using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HardLevelPropertyItemCmpt : PoolableObject
{
    private Text _nameText;
    private TextMeshProUGUI _valueText;

    private Color _color_green = new Color(0, 1, 0, 1);
    private Color _color_red = new Color(1, 0, 0, 1);

    public void Awake()
    {
        _nameText = transform.Find("Name").SafeGetComponent<Text>();
        _valueText = transform.Find("Value").SafeGetComponent<TextMeshProUGUI>();
    }

    public void SetUp(HardLevelModifyType type, float value)
    {
        _nameText.text = GameHelper.GetHardLevelModifyTypeName(type);

        if(type == HardLevelModifyType.EnemyDamage || type == HardLevelModifyType.EnemyHP)
        {
            _valueText.text = value > 0 ? string.Format("+{0}%", (int)value) : string.Format("-{0}%", (int)value);
        }

        SetUpColor(type);
    }

    private void SetUpColor(HardLevelModifyType type)
    {
        if(type == HardLevelModifyType.EnemyDamage || type == HardLevelModifyType.EnemyHP)
        {
            _valueText.color = _color_red;
        }
    }
}
