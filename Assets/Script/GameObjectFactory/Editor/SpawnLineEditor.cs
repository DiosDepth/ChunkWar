using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpawnLine))]
public class SpawnLineEditor : Editor
{
    private SpawnLine _target;



    private SerializedProperty _lineEnd;
    private SerializedProperty _flipDirection;
    private SerializedProperty _moveDirection;
    private SerializedProperty _threshold;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    protected virtual void OnEnable()
    {
        _target = (SpawnLine)target;

        _lineEnd = serializedObject.FindProperty("lineEnd");
        _flipDirection = serializedObject.FindProperty("flipDirection");
        _moveDirection = serializedObject.FindProperty("moveDirction");
        _threshold = serializedObject.FindProperty("threshold");
    }
    private void OnSceneGUI()
    {

       
        _target.transform.position = Handles.PositionHandle(_target.transform.position, Quaternion.identity);
        Handles.CircleHandleCap(0, _target.transform.position, Quaternion.identity, 0.2f, EventType.Repaint);

        _target.lineEnd = _target.transform.InverseTransformPoint(Handles.PositionHandle(_target.transform.TransformPoint(_target.lineEnd), Quaternion.identity));
        Handles.CircleHandleCap(0, _target.transform.TransformPoint(_target.lineEnd), Quaternion.identity, 0.2f, EventType.Repaint);

        Handles.DrawLine(_target.transform.position, _target.transform.TransformPoint(_target.lineEnd));
        Vector3 _randomThresholdPoint = Vector3.Lerp(_target.transform.position, _target.transform.TransformPoint(_target.lineEnd), _threshold.floatValue);

        if (_flipDirection.boolValue)
        {
            _target.moveDirction = Quaternion.AngleAxis(-90, Vector3.forward) * _target.lineEnd.normalized;

        }
        else
        {
            _target.moveDirction = Quaternion.AngleAxis(90, Vector3.forward) * _target.lineEnd.normalized;
            
        }
      

        Handles.DrawLine(_randomThresholdPoint, _randomThresholdPoint + _target.moveDirction * 3);
        Handles.ConeHandleCap(0, _randomThresholdPoint + _target.moveDirction * 3, Quaternion.LookRotation(_target.moveDirction), 0.2f, EventType.Repaint);


    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        //EditorGUILayout.PropertyField(_moveDirection);
        serializedObject.ApplyModifiedProperties();
    }
}
