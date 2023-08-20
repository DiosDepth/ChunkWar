using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPropertyGroupPanel : MonoBehaviour
{
    public enum GroupType
    {
        Main,
        Sub
    }

    /// <summary>
    /// PropertyCmpt
    /// </summary>
    private ShipPropertyItemCmpt[] propertyCmpts;
    private ShipPropertyItemCmpt[] propertySubCmpts;

    private CanvasGroup _mainPropertyCanvas;
    private CanvasGroup _subPropertyCanvas;

    public GroupType CurrentGroupType = GroupType.Sub;


    public void Awake()
    {
        _mainPropertyCanvas = transform.Find("PropertyPanel").SafeGetComponent<CanvasGroup>();
        _subPropertyCanvas = transform.Find("PropertySubPanel").SafeGetComponent<CanvasGroup>();

        propertyCmpts = transform.Find("PropertyPanel").GetComponentsInChildren<ShipPropertyItemCmpt>();
        propertySubCmpts = transform.Find("PropertySubPanel").GetComponentsInChildren<ShipPropertyItemCmpt>();
    }

    public void SwitchGroupType()
    {
        if(CurrentGroupType == GroupType.Main)
        {
            CurrentGroupType = GroupType.Sub;
            if (propertySubCmpts != null && propertySubCmpts.Length > 0)
            {
                for (int i = 0; i < propertySubCmpts.Length; i++)
                {
                    propertySubCmpts[i].SetUp();
                }
            }
           
        }
        else
        {
            CurrentGroupType = GroupType.Main;
            if (propertyCmpts != null && propertyCmpts.Length > 0)
            {
                for (int i = 0; i < propertyCmpts.Length; i++)
                {
                    propertyCmpts[i].SetUp();
                }
            }
        }

        _mainPropertyCanvas.ActiveCanvasGroup(CurrentGroupType == GroupType.Main);
        _subPropertyCanvas.ActiveCanvasGroup(CurrentGroupType == GroupType.Sub);
        
    }
}
