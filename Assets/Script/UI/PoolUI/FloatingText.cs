using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingText : GUIBasePanel,IPoolable, IUIHoverPanel
{
    public uint TargetUID;

    /// <summary>
    /// 累计伤害，用于持续伤害累加
    /// </summary>
    private int damageTotal;

    private static Color _colorNormal = Color.white;
    private static Color _colorCritical = Color.yellow;
    private static Color32 _colorPlayerDamage = new Color32(255, 48, 43, 255);

    private TextMeshProUGUI _text;
    private RectTransform m_rect;
    private CanvasGroup _cavansGroup;

    private static float RandomPosOffset_X = 2;
    private static float RandomPosOffset_Y = 2;

    private float _timer;
    private const float duration = 0.75f;
    private const float criticalScale = 1.3f;

    public bool IsNeedToRemove
    {
        get;
        private set;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialization()
    {
        base.Initialization();
        _text = transform.Find("Textinfo").SafeGetComponent<TextMeshProUGUI>();
        m_rect = transform.SafeGetComponent<RectTransform>();
        _cavansGroup = transform.SafeGetComponent<CanvasGroup>();
        IsNeedToRemove = false;
    }

    public void OnUpdate()
    {
        if (IsNeedToRemove)
            return;

        _timer += Time.unscaledDeltaTime;
        if(_timer >= duration)
        {
            IsNeedToRemove = true;
        }
    }

    public override void Hidden()
    {
        base.Hidden();
        PoolableDestroy();
    }

    public void OnRemove()
    {
        LeanTween.alphaCanvas(_cavansGroup, 0, 0.3f).setOnComplete(() =>
        {
            PoolableDestroy();
        }).setIgnoreTimeScale(true);
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        UIManager.Instance.BackPoolerUI(this.gameObject.name, this.gameObject);
    }

    public void PoolableReset()
    {
        IsNeedToRemove = false;
        _text.transform.SetLocalScaleXY(1);
        _text.color = _colorNormal;
        TargetUID = 0;
        damageTotal = 0;
        _cavansGroup.alpha = 1;
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }

    /// <summary>
    /// 延长持续伤害
    /// </summary>
    public void ProlongTextDuration(int value, bool isCritical, Vector2 updatePos)
    {
        _timer = 0;
        var oldValue = damageTotal;
        damageTotal += value;
        RandomPosition(updatePos);
        LeanTween.value(oldValue, damageTotal, 0.2f).setOnUpdate((damage) =>
        {
            SetText((int)damage);

        }).setIgnoreTimeScale(true);
    }

    /// <summary>
    /// 显示数字
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isCritical"></param>
    /// <param name="rowPos"></param>
    public void SetText(int value, bool isCritical, Vector2 rowPos, bool simple = true)
    {
        _cavansGroup.alpha = 1;
        damageTotal = value;
        _text.text = value.ToString();
        _text.color = isCritical ? _colorCritical : _colorNormal;
        _text.transform.SetLocalScaleXY(isCritical ? criticalScale : 1);
        RandomPosition(rowPos);

        float posY = m_rect.anchoredPosition.y;
        float targetPos = posY + 8f;

        if (simple)
        {
            LeanTween.value(posY, targetPos, 0.3f).setOnUpdate((value) =>
            {
                m_rect.SetRectY(value);
            }).setIgnoreTimeScale(true);
            LeanTween.alphaCanvas(_cavansGroup, 0, 0.2f).setDelay(duration).setOnComplete(() =>
            {
                PoolableDestroy();
            }).setIgnoreTimeScale(true);
        }
        else
        {
            LeanTween.value(posY, targetPos, 0.3f).setOnUpdate((value) =>
            {
                m_rect.SetRectY(value);
            }).setIgnoreTimeScale(true);
        }
    }

    public void SetText(int value)
    {
        _text.text = value.ToString();
    }

    public void SetPlayerTakeDamageText(float value, Vector2 rowPos)
    {
        _text.SetText(string.Format("-{0}", value));
        _text.color = _colorPlayerDamage;
        RandomPosition(rowPos);
        LeanTween.moveLocalY(gameObject, 8, 0.3f).setIgnoreTimeScale(true);
        LeanTween.value(1, 0, 0.2f).setOnUpdate((value) =>
        {
            _cavansGroup.alpha = value;
        }).setDelay(duration).setOnComplete(() =>
        {
            PoolableDestroy();
        }).setIgnoreTimeScale(true);
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
    /// 随机位置
    /// </summary>
    /// <param name="rowPos"></param>
    private void RandomPosition(Vector2 rowPos)
    {
        var offsetX = UnityEngine.Random.Range(-RandomPosOffset_X, RandomPosOffset_X);
        var offsetY = UnityEngine.Random.Range(-RandomPosOffset_Y, RandomPosOffset_Y);
        rowPos = new Vector2(rowPos.x + offsetX, rowPos.y + offsetY);

        m_rect.anchoredPosition = rowPos;
    }
}
