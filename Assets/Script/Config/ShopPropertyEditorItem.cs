using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class ShopPropertyEditorItem : SerializedScriptableObject
{
    public Dictionary<PropertyModifyKey, float> PropertyValueDic = new Dictionary<PropertyModifyKey, float>();

    public float GetPropertyValue(PropertyModifyKey key)
    {
        if (PropertyValueDic.ContainsKey(key))
            return PropertyValueDic[key];

        return -999;
    }
#if UNITY_EDITOR
    [OnInspectorDispose]
    private void OnDispose()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
}
