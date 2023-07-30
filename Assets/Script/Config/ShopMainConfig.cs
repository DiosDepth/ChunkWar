using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopMainConfig : SerializedScriptableObject
{
    [LabelText("ÿ��ˢ������ ���γ˲���")]
    public float RollCostIncreaceWaveParam = 0.5f;

    [LabelText("ˢ�»���")]
    public int[] RollCostWaveBase = new int[20];

    [TableList(ShowIndexLabels = true)]
    [HideReferenceObjectPicker]
    public List<ShopGoodsItemConfig> Goods = new List<ShopGoodsItemConfig>();

    [DictionaryDrawerSettings(DisplayMode =  DictionaryDisplayOptions.Foldout)]
    public Dictionary<GoodsItemRarity, ShopGoodsRarityConfig> RarityMap = new Dictionary<GoodsItemRarity, ShopGoodsRarityConfig>();

}

public enum GoodsItemRarity
{
    Tier1,
    Tier2,
    Tier3,
    Tier4
}

public enum GoodsItemType
{
    ShipUnit,
    ShipPlug,
}

[System.Serializable]
public class ShopGoodsItemConfig
{
    [TableColumnWidth(60, false)]
    [OnValueChanged("OnGoodsIDChange")]
    public int GoodID;

    [TableColumnWidth(80,false)]
    public GoodsItemRarity Rarity;

    [TableColumnWidth(70, false)]
    [PreviewField(50)]
    [AssetSelector(Paths = "Assets/Resources/Sprite/ShopIcon", DropdownHeight = 800, DropdownWidth = 400)]
    public Sprite IconSprite;

    [TableColumnWidth(80, false)]
    public byte Weight = 20;

    [TableColumnWidth(80, false)]
    public GoodsItemType ItemType;

    [TableColumnWidth(80, false)]
    public int TypeID;

    [TableColumnWidth(80, false)]
    public int CostBase;

    [TableColumnWidth(50, false)]
    public bool Unique;

    [TableColumnWidth(100, false)]
    [HideIf("Unique")]
    public int MaxBuyCount = -1;

    [TableColumnWidth(150, false)]
    [ReadOnly]
    public string Name;

    [TableColumnWidth(150, false)]
    [ReadOnly]
    public string Desc;


    private void OnGoodsIDChange()
    {
        Name = string.Format("ShopGoods_Name_{0}", GoodID);
        Desc = string.Format("ShopGoods_Desc_{0}", GoodID);
    }
}

[System.Serializable]
[HideReferenceObjectPicker]
public class ShopGoodsRarityConfig
{
    [HorizontalGroup("A1")]
    [LabelWidth(80)]
    [LabelText("��С���ֵ��")]
    public byte MinAppearEneterCount;


    [HorizontalGroup("A1")]
    [LabelWidth(120)]
    [LabelText("������Ч��С���")]
    public byte LuckModifyMinEnterCount;

    [HorizontalGroup("A1")]
    [LabelWidth(80)]
    [LabelText("����Ȩ��")]
    public byte BaseWeight;
    [HorizontalGroup("A1")]
    [LabelWidth(80)]
    [LabelText("Ȩ��ÿ������")]
    public float WeightAddPerEnterCount;
    [HorizontalGroup("A1")]
    [LabelWidth(80)]
    [LabelText("���Ȩ��")]
    public float WeightMax;
}
