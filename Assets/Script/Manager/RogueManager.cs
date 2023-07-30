using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

public struct RogueEvent
{
    public RogueEventType type;
    public object[] param;

    public RogueEvent(RogueEventType m_type, params object[] param)
    {
        type = m_type;
        this.param = param;
    }

    public static void Trigger(RogueEventType m_type, params object[] param)
    {
        RogueEvent evt = new RogueEvent(m_type, param);
        EventCenter.Instance.TriggerEvent<RogueEvent>(evt);
    }
}

public struct ShipPropertyEvent
{
    public ShipPropertyEventType type;
    public object[] param;

    public ShipPropertyEvent(ShipPropertyEventType m_type, params object[] param)
    {
        type = m_type;
        this.param = param;
    }

    public static void Trigger(ShipPropertyEventType m_type, params object[] param)
    {
        ShipPropertyEvent evt = new ShipPropertyEvent(m_type, param);
        EventCenter.Instance.TriggerEvent<ShipPropertyEvent>(evt);
    }
}

public enum RogueEventType
{
    CurrencyChange,
    ShopReroll,
    ShopCostChange,
    ShipPlugChange,
}

public enum ShipPropertyEventType
{
    EXPChange,
    LevelUp,
}

public class RogueManager : Singleton<RogueManager>
{
    /// <summary>
    /// 主要属性
    /// </summary>
    public UnitPropertyData MainPropertyData;


    public SaveData saveData;
    public InventoryItem currentShipSelection;
    public Ship currentShip;
    /// <summary>
    /// 所有商店物品
    /// </summary>
    private Dictionary<int, ShopGoodsInfo> goodsItems;

    private Dictionary<int, byte> _playerCurrentGoods;

    /// <summary>
    /// 当前舰船插件物品
    /// </summary>
    private Dictionary<uint, ShipPlugInfo> _currentShipPlugs = new Dictionary<uint, ShipPlugInfo>();
    public List<ShipPlugInfo> GetAllCurrentShipPlugs
    {
        get { return _currentShipPlugs.Values.ToList(); }
    }

    /// <summary>
    /// 当前刷新次数
    /// </summary>
    private int _currentRereollCount = 0;

    private int _waveIndex;
    public int GetCurrentWaveIndex
    {
        get { return _waveIndex; }
    }

    private ChangeValue<int> _playerCurrency;
    /// <summary>
    /// 当前货币
    /// </summary>
    public int CurrentCurrency
    {
        get { return _playerCurrency.Value; }
    }

    /// <summary>
    /// 当前刷新花费
    /// </summary>
    public int CurrentRerollCost
    {
        get;
        private set;
    }
    /// <summary>
    /// 商店进入总次数
    /// </summary>
    private byte _shopEnterTotalCount = 0;

    private static int ShopGoods_UID_Sep = 1000000;

    /// <summary>
    /// 当前随机商店物品
    /// </summary>
    public List<ShopGoodsInfo> CurrentRogueShopItems
    {
        get;
        private set;
    }

    public RogueManager()
    {
        Initialization();
    }

    public void InitRogueBattle()
    {
        MainPropertyData = new UnitPropertyData();
        _waveIndex = 1;
        InitDefaultProperty();
        InitShopData();
        InitAllGoodsItems();
        GenerateShopGoods(3, 1);
    }

    public override void Initialization()
    {
        base.Initialization();
        goodsItems = new Dictionary<int, ShopGoodsInfo>();
        _playerCurrentGoods = new Dictionary<int, byte>();
    }

    /// <summary>
    /// 增加货币
    /// </summary>
    /// <param name="value"></param>
    public void AddCurrency(int value)
    {
        var oldValue = _playerCurrency.Value;
        var newValue = oldValue + value;
        _playerCurrency.Set(newValue);
    }


    private void OnCurrencyChange(int oldValue, int newValue)
    {
        RogueEvent.Trigger(RogueEventType.CurrencyChange);
    }

