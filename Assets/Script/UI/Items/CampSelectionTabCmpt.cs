using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CampSelectionTabCmpt : MonoBehaviour, IPoolable
{

    public int CampID;

    public void Awake()
    {
        transform.SafeGetComponent<Button>().onClick.AddListener(OnButtonClick);
        OnSelected(false);
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
        transform.Find("Selected").SafeSetActive(select);
    }

    private void OnButtonClick()
    {
        GeneralUIEvent.Trigger(UIEventType.ShipSelection_CampSelect, CampID);
        OnSelected(true);
    }

    public void PoolableReset()
    {
        OnSelected(false);
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(transform.name, gameObject);
    }

    public void PoolableSetActive(bool isactive = true)
    {

    }
}
