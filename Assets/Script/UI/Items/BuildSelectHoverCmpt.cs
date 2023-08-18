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
    private Transform _levelMaxDesc;

    private List<UnitUpgradeNodeCmpt> upgradeNodeCmpts;

    private static float SizePerUnit = 45;

    private BaseConfig _currentPreviewUpgradeItem;
    private Unit currentUnit;

    public void Awake()
    {
        _sprite = transform.Find("Image").SafeGetComponent<Image>();
        _spriteRect = transform.Find("Image").SafeGetComponent<RectTransform>();
        _nameText = transform.Find("InfoContent/Name").SafeGetComponent<TextMeshProUGUI>();
        _levelMaxDesc = transform.Find("InfoContent/MaxLevelDesc");
        upgradeNodeCmpts = transform.Find("InfoContent/Upgrade").GetComponentsInChildren<UnitUpgradeNodeCmpt>().ToList();
    }

    public void SetUp(Vector2 pos, Vector2 size, Unit info)
    {
        var height = size.y * SizePerUnit;
        var width = size.x * SizePerUnit;
        transform.SafeGetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x - width / 2, pos.y + height / 2);
        _spriteRect.SetRectHeight(height);
        _spriteRect.SetRectWidth(width);

        int minSize = (int)Mathf.Min(size.x, size.y);
        int sizeDelta = 3 - minSize;
        _sprite.pixelsPerUnitMultiplier = Mathf.Clamp(sizeDelta, 1, sizeDelta) * 1.1f;
        SetUpUnitInfo(info);
    }

    public void SetUpUpgradePreivew(BaseConfig basedCfg)
    {
        if (_currentPreviewUpgradeItem == basedCfg)
            return;

        if (!ResetLevelNode())
        {
            _currentPreviewUpgradeItem = basedCfg;
            var addNode = GameHelper.GetEvoluveRarityPoints(basedCfg.GeneralConfig.Rarity);
            for (int i = upgradeNodeCmpts.Count - 1; i >= 0; i--) 
            {
                var nodeCmpt = upgradeNodeCmpts[i];
                if (addNode > 0 && !nodeCmpt.isActiveAndEnabled) 
                {
                    nodeCmpt.SetUpgradeDisplay();
                }
            }
        }
    }

    public void SetActive(bool active)
    {
        transform.SafeSetActive(active);
    }

    private void SetUpUnitInfo(Unit info)
    {
        currentUnit = info;
        var generalCfg = info._baseUnitConfig.GeneralConfig;
        _nameText.text = LocalizationManager.Instance.GetTextValue(generalCfg.Name);
        _nameText.color = GameHelper.GetRarityColor(generalCfg.Rarity);
        ResetLevelNode();
    }

    private void OnDisable()
    {
        _currentPreviewUpgradeItem = null;
    }

    private bool ResetLevelNode()
    {
        var generalCfg = currentUnit._baseUnitConfig.GeneralConfig;
        upgradeNodeCmpts.ForEach(x => x.SetVisiable(false));

        var currentRarityCost = GameHelper.GetUnitUpgradeCost(generalCfg.Rarity);
        var currentUpgradeCost = currentUnit.currentEvolvePoints;
        var isMaxLevel = generalCfg.Rarity == GoodsItemRarity.Tier4;
        _levelMaxDesc.SafeSetActive(isMaxLevel);
        if (!isMaxLevel)
        {
            for (int i = 0; i < currentRarityCost; i++)
            {
                upgradeNodeCmpts[i].SetUp(currentUpgradeCost > i);
            }
        }
        return isMaxLevel;
    }
}
