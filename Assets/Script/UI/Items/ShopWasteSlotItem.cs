using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopWasteSlotItem : MonoBehaviour, IPoolable
{
    private Image _rarityBG;
    private TextMeshProUGUI _wasteValueText;
    private TextMeshProUGUI _wasteLoadValueText;
    private TextMeshProUGUI _wasteSellAllValueText;

    public void Awake()
    {
        _rarityBG = transform.Find("BG").SafeGetComponent<Image>();
        _wasteValueText = transform.Find("WasteContent/Count/Value").SafeGetComponent<TextMeshProUGUI>();
        _wasteLoadValueText = transform.transform.Find("WasteContent/Load/Value").SafeGetComponent<TextMeshProUGUI>();
        _wasteSellAllValueText = transform.transform.Find("WasteContent/SellAll/Value").SafeGetComponent<TextMeshProUGUI>();

        transform.Find("WasteContent/WasteSell").SafeGetComponent<Button>().onClick.AddListener(OnWasteSellClick);
        transform.Find("WasteContent/SellAll").SafeGetComponent<Button>().onClick.AddListener(OnWasteSellAllClick);
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

    public void SetUpWaste()
    {
        _rarityBG.sprite = GameHelper.GetRarityBG_Big(GoodsItemRarity.Tier1);
        _wasteValueText.text = RogueManager.Instance.GetDropWasteCount.ToString();
        _wasteLoadValueText.text = string.Format("{0:F1}", RogueManager.Instance.GetDropWasteLoad);
        RefreshSellAllValue();
    }


    private void OnWasteSellClick()
    {
        ///ShowSell Dialog
        UIManager.Instance.ShowUI<WasteSellDialog>("WasteSellDialog", E_UI_Layer.Top, this, (panel) =>
        {
            panel.Initialization();
        });
    }

    private void OnWasteSellAllClick()
    {
        RogueManager.Instance.SellAllWaste();
    }

    private void RefreshSellAllValue()
    {
        var totalCount = RogueManager.Instance.GetDropWasteCount;
        _wasteSellAllValueText.text = GameHelper.CalculateWasteSellPrice(totalCount).ToString();
    }
}
