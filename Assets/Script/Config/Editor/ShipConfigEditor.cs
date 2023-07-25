using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(ShipConfig))]
public class ShipConfigEditor : BaseConfigEditor
{

    private SerializedProperty ShipTypeProp;
    private SerializedProperty MainWeaponProp;


    public override void OnEnable()
    {
        base.OnEnable();
        ShipTypeProp = serializedObject.FindProperty("ShipType");
        MainWeaponProp = serializedObject.FindProperty("MainWeapon");
    }

    public override void OnInspectorGUI()
    {

        base.OnInspectorGUI();
        // 更新序列化对象
        serializedObject.Update();

        // 绘制行数和列数字段

        EditorGUILayout.PropertyField(ShipTypeProp);
        EditorGUILayout.PropertyField(MainWeaponProp);



        // 应用修改
        serializedObject.ApplyModifiedProperties();
    }
}