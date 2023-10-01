using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectOption : MonoBehaviour
{
    private CanvasGroup _canvas;
    private CompositeButton _sellBtn;

    private const string UnitOption_SellButton_Text = "UnitOption_SellButton_Text";

    private string titleStringText;
    private Sprite icon;

    private Unit _currentUnit;

    private void Awake()
    {
        _canvas = transform.SafeGetComponent<CanvasGroup>();
        _sellBtn = transform.Find("Content/Sell").SafeGetComponent<CompositeButton>();
        icon = DataManager.Instance.gameMiscCfg.IconCfg.CurrencyIcon;
        titleStringText = LocalizationManager.Instance.GetTextValue(UnitOption_SellButton_Text);
    }

    public void Active(Unit unit, bool active)
    {
        if (active)
        {
            _currentUnit = unit;
            var pos = UIManager.Instance.GetUIPosByMousePos();
            transform.localPosition = pos;
        }

        if (unit != null)
        {
            var sellPrice = GameHelper.GetUnitSellPrice(unit);
            _sellBtn.SetUp(titleStringText, icon, sellPrice.ToString(), OnSellBtnClick);
        }

        _canvas.ActiveCanvasGroup(active);
    }

    private void OnSellBtnClick()
    {
        if (_currentUnit == null)
            return;

        ShipBuilder.instance.SellCurrentHoverUnit();
    }

}
