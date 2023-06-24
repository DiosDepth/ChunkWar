using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(ShipConfig))]
public class ShipConfigEditor : BaseCongifEditor
{
    private ShipConfig _target;
    private SerializedProperty IDProp;
    private SerializedProperty NameProp;
    private SerializedProperty TypeProp;
    private SerializedProperty HPProp;
    private SerializedProperty IconProp;
    private SerializedProperty PrefabProp;
    private SerializedProperty MapProp;



    public override void OnEnable()
    {
        _target = (ShipConfig)target;
        // 获取序列化属性
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

        
        // 更新序列化对象
        serializedObject.Update();

        // 绘制行数和列数字段
        EditorGUILayout.PropertyField(IDProp);
        EditorGUILayout.PropertyField(NameProp);
        EditorGUILayout.PropertyField(TypeProp);
        EditorGUILayout.PropertyField(HPProp);
        EditorGUILayout.PropertyField(IconProp);


        EditorGUILayout.PropertyField(PrefabProp);

        // 获取数组大小
        int rows = _target.ShipMap.GetLength(0);
        int columns = _target.ShipMap.GetLength(1);

        // 绘制二维数组表格
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

        // 应用修改
        serializedObject.ApplyModifiedProperties();
    }
}