    private void InitDefaultProperty()
    {
        var maxLevel = DataManager.Instance.battleCfg.ShipMaxLevel;
        _shipLevel = new ChangeValue<byte>(1, 1, maxLevel);
        _currentEXP = new ChangeValue<int>(0, 0, int.MaxValue);
        MainPropertyData.RegisterRowProperty(PropertyModifyKey.ShipHP, DataManager.Instance.battleCfg.PlayerShip_Default_HP);
        CurrentRequireEXP = GameHelper.GetEXPRequireMaxCount(GetCurrentShipLevel);

        _currentEXP.BindChangeAction(OnCurrrentEXPChange);
        _shipLevel.BindChangeAction(OnShipLevelUp);
    }

    #region Property

    private ChangeValue<byte> _shipLevel;
    /// <summary>
    /// 舰船等级
    /// </summary>
    public byte GetCurrentShipLevel
    {
        get { return _shipLevel.Value; }
    }

    /// <summary>
    /// 当前EXP
    /// </summary>
    private ChangeValue<int> _currentEXP;
    public int GetCurrentExp
    {
        get { return _currentEXP.Value; }
    }

    /// <summary>
    /// 当前所需EXP最大值
    /// </summary>
    public int CurrentRequireEXP
    {
        get;
        private set;
    }

    /// <summary>
    /// exp百分比， 0.76
    /// </summary>
    public float EXPPercent
    {
        get { return GetCurrentExp / (float)CurrentRequireEXP; }
    }

    /// <summary>
    /// 增加经验值
    /// </summary>
    /// <param name="value"></param>
    public void AddEXP(int value)
    {
        var targetValue = GetCurrentExp + value;
        while (targetValue >= CurrentRequireEXP)
        {
            ///LevelUp
            LevelUp();
            targetValue = targetValue - CurrentRequireEXP;
        }
        _currentEXP.Set(targetValue);
    }

    private void LevelUp()
    {
        var newLevel = GetCurrentShipLevel + 1;
        if(newLevel > DataManager.Instance.battleCfg.ShipMaxLevel)
        {
            return;
        }

        byte targetLevel = (byte)Mathf.Clamp(newLevel, 1, byte.MaxValue);
        _shipLevel.Set(targetLevel);
        CurrentRequireEXP = GameHelper.GetEXPRequireMaxCount(targetLevel);
        
    }

    private void OnCurrrentEXPChange(int old, int newValue)
    {
        ShipPropertyEvent.Trigger(ShipPropertyEventType.EXPChange);
    }

    private void OnShipLevelUp(byte old, byte newValue)
    {
        ShipPropertyEvent.Trigger(ShipPropertyEventType.LevelUp);
    }

    #endregion

    #region Shop

    public ShopGoodsInfo GetShopGoodsInfo(int goodsID)
    {
        ShopGoodsInfo info = null;
        goodsItems.TryGetValue(goodsID, out info);
        return info;
    }

    /// <summary>
    /// 购买商品
    /// </summary>
    /// <param name="info"></param>
    public bool BuyItem(ShopGoodsInfo info)
    {
        if (!info.CheckCanBuy())
            return false;

        var cost = info.Cost;
        AddCurrency(-cost);
        info.OnItemSold();
        GainShopItem(info);
        return true;
    }

    private void GainShopItem(ShopGoodsInfo info)
    {
        ///RefreshCount
        if (_playerCurrentGoods.ContainsKey(info.GoodsID))
        {
            _playerCurrentGoods[info.GoodsID]++;
            
        }
        else
        {
            _playerCurrentGoods.Add(info.GoodsID, 1);
        }

        var itemType = info._cfg.ItemType;
        int typeID = info._cfg.TypeID; 
        if (itemType == GoodsItemType.ShipPlug)
        {
            AddNewShipPlug(typeID, info.GoodsID);
        }

    }
    
    /// <summary>
    /// 刷新商店
    /// </summary>
    /// <returns></returns>
    public bool RefreshShop()
    {
        if (CurrentRerollCost > CurrentCurrency)
            return false;

        _currentRereollCount++;
        ///Cost
        CurrentRerollCost = GetCurrentRefreshCost();
        AddCurrency(-CurrentRerollCost);
        ///RefreshShop
        var refreshCount = GetCurrentShopRefreshCount();
        GenerateShopGoods(refreshCount, _shopEnterTotalCount);
        RogueEvent.Trigger(RogueEventType.ShopReroll);
        Debug.Log("刷新商店，刷新次数 = " + _currentRereollCount);
        return true;
    }

