using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class AIManager : Singleton<AIManager>
{

    // Start is called before the first frame update
    public AIManager()
    {
        Initialization();
        MonoManager.Instance.AddUpdateListener(Update);
        MonoManager.Instance.AddLaterUpdateListener(LaterUpdate);
    }
    ~AIManager()
    {

        MonoManager.Instance.RemoveUpdateListener(Update);
        MonoManager.Instance.RemoveUpdateListener(LaterUpdate);
    }
    private void Update()
    {
        
    }
    private void LaterUpdate()
    {
        
    }
}
