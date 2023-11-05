using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyGroupGraphView : GraphView
{
    public EnemyGroupGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
       
    }
}

public class EnemyShipNode: Node
{
    public int ID;
}