    /// <summary>
    /// 获取当前拥有的物件数量
    /// </summary>
    /// <param name="goodsID"></param>
    /// <returns></returns>
    public byte GetCurrentGoodsCount(int goodsID)
    {
        if (_playerCurrentGoods.ContainsKey(goodsID))
            return _playerCurrentGoods[goodsID];

        return 0;
    }

    /// <summary>
    /// 生成商店物品
    /// </summary>
    /// <param name="count"></param>
    /// <param name="enterCount"></param>
    /// <returns></returns>
    public List<ShopGoodsInfo> GenerateShopGoods(byte count, int enterCount)
    {
        List<ShopGoodsInfo> result = new List<ShopGoodsInfo>();
        var allVaild = goodsItems.Values.ToList().FindAll(x => x.IsVaild);

        var tier2Rate = GetWeightByRarityAndEnterCount(enterCount, GoodsItemRarity.Tier2);
        var tier3Rate = GetWeightByRarityAndEnterCount(enterCount, GoodsItemRarity.Tier3);
        var tier4Rate = GetWeightByRarityAndEnterCount(enterCount, GoodsItemRarity.Tier4);
        var tier1Rate = 100 - tier2Rate - tier3Rate - tier4Rate;
        
        List<ShopGoodsRarityItem> rarityItems = new List<ShopGoodsRarityItem>();
        rarityItems.Add(new ShopGoodsRarityItem(GoodsItemRarity.Tier2, tier2Rate));
        rarityItems.Add(new ShopGoodsRarityItem(GoodsItemRarity.Tier3, tier3Rate));
        rarityItems.Add(new ShopGoodsRarityItem(GoodsItemRarity.Tier4, tier4Rate));
        rarityItems.Add(new ShopGoodsRarityItem(GoodsItemRarity.Tier1, tier1Rate));

        for (int i = 0; i < count; i++) 
        {
            var rarityResult = Utility.GetRandomList<ShopGoodsRarityItem>(rarityItems, 1);
            if(rarityResult.Count <= 0)
            {
                Debug.LogError("Shop Item RandomError! WaveIndex = " + enterCount);
                continue;
            }

            var rarity = rarityResult[0];
            var allVaildRarityItems = allVaild.FindAll(x => x.Rarity == rarity.Rarity && !result.Contains(x));

            var goodsReusult = Utility.GetRandomList<ShopGoodsInfo>(allVaildRarityItems, 1);
            if (goodsReusult.Count > 0)
            {
                var goods = goodsReusult[0];
                goods.OnItemAddToShop();
                result.Add(goods);
                ///如果数量到上限或者unique,从列表中去除
                if (goods._cfg.Unique)
                {
                    allVaild.Remove(goods);
                }
                else if (goods._cfg.MaxBuyCount > 0)
                {
                    var currentCount = GetCurrentGoodsCount(goods.GoodsID);
                    var currentRandomPoolCount = result.FindAll(x => x.GoodsID == goods.GoodsID).Count;
                    if(currentCount + currentRandomPoolCount + 1 >= goods._cfg.MaxBuyCount)
                    {
                        allVaild.Remove(goods);
                    }
                }
            }
        }

#if UNITY_EDITOR
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < result.Count; i++) 
        {
            sb.Append(result[i].GetLog());
        }
        Debug.Log(sb.ToString());
#endif
        CurrentRogueShopItems = result;
        return result;
    }

    private void InitShopData()
    {
        MainPropertyData.RegisterRowProperty(PropertyModifyKey.ShopRefreshCount, DataManager.Instance.battleCfg.RogueShop_Origin_RefreshNum);
        _playerCurrency = new ChangeValue<int>(1000, int.MinValue, int.MaxValue);
        _playerCurrency.BindChangeAction(OnCurrencyChange);
        CurrentRerollCost = GetCurrentRefreshCost();

        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.ShopCostPercent, OnShopCostChange);
    }

    /// <summary>
    /// 根据波次和稀有度获取随机权重
    /// </summary>
    /// <param name="enterCount"></param>
    /// <param name="rarity"></param>
    /// <returns></returns>
    private float GetWeightByRarityAndEnterCount(int enterCount, GoodsItemRarity rarity)
    {
        var rarityMap = DataManager.Instance.shopCfg.RarityMap;
        if (!rarityMap.ContainsKey(rarity))
            return 0;

        ///TODO
        var playerLuck = 0;

        var rarityCfg = rarityMap[rarity];
        if (enterCount < rarityCfg.MinAppearEneterCount)
            return 0;

        var waveIndexDelta = enterCount - rarityCfg.LuckModifyMinEnterCount;
        return rarityCfg.WeightAddPerEnterCount * waveIndexDelta + rarityCfg.BaseWeight * (100 + playerLuck);
    }

    private void InitAllGoodsItems()
    {
        var allGoods = DataManager.Instance.shopCfg.Goods;
        for(int i = 0; i < allGoods.Count; i++)
        {
            var goodsCfg = allGoods[i];
            if (!goodsItems.ContainsKey(goodsCfg.GoodID))
            {
                var goodsInfo = ShopGoodsInfo.CreateGoods(goodsCfg.GoodID);
                goodsItems.Add(goodsInfo.GoodsID, goodsInfo);
            }
        }
    }


    /// <summary>
    /// 当前商店刷新商品数量
    /// </summary>
    /// <returns></returns>
    private byte GetCurrentShopRefreshCount()
    {
        var refreshCount = MainPropertyData.GetPropertyFinal(PropertyModifyKey.ShopRefreshCount);
        return (byte)Mathf.Clamp(refreshCount, 1, 6);
    }

    /// <summary>
    /// 当前刷新耗费
    /// </summary>
    /// <returns></returns>
    private int GetCurrentRefreshCost()
    {
        var rerollParam = DataManager.Instance.shopCfg.RollCostIncreaceWaveParam;

        int rollBase = 999;
        var rollCostBaseMap = DataManager.Instance.shopCfg.RollCostWaveBase;
        if (_waveIndex < rollCostBaseMap.Length)
        {
            rollBase = rollCostBaseMap[_waveIndex];
        }

        var rerollIncrease = Mathf.RoundToInt(_waveIndex * rerollParam);

        return _currentRereollCount * rerollIncrease + rollBase;
    }

    /// <summary>
    /// 商店花费刷新
    /// </summary>
    private void OnShopCostChange()
    {
        RogueEvent.Trigger(RogueEventType.ShopCostChange);
    }

    #endregion

    #region Ship Plug

    public ShipPlugInfo GetShipPlug(uint uid)
    {
        if (_currentShipPlugs.ContainsKey(uid))
        {
            return _currentShipPlugs[uid];
        }
        return null;
    }

    /// <summary>
    /// 根据插件UID获取商品数据
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public ShopGoodsInfo GetShipPlugGoodsInfo(uint uid)
    {
        var plugInfo = GetShipPlug(uid);
        if (plugInfo == null)
            return null;
        var goods = GetShopGoodsInfo(plugInfo.GoodsID);
        return goods;
    }



    public  void AddNewShipPlug(int plugID, int goodsID = -1)
    {
        var plugInfo = ShipPlugInfo.CreateInfo(plugID, goodsID);
        if (plugInfo == null)
            return;

        var uid = GetShipPlugUID();
        plugInfo.PlugUID = uid;
        plugInfo.OnAdded();
        _currentShipPlugs.Add(uid, plugInfo);
        RogueEvent.Trigger(RogueEventType.ShipPlugChange);
    }



    private uint GetShipPlugUID()
    {
        var uid = (uint)UnityEngine.Random.Range(1, ShopGoods_UID_Sep);
        if (_currentShipPlugs.ContainsKey(uid))
            return GetShipPlugUID();

        return uid;
    }
    #endregion
}
