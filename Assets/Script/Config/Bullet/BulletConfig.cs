using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class GeneralEffectConfig
{
    [HorizontalGroup("ZZ",300)]
    [LabelText("��Ч��")]
    [LabelWidth(80)]
    public string EffectName;

    [HorizontalGroup("ZZ",150)]
    [LabelText("����ߴ�")]
    [LabelWidth(80)]
    public bool RandomScale = false;

    [HorizontalGroup("ZZ")]
    [LabelText("��С�ߴ�")]
    [LabelWidth(80)]
    [ShowIf("RandomScale")]
    public float ScaleMin;

    [HorizontalGroup("ZZ")]
    [LabelText("���ߴ�")]
    [LabelWidth(80)]
    [ShowIf("RandomScale")]
    public float ScaleMax;

    [HorizontalGroup("ZZ", 150)]
    [LabelText("���λ��")]
    [LabelWidth(80)]
    public bool RandomPosition = false;

    [HorizontalGroup("ZZ")]
    [LabelText("OffsetX")]
    [LabelWidth(80)]
    [ShowIf("RandomPosition")]
    public float PosRandomX;

    [HorizontalGroup("ZZ")]
    [LabelText("OffsetY")]
    [LabelWidth(80)]
    [ShowIf("RandomPosition")]
    public float PosRandomY;
}

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

    [HorizontalGroup("AA", 250)]
    [LabelText("Owner")]
    [LabelWidth(60)]
    public OwnerType Owner;

    [HorizontalGroup("AB", 400)]
    [LabelText("Prefab")]
    [AssetSelector(Paths = "Assets/Resources/Prefab/GameplayPrefab/Bullets")]
    [LabelWidth(60)]
    [OnValueChanged("PrefabChange")]
    public GameObject Prefab;

    [ReadOnly]
    public string PrefabPath;

    [FoldoutGroup("Effect")]
    [LabelText("������Ч")]
    [LabelWidth(60)]
    public string HitAudio;

    [FoldoutGroup("Effect")]
    [LabelText("������Ч")]
    [LabelWidth(60)]
    public GeneralEffectConfig HitEffect;


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