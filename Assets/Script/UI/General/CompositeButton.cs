using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class CompositeButton : MonoBehaviour
{
    public void SetUp(string titleText, Sprite icon, string value, UnityAction clickAction)
    {
        transform.Find("Text").SafeGetComponent<TextMeshProUGUI>().text = titleText;
        transform.Find("Icon").SafeGetComponent<Image>().sprite = icon;
        transform.Find("Value").SafeGetComponent<TextMeshProUGUI>().text = value;

        var btn = transform.SafeGetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(clickAction);

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.SafeGetComponent<RectTransform>());
    }
}
