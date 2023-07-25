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

        // ���������������ֶ�
        EditorGUILayout.PropertyField(TypeProp);
        EditorGUILayout.PropertyField(RedirectionProp);


        // Ӧ���޸�
        serializedObject.ApplyModifiedProperties();

    }
}