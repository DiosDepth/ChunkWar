using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BOSSHPSlider : MonoBehaviour
{
    private Image _fill_01;
    private Image _fill_02;

    private float LerpMaxTime = 1f;
    private float _lerpTimer = 0f;
    private bool _startLerp = false;
    private float _targetPercent;

    public AIShip _targetShip
    {
        get;
        private set;
    }

    public void Awake()
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

    public void SetUpSlider(AIShip ship)
    {
        _targetShip = ship;
        var name = LocalizationManager.Instance.GetTextValue(_targetShip.AIShipCfg.GeneralConfig.Name);
        transform.Find("Name").SafeGetComponent<TextMeshProUGUI>().text = name;
        _fill_01.fillAmount = 1;
        _fill_02.fillAmount = 1;

        LevelManager.Instance.OnEnemyCoreHPPercentChange += OnUpdateSlider;
        ship.OnCoreHPChange();
    }

    public void RemoveSlider()
    {
        LevelManager.Instance.OnEnemyCoreHPPercentChange -= OnUpdateSlider;
        GameObject.DestroyImmediate(this.gameObject);
    }

    private void OnUpdateSlider(float hpPercent, int currentHP, int MaxHP)
    {
        _fill_01.fillAmount = hpPercent;
        _targetPercent = hpPercent;
        if (!_startLerp)
            _startLerp = true;
    }
}
