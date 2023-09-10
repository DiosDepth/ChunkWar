using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class GetTextByID_TMP : MonoBehaviour
{
    public string Textid;
    void Awake()
    {
        InitText();
    }

    void InitText()
    {
        if (!string.IsNullOrEmpty(Textid))
        {
            GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetTextValue(Textid);
        }
        else
        {
            GetComponent<TextMeshProUGUI>().text = "Error!";
        }
    }
   
}
