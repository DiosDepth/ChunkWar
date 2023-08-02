using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEditor;
using UnityEngine;


//[CustomEditor(typeof(AIController))]
public class AIControllerEditor : Editor
{
    //ReorderableList _stateList;
    //SerializedProperty _currentState;

    //public void OnEnable()
    //{
    //    _stateList = new ReorderableList(serializedObject.FindProperty("States"), true, true, true);
    //    _stateList.elementNameProperty = "States";
    //    _stateList.elementDisplayType = ReorderableList.ElementDisplayType.Expandable;
    //    _currentState = serializedObject.FindProperty("currentState").FindPropertyRelative("StateName");
       
    //}

    //public override void OnInspectorGUI()
    //{
    //    //base.OnInspectorGUI();
    //    serializedObject.Update();
    //    EditorGUILayout.PropertyField(_currentState);
    //    _stateList.DoLayoutList();
    //    serializedObject.ApplyModifiedProperties();
    //}

}
