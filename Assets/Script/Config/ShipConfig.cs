using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Configs_Ship_", menuName = "Configs/Unit/ShipConfig")]
public class ShipConfig : BaseConfig, ISerializationCallbackReceiver
{
    [LabelText("Ω¢¥¨¿‡–Õ")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 300)]
    public ShipType ShipType;


    public AvalibalMainWeapon MainWeapon;


    public override void OnEnable()
    {
        base.OnEnable();

    }

}
