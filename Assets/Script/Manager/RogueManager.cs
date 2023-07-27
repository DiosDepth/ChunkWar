using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RogueManager : Singleton<RogueManager>
{

    private Dictionary<int, ShopGoodsInfo> goodsItems;

    public RogueManager()
    {
        Initialization();
    }

    public override void Initialization()
    {
        base.Initialization();
        goodsItems = new Dictionary<int, ShopGoodsInfo>();
    }

    /// <summary>
    /// ���ݲ��κ�ϡ�жȻ�ȡ���Ȩ��
    /// </summary>
    /// <param name="waveIndex"></param>
    /// <param name="rarity"></param>
    /// <returns></returns>
    private float GetWeightByRarityAndWave(int waveIndex, GoodsItemRarity rarity)
    {
        var rarityMap = DataManager.Instance.shopCfg.RarityMap;
        if (!rarityMap.ContainsKey(rarity))
            return 0;

        ///TODO
        var playerLuck = 0;

        var rarityCfg = rarityMap[rarity];
        if (waveIndex < rarityCfg.MinAppearWave)
            return 0;

        var waveIndexDelta = waveIndex - rarityCfg.LuckModifyMinWave;
        return rarityCfg.WeightAddPerWave * waveIndexDelta + rarityCfg.BaseWeight * (100 + playerLuck);
    }
    
}
