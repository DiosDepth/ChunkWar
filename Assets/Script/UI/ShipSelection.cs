using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipSelection : GUIBasePanel
{



    public List<ItemGUISlot> shipSlotList = new List<ItemGUISlot>();


    // Start is called before the first frame update
    void Start()
    {
        
    }


    public override void Initialization()
    {
        base.Initialization();
        for (int i = 0; i < shipSlotList.Count; i++)
        {

        }

    }

    // Update is called once per frame
    void Update()
    {

    }

}
