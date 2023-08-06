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
    ShipUnitTempSlotChange,
    RefreshShopWeaponInfo,
}

public enum ShipPropertyEventType
{
    CoreHPChange,
    EXPChange,
    LevelUp,
}

public class RogueManager : Singleton<RogueManager>
{
    /// <summary>
    /// 主要属性
    /// </summary>
    public UnitPropertyData MainPropertyData;
    public ShipMapData ShipMapData;
    public SaveData saveData;
    public InventoryItem currentShipSelection;
    public PlayerShip currentShip;
    public LevelTimer Timer;

    public HardLevelInfo CurrentHardLevel
    {
        get;
        private set;
    }

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
    /// 当前舰船建筑组件
    /// </summary>
    private Dictionary<uint, Unit> _currentShipUnits = new Dictionary<uint, Unit>();

    /// <summary>
    /// 当前刷新次数
    /// </summary>
    private int _currentRereollCount = 0;

    private int _waveIndex;
    public int GetCurrentWaveIndex
    {
        get { return _waveIndex; }
    }
    private int _tempWaveTime;
    /// <summary>
    /// 当前剩余时间
    /// </summary>
    public int GetTempWaveTime
    {
        get { return _tempWaveTime; }
    }

    private ChangeValue<float> _playerCurrency;
    /// <summary>
    /// 当前货币
    /// </summary>
    public int CurrentCurrency
    {
        get { return Mathf.RoundToInt(_playerCurrency.Value); }
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
        InitWave();
        InitDefaultProperty();
        InitShopData();
        InitAllGoodsItems();
        AddNewShipPlug((currentShipSelection.itemconfig as PlayerShipConfig).CorePlugID);
    }

    public override void Initialization()
    {
        base.Initialization();
        goodsItems = new Dictionary<int, ShopGoodsInfo>();
        _playerCurrentGoods = new Dictionary<int, byte>();
    }

    /// <summary>
    /// 设置HardLevel
    /// </summary>
    /// <param name="info"></param>
    public void SetCurrentHardLevel(HardLevelInfo info)
    {
        CurrentHardLevel = info;
    }

    /// <summary>
    /// 当前关卡卸载，一般为进入habor
    /// </summary>
    public void OnMainLevelUnload()
    {
        _tempWaveTime = Timer.TotalSecond;
        ///RefreshShop
        GenerateShopGoods(3, _shopEnterTotalCount);
    }

    /// <summary>
    /// 增加货币
    /// </summary>
    /// <param name="value"></param>
    public void AddCurrency(float value)
    {
        var currencyGainAdd = MainPropertyData.GetPropertyFinal(PropertyModifyKey.CurrencyAddPercent);

        var oldValue = _playerCurrency.Value;
        var newValue = oldValue + (value * (1 + currencyGainAdd / 100f));
        _playerCurrency.Set(newValue);
    }


    private void OnCurrencyChange(float oldValue, float newValue)
    {
        RogueEvent.Trigger(RogueEventType.CurrencyChange);
    }

