using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopMainConfig : SerializedScriptableObject
{
    [LabelText("ˢ�����Ļ���")]
    [ListDrawerSettings(ShowIndexLabels = true, NumberOfItemsPerPage = 10)]
    public int[] RollCostBase = new int[20];

    [LabelText("ÿ��ˢ�����Ӱٷֱ�")]
    [ListDrawerSettings(ShowIndexLabels = true, NumberOfItemsPerPage = 10)]
    public int[] RoolCostIncreaceMap = new int[20];


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

[System.Serializable]
public class ShopGoodsItemConfig
{
    [TableColumnWidth(60, false)]
    public int GoodID;

    [TableColumnWidth(80,false)]
    public GoodsItemRarity Rarity;

    [TableColumnWidth(80, false)]
    public int CostBase;

    [TableColumnWidth(50, false)]
    public bool Unique;

    [TableColumnWidth(100, false)]
    public int MaxBuyCount = -1;

    [TableColumnWidth(200, false)]
    public string Name;
    public string Desc;
}

[System.Serializable]
[HideReferenceObjectPicker]
public class ShopGoodsRarityConfig
{
    [HorizontalGroup("A1")]
    [LabelWidth(80)]
    [LabelText("��С���ֲ���")]
    public byte MinAppearWave;


    [HorizontalGroup("A1")]
    [LabelWidth(120)]
    [LabelText("������Ч��С����")]
    public byte LuckModifyMinWave;

    [HorizontalGroup("A1")]
    [LabelWidth(80)]
    [LabelText("����Ȩ��")]
    public byte BaseWeight;
    [HorizontalGroup("A1")]
    [LabelWidth(80)]
    [LabelText("Ȩ��ÿ������")]
    public float WeightAddPerWave;
    [HorizontalGroup("A1")]
    [LabelWidth(80)]
    [LabelText("���Ȩ��")]
    public float WeightMax;
}
