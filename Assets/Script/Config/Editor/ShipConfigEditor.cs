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
        // �������л�����
        serializedObject.Update();

        // ���������������ֶ�

        EditorGUILayout.PropertyField(ShipTypeProp);
        EditorGUILayout.PropertyField(MainWeaponProp);



        // Ӧ���޸�
        serializedObject.ApplyModifiedProperties();
    }
}