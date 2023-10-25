using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

public class EnemyWarningLabel : GUIBasePanel, IPoolable
{
    private TextMeshProUGUI _text;

    protected override void Awake()
    {
        _text = transform.Find("Content/Text").SafeGetComponent<TextMeshProUGUI>();
    }

    public void SetUp(string content)
    {
        _text.text = content;
        DelayDestroy();
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(transform.name, gameObject);
    }

    public void PoolableReset()
    {

    }

    public void PoolableSetActive(bool isactive = true)
    {

    }

    private async void DelayDestroy()
    {
        await UniTask.Delay(3000);
        PoolableDestroy();
    }
}
