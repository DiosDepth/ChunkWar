using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PropertyHoverItem : GUIBasePanel, IPoolable, IUIHoverPanel
{
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _descText;
    private CanvasGroup _mainCanvas;

    private bool _enable = false;

    protected override void Awake()
    {
        _mainCanvas = transform.SafeGetComponent<CanvasGroup>();
        _nameText = transform.Find("Content/Title").SafeGetComponent<TextMeshProUGUI>();
        _descText = transform.Find("Content/Content").SafeGetComponent<TextMeshProUGUI>();
    }

    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
        PropertyDisplayConfig cfg = (PropertyDisplayConfig)param[0];
        PropertyModifyKey key = (PropertyModifyKey)param[1];
        if (cfg == null)
            return;

        _nameText.text = LocalizationManager.Instance.GetTextValue(cfg.NameText);
        _descText.text = GameHelper.GetPropertyHoverDesc(key, cfg);

        _enable = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.Find("Content").SafeGetComponent<RectTransform>());
        UpdatePosition();
        _mainCanvas.ActiveCanvasGroup(true);
    }

    public void Update()
    {
        if (_enable)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        transform.localPosition = UIManager.Instance.GetUIPosByMousePos();
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        UIManager.Instance.BackPoolerUI(transform.name, gameObject);
    }

    public void PoolableReset()
    {
        _enable = false;
        _mainCanvas.ActiveCanvasGroup(false);
    }

    public void PoolableSetActive(bool isactive = true)
    {

    }
}
