using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingText : GUIBasePanel,IPoolable
{
    public float duration = 0.75f;

    private static Color _colorNormal = Color.white;
    private static Color _colorCritical = Color.yellow;
    private static Color _colorPlayerDamage = Color.red;

    private static float _defaultTextSize = 16;
    private TextMeshProUGUI _text;

    private static float RandomPosOffset_X = 15;
    private static float RandomPosOffset_Y = 15;

    protected override void Awake()
    {
        base.Awake();
        
    }

    public override void Initialization()
    {
        base.Initialization();
        _text = transform.Find("uiGroup/Textinfo").SafeGetComponent<TextMeshProUGUI>();
    }

    public override void Show()
    {
        StartCoroutine(MonoManager.Instance.DelayUnSacleTime(duration, () => 
        {
            Hidden();
        }));
    }

    public override void Hidden()
    {
        base.Hidden();
        PoolableDestroy();
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableReset()
    {
        _text.fontSize = _defaultTextSize;
        _text.color = _colorNormal;
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }

    public void SetText(string text, bool isCritical, Vector2 rowPos)
    {
        _text.SetText(text);
        _text.color = isCritical ? _colorCritical : _colorNormal;
        RandomPosition(rowPos);
    }

    public void SetText(float value, bool isCritical, Vector2 rowPos)
    {
        _text.SetText(value.ToString());
        _text.color = isCritical ? _colorCritical : _colorNormal;
        RandomPosition(rowPos);
    }

    public void SetPlayerTakeDamageText(float value, Vector2 rowPos)
    {
        _text.SetText(string.Format("-{0}", value));
        _text.color = _colorPlayerDamage;
        RandomPosition(rowPos);
    }

    public void SetSize(float size)
    {
        _text.fontSize = size;
    }
    public void SetColor(UnityEngine.Color color)
    {
        _text.color = color;
    }

    /// <summary>
    /// Ëæ»úÎ»ÖÃ
    /// </summary>
    /// <param name="rowPos"></param>
    private void RandomPosition(Vector2 rowPos)
    {
        var offsetX = UnityEngine.Random.Range(-RandomPosOffset_X, RandomPosOffset_X);
        var offsetY = UnityEngine.Random.Range(-RandomPosOffset_Y, RandomPosOffset_Y);
        rowPos = new Vector2(rowPos.x + offsetX, rowPos.y + offsetY);
        transform.position = rowPos;
    }
}
