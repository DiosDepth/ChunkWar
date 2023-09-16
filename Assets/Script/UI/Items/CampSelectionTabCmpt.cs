using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CampSelectionTabCmpt : MonoBehaviour
{

    public int CampID;

    public void Awake()
    {
        transform.SafeGetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    public void SetUp(int campID)
    {
        this.CampID = campID;
        var campInfo = GameManager.Instance.GetCampDataByID(campID);
        if(campInfo != null)
        {
            transform.Find("Name").SafeGetComponent<TextMeshProUGUI>().text = campInfo.CampName;
        }
    }

    public void OnSelected(bool select)
    {

    }

    private void OnButtonClick()
    {
        GeneralUIEvent.Trigger(UIEventType.ShipSelection_CampSelect, CampID);
        OnSelected(true);
    }
}
