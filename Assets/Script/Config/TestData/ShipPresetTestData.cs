using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if GMDEBUG
[CreateAssetMenu(fileName = "TestData_Ship_", menuName = "Configs/TestData_Ship")]
public class ShipPresetTestData : SerializedScriptableObject
{
    public int ID;
    public string Name;
    public string Comment;

    public int ShipID;
    public int ShipMainWeaponID;
}
#endif