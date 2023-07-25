using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;




[CustomEditor(typeof(BaseUnitConfig))]
public class BaseUnitConfigEditor : BaseConfigEditor
{


 
    private SerializedProperty RedirectionProp;
    private SerializedProperty TypeProp;

    public override void OnEnable()
    {
        base.OnEnable();

        RedirectionProp = serializedObject.FindProperty("redirection");
        TypeProp = serializedObject.FindProperty("unitType");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        // 绘制行数和列数字段
        EditorGUILayout.PropertyField(TypeProp);
        EditorGUILayout.PropertyField(RedirectionProp);


        // 应用修改
        serializedObject.ApplyModifiedProperties();

    }
}