    private void InitDefaultProperty()
    {
        HarborTempUnitSlotCount = DataManager.Instance.battleCfg.HarborMapTempUnitSlotCount;
        var maxLevel = DataManager.Instance.battleCfg.ShipMaxLevel;
        _shipLevel = new ChangeValue<byte>(1, 1, maxLevel);
        _currentEXP = new ChangeValue<float>(0, 0, int.MaxValue);
        CurrentRequireEXP = GameHelper.GetEXPRequireMaxCount(GetCurrentShipLevel);

        _currentEXP.BindChangeAction(OnCurrrentEXPChange);
        _shipLevel.BindChangeAction(OnShipLevelUp);

        ///BindProperty Change
        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.DamagePercent, () => { RefreshWeaponItemInfo(UI_WeaponUnitPropertyType.Damage); });
        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.PhysicsDamage, () => { RefreshWeaponItemInfo(UI_WeaponUnitPropertyType.Damage); });
        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.EnergyDamage, () => { RefreshWeaponItemInfo(UI_WeaponUnitPropertyType.Damage); });
    }

    #region Wave & HardLevel

    private int _currentHardLevel;
    /// <summary>
    /// 当前难度等级
    /// </summary>
    public int GetHardLevel
    {
        get { return _currentHardLevel; }
    }

    /// <summary>
    /// 计算hardLevel等级
    /// </summary>
    /// <returns></returns>
    public int CalculateHardLevelIndex()
    {
        return 0;
    }

    private void InitWave()
    {
        _waveIndex = 1;
        _tempWaveTime = GetCurrentWaveTime();
        Timer = new LevelTimer();
        var totalTime =GetTempWaveTime;
        Timer.InitTimer(totalTime);
    }

    private int GetCurrentWaveTime()
    {
        var waveCfg = CurrentHardLevel.GetWaveConfig(GetCurrentWaveIndex);
        if (waveCfg == null)
            return 0;
        return waveCfg.DurationTime;
    }

    /// <summary>
    /// 波次结束
    /// </summary>
    public void OnWaveFinish()
    {
        if (IsFinalWave())
        {
            ///Level Success
            return;
        }

        _waveIndex++;
        _tempWaveTime = GetCurrentWaveTime();
    }

    private bool IsFinalWave()
    {
        var waveCount = CurrentHardLevel.Cfg.WaveConfig.Count;
        return GetCurrentWaveIndex >= waveCount;
    }

    #endregion

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
    private ChangeValue<float> _currentEXP;
    public int GetCurrentExp
    {
        get { return Mathf.RoundToInt(_currentEXP.Value); }
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
    public void AddEXP(float value)
    {
        var expAddtion = MainPropertyData.GetPropertyFinal(PropertyModifyKey.EXPAddPercent);

        var targetValue = GetCurrentExp + value * (1 + expAddtion / 100f);
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

    private void OnCurrrentEXPChange(float old, float newValue)
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
        else if (itemType == GoodsItemType.ShipUnit)
        {
            AddNewTempUintToHarbor(typeID, info.GoodsID);
            ///只有装备上去再加数量
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
        _playerCurrency = new ChangeValue<float>(1000, int.MinValue, int.MaxValue);
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

    private void RefreshWeaponItemInfo(UI_WeaponUnitPropertyType type)
    {
        RogueEvent.Trigger(RogueEventType.RefreshShopWeaponInfo, type);
    }

    #endregion

    #region Ship Unit


    private List<int> harborTempUnitSlotIDs = new List<int>();
    /// <summary>
    /// 临时建筑组件库存
    /// </summary>
    public List<int> HarborTempUnitSlots
    {
        get { return harborTempUnitSlotIDs; }
    }
    private byte HarborTempUnitSlotCount;
    

    public void AddNewShipUnit(Unit unit)
    {
        var uid = GetShipUnitUID();
        unit.UID = uid;
        _currentShipUnits.Add(uid, unit);
    }

    /// <summary>
    /// 添加建筑到临时库存
    /// </summary>
    /// <param name="unitID"></param>
    /// <param name="goodsID"></param>
    private void AddNewTempUintToHarbor(int unitID, int goodsID = -1)
    {
        if (harborTempUnitSlotIDs.Count > HarborTempUnitSlotCount)
            return;

        harborTempUnitSlotIDs.Add(unitID);
        RogueEvent.Trigger(RogueEventType.ShipUnitTempSlotChange, true, unitID);
    }

    private uint GetShipUnitUID()
    {
        var uid = (uint)UnityEngine.Random.Range(ShopGoods_UID_Sep, uint.MaxValue);
        if (_currentShipUnits.ContainsKey(uid))
            return GetShipUnitUID();

        return uid;
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

    /// <summary>
    /// 增加插件
    /// </summary>
    /// <param name="plugID"></param>
    /// <param name="goodsID"> 如果为-1 则会自动找商品ID</param>
    public void AddNewShipPlug(int plugID, int goodsID = -1)
    {
        if(goodsID == -1)
        {
            goodsID = GetGoodsIDByPlugID(plugID);
        }

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

    private int GetGoodsIDByPlugID(int plugID)
    {
        foreach(var item in goodsItems.Values)
        {
            var cfg = item._cfg;
            if (cfg.ItemType == GoodsItemType.ShipPlug && cfg.TypeID == plugID)
                return item.GoodsID;
        }
        return 0;
    }
    #endregion
}
