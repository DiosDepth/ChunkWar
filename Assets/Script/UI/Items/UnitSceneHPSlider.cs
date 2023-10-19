using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSceneHPSlider : GUIBasePanel, IPoolable
{
    private Image _fill_01;
    private Image _fill_02;

    private float LerpMaxTime = 1f;
    private float _lerpTimer = 0f;
    private bool _startLerp = false;
    private float _targetPercent;

    public Unit _targetUnit
    {
        get;
        private set;
    }

    protected override void Awake()
    {
        _fill_01 = transform.Find("Fill_1").SafeGetComponent<Image>();
        _fill_02 = transform.Find("Fill_2").SafeGetComponent<Image>();
    }

    private void Update()
    {
        if (!_startLerp)
            return;

        _lerpTimer += Time.deltaTime;
        if (_lerpTimer >= LerpMaxTime)
        {
            _startLerp = false;
            _lerpTimer = 0;
            var startvalue = _fill_02.fillAmount;
            LeanTween.value(this.gameObject, (value) =>
            {
                _fill_02.fillAmount = value;
            }, startvalue, _targetPercent, 0.25f);
        }
    }

    public void SetUpSlider(Unit unit)
    {
        _targetUnit = unit;
        var cmpt = unit.HpComponent;
        cmpt.OnHpChangeAction += OnUpdateSlider;
        OnUpdateSlider(cmpt.GetCurrentHP, cmpt.MaxHP, cmpt.HPPercent);
    }

    private void OnUpdateSlider(int currentHP, int MaxHP, float hpPercent)
    {
        _fill_01.fillAmount = hpPercent;
        _targetPercent = hpPercent;
        if (!_startLerp)
            _startLerp = true;
    }

    public void PoolableReset()
    {

    }

    public void PoolableDestroy()
    {
        PoolableReset();
        UIManager.Instance.BackPoolerUI("UnitHPSlider", gameObject);
    }

    public void PoolableSetActive(bool isactive = true)
    {

    }
}
