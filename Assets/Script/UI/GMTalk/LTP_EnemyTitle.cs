using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

#if GMDEBUG
public class LTP_EnemyTitle : MonoBehaviour
{
    public void SetUp(string name, Color color)
    {
        var text = transform.Find("Name").SafeGetComponent<TextMeshProUGUI>();
        text.text = name;
        transform.Find("BG").SafeGetComponent<Image>().color = color;
    }
}
#endif