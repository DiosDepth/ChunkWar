using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if GMDEBUG
[CreateAssetMenu(fileName = "TestData_Plug_", menuName = "Configs/TestData_Plug")]
public class ShipPlugPresetTestData : SerializedScriptableObject
{
    public int ID;
    public string Name;
    public string Comment;
    public Dictionary<int, int> PlugDic = new Dictionary<int, int>();
}
#endif