using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ShopMainConfig : SerializedScriptableObject
{
#if UNITY_EDITOR
    private const string ShopPropertyItemConfigPath = "Assets/EditorRes/Misc/ShopPropertyEditorItem.asset";

    private static ShopPropertyEditorItem _propertyItem;
    public static ShopPropertyEditorItem PropertyItem
    {
        get
        {
            if (_propertyItem == null)
                _propertyItem = AssetDatabase.LoadAssetAtPath<ShopPropertyEditorItem>(ShopPropertyItemConfigPath);

            return _propertyItem;
        }
    }
#endif 

    [LabelText("每次刷新增加 波次乘参数")]
    public float RollCostIncreaceWaveParam = 0.5f;

    [LabelText("刷新基础，根据总进入次数")]
    public int[] RollCostWaveBase = new int[20];

    [TableList(ShowIndexLabels = true, NumberOfItemsPerPage = 25)]
    [HideReferenceObjectPicker]
    public List<ShopGoodsItemConfig> Goods = new List<ShopGoodsItemConfig>();

    [TableList(ShowIndexLabels = true)]
    [HideReferenceObjectPicker]
    public List<WreckageDropItemConfig> WreckageDrops = new List<WreckageDropItemConfig>();

    [DictionaryDrawerSettings(DisplayMode =  DictionaryDisplayOptions.Foldout)]
    public Dictionary<GoodsItemRarity, ShopGoodsRarityConfig> RarityMap = new Dictionary<GoodsItemRarity, ShopGoodsRarityConfig>();

    [LabelText("装备包商店刷新")]
    [HideReferenceObjectPicker]
    public List<WreckageShopBuyItemConfig> WreckageShopBuyItemCfg = new List<WreckageShopBuyItemConfig>();
}

public enum GoodsItemRarity
{
    Tier1,
    Tier2,
    Tier3,
    Tier4,
    Tier5,
}


[System.Serializable]
public class WreckageDropItemConfig
{
    [TableColumnWidth(150, false)]
    public int UnitID;

    [TableColumnWidth(150, false)]
    public byte Weight = 20;

    [TableColumnWidth(150, false)]
    public int SellPrice;

    [TableColumnWidth(150, false)]
    public int LoadCost;

}

[System.Serializable]
public class WreckageShopBuyItemConfig
{
    public byte WeightBase;
    public GoodsItemRarity Rarity;
    public int CostBase;
    public GeneralItemConfig ItemConfig;

    public int GetGoodsID()
    {
        switch (Rarity)
        {
            case GoodsItemRarity.Tier1:
                return 100000;
            case GoodsItemRarity.Tier2:
                return 100001;
            case GoodsItemRarity.Tier3:
                return 100002;
            case GoodsItemRarity.Tier4:
                return 100004;
        }
        return -1;
    }
}

[System.Serializable]
public class ShopGoodsItemConfig
{
    [TableColumnWidth(60, false)]
    public int GoodID;

    [TableColumnWidth(80, false)]
    public byte Weight = 20;

    [TableColumnWidth(80, false)]
    public int TypeID;

    [TableColumnWidth(80, false)]
    public int CostBase;

    [TableColumnWidth(100, false)]
    public int MaxBuyCount = -1;

#if UNITY_EDITOR

    [TableColumnWidth(150,false)]
    [ReadOnly]
    [ShowInInspector]
    private float CostValue;

    [OnInspectorInit]
    private void OnInit()
    {
        CostValue = 0;
        if (TypeID == 0)
            return;

        var items = DataManager.Instance.GetShipPlugItemConfig(TypeID);
        if (items != null)
        {
            var propertyCfg = items.PropertyModify;
            if (propertyCfg != null && propertyCfg.Length > 0)
            {
                for (int i = 0; i < propertyCfg.Length; i++)
                {
                    var value = ShopMainConfig.PropertyItem.GetPropertyValue(propertyCfg[i].ModifyKey);
                    if (!propertyCfg[i].BySpecialValue)
                    {
                        CostValue += propertyCfg[i].Value * value;
                    }
                }
            }
        }
    }

#endif

}

[System.Serializable]
[HideReferenceObjectPicker]
public class ShopGoodsRarityConfig
{
    [HorizontalGroup("A1")]
    [LabelWidth(80)]
    [LabelText("最小出现")]
    public byte MinAppearEneterCount;

    [HorizontalGroup("A1")]
    [LabelWidth(120)]
    [LabelText("幸运有效最小")]
    public byte LuckModifyMinEnterCount;

    [HorizontalGroup("A1")]
    [LabelWidth(80)]
    [LabelText("基础权重")]
    public byte BaseWeight;
    [HorizontalGroup("A1")]
    [LabelWidth(80)]
    [LabelText("权重每波增加")]
    public float WeightAddPerEnterCount;
    [HorizontalGroup("A1")]
    [LabelWidth(80)]
    [LabelText("最大权重")]
    public float WeightMax;
}

[System.Serializable]
public class GeneralItemConfig
{
    [VerticalGroup("AA")]
    [PreviewField(50, Alignment = ObjectFieldAlignment.Left)]
    [LabelWidth(80)]
    [LabelText("图标")]
    public Sprite IconSprite;

    [LabelText("名称")]
    [LabelWidth(80)]
    [OnValueChanged("OnNameChange")]
    [DelayedProperty]
    public string Name;

    [LabelText("CN")]
    [LabelWidth(80)]
    [ShowInInspector]
    [ReadOnly]
    private string NamePreview;

    [VerticalGroup("AA")]
    [Multiline(1)]
    [LabelText("描述")]
    [LabelWidth(80)]
    [OnValueChanged("OnDescChange")]
    [DelayedProperty]
    public string Desc;

    [VerticalGroup("AA")]
    [LabelText("CN")]
    [LabelWidth(80)]
    [ShowInInspector]
    [ReadOnly]
    private string DescPreview;

    [HorizontalGroup("AA/A", 300)]
    [LabelText("稀有度")]
    [LabelWidth(80)]
    public GoodsItemRarity Rarity;

    [HorizontalGroup("AA/A", 300)]
    [LabelText("排序")]
    [LabelWidth(80)]
    public int SortingOrder;

    [OnInspectorInit]
    private void Init()
    {
        if (!string.IsNullOrEmpty(Name))
        {
            NamePreview = LocalizationManager.Instance.GetTextValue(Name);
        }
        if (!string.IsNullOrEmpty(Desc))
        {
            DescPreview = LocalizationManager.Instance.GetTextValue(Desc);
        }
    }

#if UNITY_EDITOR

    private void OnNameChange()
    {
        NamePreview = LocalizationManager.Instance.GetTextValue(Name);
    }

    private void OnDescChange()
    {
        DescPreview = LocalizationManager.Instance.GetTextValue(Desc);
    }

#endif

}