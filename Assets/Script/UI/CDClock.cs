using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CDClock : GUIBasePanel
{
    public Image CDMask;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Initialization()
    {
        base.Initialization();
        CDMask = GetGUIComponent<Image>("CDMask");
    }
}
