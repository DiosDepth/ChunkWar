using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum DamageTextType
{
    Normal,
    Critical
}
public class DamageTextItemCmpt : PoolableObject
{

    private float randomOffset = 1f;

    public void SetUp(int value, Vector2 postion, DamageTextType type)
    {
        var txt = transform.SafeGetComponent<TextMeshProUGUI>();
        txt.text = value.ToString();
        txt.color = GameHelper.GetDamageTextColor(type);
        RandomPosition(postion);
    }

    private void RandomPosition(Vector2 pos)
    {
        var offset = UnityEngine.Random.Range(-randomOffset, randomOffset);
        transform.position = new Vector3(pos.x + offset, pos.y + offset);
    }
}
