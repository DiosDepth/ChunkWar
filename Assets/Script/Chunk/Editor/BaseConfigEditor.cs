using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(BaseConfig))]
public class BaseCongifEditor : Editor
{


    public virtual void OnEnable()
    {
   
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
