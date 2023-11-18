using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if GMDEBUG
public class LTP_EnemyItem : MonoBehaviour
{
    private Transform _infoRoot;
    private TextMeshProUGUI _CountText;
    private TextMeshProUGUI _threadText;
    private Image _bg;

    private void Awake()
    {
        _infoRoot = transform.Find("Info");
        _CountText = transform.Find("Info/Count").SafeGetComponent<TextMeshProUGUI>();
        _threadText = transform.Find("Info/Thread/Value").SafeGetComponent<TextMeshProUGUI>();
        _bg = transform.Find("BG").SafeGetComponent<Image>();
    }

    public void SetUp(int count, float thread, Color color)
    {
        _infoRoot.SafeSetActive(true);
        _CountText.text = count.ToString();
        _threadText.text = thread.ToString();
        _bg.color = color;
    }

    public void SetEmpty()
    {
        _infoRoot.SafeSetActive(false);
        _bg.color = new Color(0, 0, 0, 0.15f);
    }
}
#endif