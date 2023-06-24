using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;




[CustomEditor(typeof(BaseUnitConfig))]
public class BaseUnitConfigEditor : BaseCongifEditor
{
    private BaseUnitConfig _target;
    private SerializedProperty IDProp;
    private SerializedProperty NameProp;
    private SerializedProperty TypeProp;
    private SerializedProperty HPProp;
    private SerializedProperty IconProp;
    private SerializedProperty PrefabProp;
    private SerializedProperty MapProp;



    public override void OnEnable()
    {
        _target = (BaseUnitConfig)target;
        // ��ȡ���л�����
        IDProp = serializedObject.FindProperty("ID");
        NameProp = serializedObject.FindProperty("UnitName");
        TypeProp = serializedObject.FindProperty("Type");
        HPProp = serializedObject.FindProperty("HP");
        IconProp = serializedObject.FindProperty("Icon");
        PrefabProp = serializedObject.FindProperty("Prefab");
        MapProp = serializedObject.FindProperty("m_FlattendMapLayout");
    }

    public override void OnInspectorGUI()
    {

   
        // �������л�����
        serializedObject.Update();

        // ���������������ֶ�
        EditorGUILayout.PropertyField(IDProp);
        EditorGUILayout.PropertyField(NameProp);
        EditorGUILayout.PropertyField(TypeProp);
        EditorGUILayout.PropertyField(HPProp);
        EditorGUILayout.PropertyField(IconProp);


        EditorGUILayout.PropertyField(PrefabProp);

        // ��ȡ�����С
        int rows = _target.Map.GetLength(0);
        int columns  = _target.Map.GetLength(1);

        // ���ƶ�ά������
        EditorGUILayout.LabelField("MapProp");
        EditorGUI.indentLevel++;


        for (int i = 0; i < rows; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < columns; j++)
            {
                
                SerializedProperty elementProp = MapProp.GetArrayElementAtIndex(i * columns + j);
                EditorGUILayout.PropertyField(elementProp, GUIContent.none);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUI.indentLevel--;

        // Ӧ���޸�
        serializedObject.ApplyModifiedProperties();
    }
}