using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if GMDEBUG
public class LTP_TimeLineNode : MonoBehaviour
{
    public int Index;

    public void Awake()
    {
        
    }

    public void SetUp(int index)
    {
        this.Index = index;
        transform.Find("Value").SafeGetComponent<TextMeshProUGUI>().text = index.ToString();
    }
}

#endif