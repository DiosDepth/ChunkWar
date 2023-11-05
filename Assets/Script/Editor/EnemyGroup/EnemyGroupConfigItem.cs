using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroupConfigItem : SerializedScriptableObject
{
    public int ID;

    public EnemyGroupGraphView _view;

    [OnInspectorInit]
    private void OnInit()
    {
        _view = new EnemyGroupGraphView
        {
            name = "View"
        };

    }
}
