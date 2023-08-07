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
    /// ��Ҫ����
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
    /// �����̵���Ʒ
    /// </summary>
    private Dictionary<int, ShopGoodsInfo> goodsItems;

    private Dictionary<int, byte> _playerCurrentGoods;

    /// <summary>
    /// ��ǰ���������Ʒ
    /// </summary>
    private Dictionary<uint, ShipPlugInfo> _currentShipPlugs = new Dictionary<uint, ShipPlugInfo>();
    public List<ShipPlugInfo> GetAllCurrentShipPlugs
    {
        get { return _currentShipPlugs.Values.ToList(); }
    }

    /// <summary>
    /// ��ǰ�����������
    /// </summary>
    private Dictionary<uint, Unit> _currentShipUnits = new Dictionary<uint, Unit>();

    /// <summary>
    /// ��ǰˢ�´���
    /// </summary>
    private int _currentRereollCount = 0;

    private int _waveIndex;
    public int GetCurrentWaveIndex
    {
        get { return _waveIndex; }
    }
    private int _tempWaveTime;
    /// <summary>
    /// ��ǰʣ��ʱ��
    /// </summary>
    public int GetTempWaveTime
    {
        get { return _tempWaveTime; }
    }

    private ChangeValue<float> _playerCurrency;
    /// <summary>
    /// ��ǰ����
    /// </summary>
    public int CurrentCurrency
    {
        get { return Mathf.RoundToInt(_playerCurrency.Value); }
    }

    /// <summary>
    /// ��ǰˢ�»���
    /// </summary>
    public int CurrentRerollCost
    {
        get;
        private set;
    }
    /// <summary>
    /// �̵�����ܴ���
    /// </summary>
    private byte _shopEnterTotalCount = 0;

    private static int ShopGoods_UID_Sep = 1000000;

    /// <summary>
    /// ��ǰ����̵���Ʒ
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
    /// ����HardLevel
    /// </summary>
    /// <param name="info"></param>
    public void SetCurrentHardLevel(HardLevelInfo info)
    {
        CurrentHardLevel = info;
    }

    /// <summary>
    /// ��ǰ�ؿ�ж�أ�һ��Ϊ����habor
    /// </summary>
    public void OnMainLevelUnload()
    {
        _tempWaveTime = Timer.CurrentSecond;
        ///RefreshShop
        GenerateShopGoods(3, _shopEnterTotalCount);
    }

    /// <summary>
    /// ���ӻ���
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
    /// ��ǰ�Ѷȵȼ�
    /// </summary>
    public int GetHardLevelValue
    {
        get { return _currentHardLevel; }
    }

    /// <summary>
    /// ����hardLevel�ȼ�
    /// </summary>
    /// <returns></returns>
    private void CalculateHardLevelIndex()
    {
        var currentSecond = Timer.TotalSeconds;
        var secondsLevel = Mathf.RoundToInt(currentSecond / (float)(2 * 60));

        var wave = DataManager.Instance.battleCfg.HardLevelWaveIndexMultiple;
        _currentHardLevel = wave * (GetCurrentWaveIndex - 1) + secondsLevel;
        Debug.Log("Update HardLevel , HardLevel = " + _currentHardLevel);
    }

    private void InitWave()
    {
        _waveIndex = 1;
        _tempWaveTime = GetCurrentWaveTime();
        Timer = new LevelTimer();
        var totalTime = GetTempWaveTime;
        Timer.InitTimer(totalTime);

        var hardLevelDelta = DataManager.Instance.battleCfg.HardLevelDeltaSeconds;
        Timer.AddTrigger(LevelTimerTrigger.CreateTriger(hardLevelDelta, -1, CalculateHardLevelIndex, "HardLevelUpdate"));
        CalculateHardLevelIndex();
    }

    private int GetCurrentWaveTime()
    {
        var waveCfg = CurrentHardLevel.GetWaveConfig(GetCurrentWaveIndex);
        if (waveCfg == null)
            return 0;
        return waveCfg.DurationTime;
    }

    /// <summary>
    /// ���ν���
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
    /// �����ȼ�
    /// </summary>
    public byte GetCurrentShipLevel
    {
        get { return _shipLevel.Value; }
    }

    /// <summary>
    /// ��ǰEXP
    /// </summary>
    private ChangeValue<float> _currentEXP;
    public int GetCurrentExp
    {
        get { return Mathf.RoundToInt(_currentEXP.Value); }
    }

    /// <summary>
    /// ��ǰ����EXP���ֵ
    /// </summary>
    public int CurrentRequireEXP
    {
        get;
        private set;
    }

    /// <summary>
    /// exp�ٷֱȣ� 0.76
    /// </summary>
    public float EXPPercent
    {
        get { return GetCurrentExp / (float)CurrentRequireEXP; }
    }

    /// <summary>
    /// ���Ӿ���ֵ
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
    /// ������Ʒ
    /// </summary>
    /// <param name="info"></param>
    public bool BuyItem(ShopGoodsInfo info)
    {
        if (!info.CheckCanBuy())
            return false;

        ///��ʱ�ֿ��������޷�����
        if (info._cfg.ItemType == GoodsItemType.ShipUnit && IsTempUnitHarborFull())
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
            ///ֻ��װ����ȥ�ټ�����
        }
    }
    
    /// <summary>
    /// ˢ���̵�
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
        Debug.Log("ˢ���̵꣬ˢ�´��� = " + _currentRereollCount);
        return true;
    }

    /// <summary>
    /// ��ȡ��ǰӵ�е��������
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
    /// �����̵���Ʒ
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
                ///������������޻���unique,���б���ȥ��
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
    /// ���ݲ��κ�ϡ�жȻ�ȡ���Ȩ��
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
    /// ��ǰ�̵�ˢ����Ʒ����
    /// </summary>
    /// <returns></returns>
    private byte GetCurrentShopRefreshCount()
    {
        var refreshCount = MainPropertyData.GetPropertyFinal(PropertyModifyKey.ShopRefreshCount);
        return (byte)Mathf.Clamp(refreshCount, 1, 6);
    }

    /// <summary>
    /// ��ǰˢ�ºķ�
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
    /// �̵껨��ˢ��
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


    private int[] harborTempUnitSlotIDs;
    /// <summary>
    /// ��ʱ����������
    /// </summary>
    public int[] HarborTempUnitSlots
    {
        get { return harborTempUnitSlotIDs; }
    }
    public byte CurrentSelectedHarborSlotIndex;
    
    public void InitTempUnitSlots()
    {
        var count = DataManager.Instance.battleCfg.HarborMapTempUnitSlotCount;
        harborTempUnitSlotIDs = new int[count];
        for (int i = 0; i < harborTempUnitSlotIDs.Length; i++) 
        {
            harborTempUnitSlotIDs[i] = -1;
        }
    }

    public void RemoveTempUnitInHarbor(byte slotIndex)
    {
        harborTempUnitSlotIDs[slotIndex] = -1;
    }

    public void AddNewShipUnit(Unit unit)
    {
        var uid = GetShipUnitUID();
        unit.UID = uid;
        _currentShipUnits.Add(uid, unit);
    }

    /// <summary>
    /// ��ʱ�ֿ��Ƿ�����
    /// </summary>
    /// <returns></returns>
    private bool IsTempUnitHarborFull()
    {
        for(int i =0;i< harborTempUnitSlotIDs.Length; i++)
        {
            if (harborTempUnitSlotIDs[i] == -1)
                return false;
        }
        return true;
    }

    /// <summary>
    /// ��ӽ�������ʱ���
    /// </summary>
    /// <param name="unitID"></param>
    /// <param name="goodsID"></param>
    private void AddNewTempUintToHarbor(int unitID, int goodsID = -1)
    {
        var emptySlot = GetEmptyTempUnitSlot();
        if (emptySlot == -1)
            return;

        harborTempUnitSlotIDs[emptySlot] = unitID;
        RogueEvent.Trigger(RogueEventType.ShipUnitTempSlotChange, true, unitID, (byte)emptySlot);
    }

    private uint GetShipUnitUID()
    {
        var uid = (uint)UnityEngine.Random.Range(ShopGoods_UID_Sep, uint.MaxValue);
        if (_currentShipUnits.ContainsKey(uid))
            return GetShipUnitUID();

        return uid;
    }

    private int GetEmptyTempUnitSlot()
    {
        for (int i = 0; i < harborTempUnitSlotIDs.Length; i++) 
        {
            if (harborTempUnitSlotIDs[i] == -1)
                return i;
        }
        return -1;
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
    /// ���ݲ��UID��ȡ��Ʒ����
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
    /// ���Ӳ��
    /// </summary>
    /// <param name="plugID"></param>
    /// <param name="goodsID"> ���Ϊ-1 ����Զ�����ƷID</param>
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
