using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BulletConfig : SerializedScriptableObject
{
    [HorizontalGroup("AA", 200)]
    [LabelText("ID")]
    [LabelWidth(60)]
    public int ID;

    [HorizontalGroup("AA", 300)]
    [LabelText("����")]
    [LabelWidth(60)]
    public string BulletName;

    [HorizontalGroup("AA", 250)]
    [LabelText("����")]
    [LabelWidth(60)]
    public BulletType Type;

    [HorizontalGroup("AB", 400)]
    [LabelText("Prefab")]
    [AssetSelector(Paths = "Assets/Resources/Prefab/GameplayPrefab/Bullets")]
    [LabelWidth(60)]
    [OnValueChanged("PrefabChange")]
    public GameObject Prefab;

    [ReadOnly]
    public string PrefabPath;

    [HorizontalGroup("AB", 300)]
    [LabelText("������Ч")]
    [LabelWidth(60)]
    public string HitAudio;

#if UNITY_EDITOR
        
    [OnInspectorDispose]
    private void OnDispose()
    {
        PrefabChange();
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void PrefabChange()
    {
        if (Prefab != null)
        {
            var path = AssetDatabase.GetAssetPath(Prefab);
            path = path.Replace("Assets/Resources/", "");
            path = path.Replace(".prefab", "");
            PrefabPath = path;
        }
    }
#endif


}