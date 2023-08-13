using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class BuildSelectHoverCmpt : MonoBehaviour
{
    private Image _sprite;
    private RectTransform _spriteRect;
    private TextMeshProUGUI _nameText;

    private List<UnitUpgradeNodeCmpt> upgradeNodeCmpts;

    private static float SizePerUnit = 45;

    public void Awake()
    {
        _sprite = transform.Find("Image").SafeGetComponent<Image>();
        _spriteRect = transform.Find("Image").SafeGetComponent<RectTransform>();
        _nameText = transform.Find("InfoContent/Name").SafeGetComponent<TextMeshProUGUI>();
        upgradeNodeCmpts = transform.Find("InfoContent/Upgrade").GetComponentsInChildren<UnitUpgradeNodeCmpt>().ToList();
    }

    public void SetUp(Vector2 pos, Vector2 size, Unit info)
    {
        transform.SafeGetComponent<RectTransform>().anchoredPosition = pos;
        _spriteRect.SetRectHeight(size.y * SizePerUnit);
        _spriteRect.SetRectWidth(size.x * SizePerUnit);

        int minSize = (int)Mathf.Min(size.x, size.y);
        int sizeDelta = 3 - minSize;
        _sprite.pixelsPerUnitMultiplier = Mathf.Clamp(sizeDelta, 1, sizeDelta) * 1.1f;
        SetUpUnitInfo(info);
    }

    public void SetActive(bool active)
    {
        transform.SafeSetActive(active);
    }

    private void SetUpUnitInfo(Unit info)
    {
        var generalCfg = info._baseUnitConfig.GeneralConfig;
        _nameText.text = LocalizationManager.Instance.GetTextValue(generalCfg.Name);
        _nameText.color = GameHelper.GetRarityColor(generalCfg.Rarity);

        var currentRarityCost = GameHelper.GetUnitUpgradeCost(generalCfg.Rarity);
        var currentUpgradeCost = info.currentEvolvePoints;

        upgradeNodeCmpts.ForEach(x => x.SetVisiable(false));
        if(generalCfg.Rarity != GoodsItemRarity.Tier4)
        {
            for (int i = 0; i < currentRarityCost; i++)
            {
                upgradeNodeCmpts[i].SetUp(currentUpgradeCost > i);
            }
        }
    }
}
