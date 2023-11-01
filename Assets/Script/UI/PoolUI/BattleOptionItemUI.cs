using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleOptionItemUI : GUIBasePanel, IPoolable
{

    private Image _icon;
    private TextMeshProUGUI _text;

    private BattleOptionItem _item;
    private Transform _ship;

    protected override void Awake()
    {
        _icon = transform.Find("Content/Icon").SafeGetComponent<Image>();
        _text = transform.Find("Content/Text").SafeGetComponent<TextMeshProUGUI>();

    }


    public override void Initialization()
    {
        base.Initialization();
     
    }


    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
        _item = (BattleOptionItem)(param[0]);
        if (_item == null)
            return;

        _ship = RogueManager.Instance.currentShip.transform;
        _text.text = _item.OptionName;
        _icon.sprite = _item.OptionSprite;
        UpdatePos();
    }


    public void Update()
    {
        ///FollowPos
        UpdatePos();
    }


    public void PoolableDestroy()
    {
        PoolableReset();
        UIManager.Instance.BackPoolerUI(this.gameObject.name, this.gameObject);
    }

    public void PoolableReset()
    {
        _item = null;
    }

    public void PoolableSetActive(bool isactive = true)
    {

    }

    private void UpdatePos()
    {
        if (_ship == null)
            return;

        var uiPos = UIManager.Instance.GetUIposBWorldPosition(_ship.position);
        transform.localPosition = uiPos;
    }
}
