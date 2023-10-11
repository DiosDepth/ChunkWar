using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

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
    HoverUnitDisplay,
    HideHoverUnitDisplay,
    WreckageDropRefresh,
    CancelWreckageSelect,
    WreckageAddToShip,
    RefreshWreckage,
    WasteCountChange,
    ShopTeleportSpawn,
    ShopTeleportWarning,
    RegisterBossHPBillBoard,
    RemoveBossHPBillBoard,
    CampBuffUpgrade,
    RefreshTimerDisplay,
    ShowUnitSelectOptionPanel,
    HideUnitSelectOptionPanel
}

public enum ShipPropertyEventType
{
    MainPropertyValueChange,
    CoreHPChange,
    EXPChange,
    LevelUp,
    ReloadCDStart,
    ReloadCDEnd,
    EnergyChange,
    WreckageLoadChange,
}

public class RogueManager : Singleton<RogueManager>, IPauseable
{
    /// <summary>
    /// 主要属性
    /// </summary>
    public UnitPropertyData MainPropertyData;
    public ShipMapData ShipMapData;

    public InventoryItem currentShipSelection;
    public InventoryItem currentWeaponSelection;
    public PlayerShip currentShip;
    public LevelTimer Timer;

    private Dictionary<GoodsItemRarity, int> _inLevelDropItems = new Dictionary<GoodsItemRarity, int>
    {
        { GoodsItemRarity.Tier1, 0 },
        { GoodsItemRarity.Tier2, 0 },
        { GoodsItemRarity.Tier3, 0 },
        { GoodsItemRarity.Tier4, 0 },
    };
    /// <summary>
    /// 获取局内残骸掉落
    /// </summary>
    public Dictionary<GoodsItemRarity, int> GetInLevelWreckageDrops
    {
        get { return _inLevelDropItems; }
    }

    public HardLevelInfo CurrentHardLevel
    {
        get;
        private set;
    }

    /// <summary>
    /// 战斗结果
    /// </summary>
    public BattleResultInfo BattleResult
    {
        get;
        private set;
    }

    /// <summary>
    /// 是否在局内
    /// </summary>
    public bool InBattle
    {
        get;
        private set;
    }

    /// <summary>
    /// 是否显示舰船升级界面
    /// </summary>
    public bool IsShowingShipLevelUp = false;

    /// <summary>
    /// 所有商店物品
    /// </summary>
    private Dictionary<int, ShopGoodsInfo> goodsItems;
    private Dictionary<int, WreckageItemInfo> wreckageItems;
    private Dictionary<int, int> plugItemCountDic;

    /// <summary>
    /// 当前舰船插件物品
    /// </summary>
    private Dictionary<uint, ShipPlugInfo> _currentShipPlugs = new Dictionary<uint, ShipPlugInfo>();
    public List<ShipPlugInfo> AllCurrentShipPlugs = new List<ShipPlugInfo>();

    /// <summary>
    /// 局内特殊加成
    /// </summary>
    private List<PropertyModifySpecialData> globalModifySpecialDatas = new List<PropertyModifySpecialData>();

    /// <summary>
    /// 当前舰船建筑组件
    /// </summary>
    private Dictionary<uint, Unit> _currentShipUnits = new Dictionary<uint, Unit>();
    public List<Unit> AllShipUnits = new List<Unit>();

    private BattleSpecialEntitySpawnConfig _entitySpawnConfig;
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
    private byte _shopRefreshTotalCount = 0;

    /// <summary>
    /// 当前随机商店物品
    /// </summary>
    public List<ShopGoodsInfo> CurrentRogueShopItems
    {
        get;
        private set;
    }

    /// <summary>
    /// 当前进入站点时Unit0
    /// </summary>
    public Dictionary<uint, WreckageItemInfo> CurrentWreckageItems
    {
        get;
        private set;
    }

    /// <summary>
    /// 舰船升级时属性随机池
    /// </summary>
    private List<ShipLevelUpItem> _allShipLevelUpItems;

    /// <summary>
    /// 波次结束后自动进入港口
    /// </summary>
    private bool autoEnterHarbor = true;

    #region Action 

    /* 负载百分比变化 */
    public UnityAction<float> OnWreckageLoadPercentChange;
    /* 能源百分比变化 */
    public UnityAction<float> OnEnergyPercentChange;
    /* 残骸数量变化 */
    public UnityAction<int> OnWasteCountChange;
    /* 残骸物件数量变化  ID : Count*/
    public UnityAction<int, int> OnShipPlugCountChange;
    /* 波次变化  bool : 波次开始或结束*/
    public UnityAction<bool> OnWaveStateChange;
    /* 商店刷新  刷新次数 */
    public UnityAction<int> OnShopRefresh;
    /* 物品数量变更 */
    public UnityAction OnItemCountChange;
    /* 进入太空港 */
    public UnityAction OnEnterHarbor;

    private void ClearAction()
    {
        OnWreckageLoadPercentChange = null;
        OnEnergyPercentChange = null;
        OnWasteCountChange = null;
        OnShipPlugCountChange = null;
        OnWaveStateChange = null;
        OnShopRefresh = null;
        OnItemCountChange = null;
        OnEnterHarbor = null;
    }

    #endregion

    public RogueManager()
    {
        Initialization();
    }

    /// <summary>
    /// Reset Game
    /// </summary>
    public void Clear()
    {
        InBattle = false;
        currentShip = null;
        CurrentHardLevel = null;
        currentShipSelection = null;
        currentWeaponSelection = null;
        ShipMapData = null;
        Timer = null;
        BattleResult = null;
        ///Reset
        _inLevelDropItems[GoodsItemRarity.Tier1] = 0;
        _inLevelDropItems[GoodsItemRarity.Tier2] = 0;
        _inLevelDropItems[GoodsItemRarity.Tier3] = 0;
        _inLevelDropItems[GoodsItemRarity.Tier4] = 0;

        plugItemCountDic.Clear();
        MainPropertyData.Clear();
        goodsItems.Clear();
        wreckageItems.Clear();
        _currentShipPlugs.Clear();
        AllCurrentShipPlugs.Clear();
        globalModifySpecialDatas.Clear();
        _currentShipUnits.Clear();
        AllShipUnits.Clear();
        CurrentRogueShopItems.Clear();
        CurrentWreckageItems.Clear();
        _allShipLevelUpItems.Clear();
        CurrentShipLevelUpItems.Clear();

        _currentRereollCount = 0;
        _shopRefreshTotalCount = 0;
        ClearAction();
    }

    public void PauseGame()
    {
        Timer.Pause();
    }

    public void UnPauseGame()
    {
        Timer.StartTimer();
    }

    /// <summary>
    /// 舰船选择界面初始化
    /// </summary>
    public void InitShipSelection()
    {
        MainPropertyData = new UnitPropertyData();
        InitDefaultProperty();
    }

    /// <summary>
    /// 更新战斗
    /// </summary>
    public void OnUpdateBattle()
    {
        for (int i = 0; i < AllCurrentShipPlugs.Count; i++) 
        {
            AllCurrentShipPlugs[i].OnBattleUpdate();
        }

        for (int i = 0; i < AllShipUnits.Count; i++) 
        {
            AllShipUnits[i].OnUpdateBattle();
        }
    }

    /// <summary>
    /// 进入harbor
    /// </summary>
    public void OnEnterHarborInit()
    {
        OnEnterHarbor?.Invoke();

        var newWreckage = GenerateWreckageItems();
        for(int i = 0; i < newWreckage.Count; i++)
        {
            var wreckage = newWreckage[i];
            var uid = ModifyUIDManager.Instance.GetUID(PropertyModifyCategory.Wreckage, wreckage);
            wreckage.UID = uid;
            CurrentWreckageItems.Add(uid, wreckage);
        }
        CalculateTotalLoadCost();

        ///Reset
        _inLevelDropItems[GoodsItemRarity.Tier1] = 0;
        _inLevelDropItems[GoodsItemRarity.Tier2] = 0;
        _inLevelDropItems[GoodsItemRarity.Tier3] = 0;
        _inLevelDropItems[GoodsItemRarity.Tier4] = 0;
    }

    /// <summary>
    /// 舰船选择界面临时预览属性
    /// </summary>
    public void SetTempShipSelectionPreview(int shipID)
    {
        MainPropertyData.Clear();
        _currentShipPlugs.Clear();
        var shipCfg = DataManager.Instance.GetShipConfig(shipID);
        if(shipCfg != null)
        {
            ///TempAdd
            AddNewShipPlug(shipCfg.CorePlugID);
        }
    }

    /// <summary>
    /// 进入战斗初始化
    /// </summary>
    public void InitRogueBattle()
    {
        InBattle = true;
        MainPropertyData.Clear();
        _currentShipPlugs.Clear();
        AllCurrentShipPlugs.Clear();
        globalModifySpecialDatas.Clear();
        goodsItems.Clear();

        ShipMapData = new ShipMapData((currentShipSelection.itemconfig as PlayerShipConfig).Map);

        InitEnergyAndWreckageCommonBuff();
        InitWave();
        InitWreckageData();
        InitShopData();
        InitAllGoodsItems();
        InitModifyPlugTagWeight();
        ///InitPlugs
        InitShipPlugs();
        InitOriginShipUnit();
    }

    public void PlayCurrentHardLevelBGM()
    {
        if (CurrentHardLevel == null)
            return;

        var bgmEvent = CurrentHardLevel.BGMEventName;
        SoundManager.Instance.PlayBGM(bgmEvent);
    }

    /// <summary>
    /// 设置关卡胜利
    /// </summary>
    public void SetBattleSuccess()
    {
        BattleResult.Success = true;
    }

    /// <summary>
    /// 关卡结束
    /// </summary>
    public void RogueBattleOver()
    {
        currentShip?.controller.GameOver();
        Timer.Pause();
        Timer.RemoveAllTrigger();
        SettleCampScore();
    }

    public override void Initialization()
    {
        base.Initialization();
        plugItemCountDic = new Dictionary<int, int>();
        BattleResult = new BattleResultInfo();
        goodsItems = new Dictionary<int, ShopGoodsInfo>();
        CurrentRogueShopItems = new List<ShopGoodsInfo>();
        CurrentShipLevelUpItems = new List<ShipLevelUpItem>();
        _entitySpawnConfig = DataManager.Instance.battleCfg.EntitySpawnConfig;
        GameManager.Instance.RegisterPauseable(this);
        InitShipLevelUpItems();
    }

    /// <summary>
    /// 设置HardLevel
    /// </summary>
    /// <param name="info"></param>
    public void SetCurrentHardLevel(HardLevelInfo info)
    {
        CurrentHardLevel = info;
        ///Init PropertyDic
        var propertyDic = info.Cfg.ModifyDic;
        if(propertyDic != null && propertyDic.Count > 0)
        {
            foreach(var item in propertyDic)
            {
                switch (item.Key)
                {
                    case HardLevelModifyType.EnemyDamage:
                        MainPropertyData.AddPropertyModifyValue(PropertyModifyKey.EnemyDamagePercent, PropertyModifyType.Modify, GameGlobalConfig.PropertyModifyUID_HardLevel, item.Value);
                        break;
                    case HardLevelModifyType.EnemyHP:
                        MainPropertyData.AddPropertyModifyValue(PropertyModifyKey.EnemyHPPercent, PropertyModifyType.Modify, GameGlobalConfig.PropertyModifyUID_HardLevel, item.Value);
                        break;
                }
            }
        }

        ///WreckageStart
        var startWreckageDic = info.Cfg.StartWreckageDic;
        if(startWreckageDic != null && startWreckageDic.Count > 0)
        {
            foreach(var item in startWreckageDic)
            {
                if(item.Value > 0)
                {
                    if (_inLevelDropItems.ContainsKey(item.Key))
                    {
                        _inLevelDropItems[item.Key] = item.Value;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 当前关卡卸载，一般为进入habor
    /// </summary>
    public void OnMainLevelUnload()
    {
        ClearShip();
        _tempWaveTime = Timer.CurrentSecond;

    }

    public void ClearShip()
    {
        if(currentShip != null)
        {
            (currentShip.controller as ShipController).shipUnitManager.Unload();
            GameObject.Destroy(currentShip.container.gameObject);
            currentShip = null;
        }

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
        var delta = newValue - oldValue;
        if(delta > 0)
        {
            ///Achievement
            AchievementManager.Instance.Trigger<int>(AchievementWatcherType.CurrencyChange, Mathf.CeilToInt(delta));
        }
    }

    private void InitDefaultProperty()
    {
        var maxLevel = DataManager.Instance.battleCfg.ShipMaxLevel;
        _shipLevel = new ChangeValue<byte>(1, 1, maxLevel);
        _currentEXP = new ChangeValue<float>(0, 0, int.MaxValue);
        CurrentRequireEXP = GameHelper.GetEXPRequireMaxCount(GetCurrentShipLevel);

        _currentEXP.BindChangeAction(OnCurrrentEXPChange);
        _shipLevel.BindChangeAction(OnShipLevelUp);
    }

    /// <summary>
    /// 结算阵营积分
    /// </summary>
    private void SettleCampScore()
    {
        var totalScore = CalculateCurrentWaveScore(true);
        var hardLevelRatio = CurrentHardLevel.Cfg.ScoreRatio;
        totalScore = Mathf.RoundToInt(totalScore * hardLevelRatio);
        BattleResult.Score = totalScore;
        var playerCampID = currentShip.playerShipCfg.PlayerShipCampID;
        var campData = GameManager.Instance.GetCampDataByID(playerCampID);
        if(campData != null)
        {
            campData.AddCampScore(totalScore, out BattleResult.campLevelInfo);
        }
    }

    /// <summary>
    /// 舰船初始化携带的额外Building
    /// </summary>
    private void InitOriginShipUnit()
    {
        if (currentShipSelection == null)
            return;

        var shipCfg = currentShipSelection.itemconfig as PlayerShipConfig;
        var shipUnitCfg = shipCfg.OriginUnits;
        if(shipUnitCfg != null && shipUnitCfg.Count > 0)
        {
            for(int i = 0; i < shipUnitCfg.Count; i++)
            {
                var unitCfg = DataManager.Instance.GetUnitConfig(shipUnitCfg[i].UnitID);
                if (unitCfg == null)
                    return;

                UnitInfo info = new UnitInfo(shipUnitCfg[i]);
                info.occupiedCoords = ShipMapData.GetOccupiedCoords(unitCfg.ID, info.pivot);

                ShipMapData.UnitList.Add(info);
            }
        }
    }

    #region Drop

    /// <summary>
    /// 总负载消耗
    /// </summary>
    public float WreckageTotalLoadCost
    {
        get;
        private set;
    }

    /// <summary>
    /// 总负载值
    /// </summary>
    public int WreckageTotalLoadValue
    {
        get;
        private set;
    }

    /// <summary>
    /// 负载百分比
    /// </summary>
    public float WreckageLoadPercent
    {
        get { return (WreckageTotalLoadValue * 100) / (float)WreckageTotalLoadCost; }
    }

    private ChangeValue<int> _dropWasteCount;

    /// <summary>
    /// 舰船残骸数量
    /// </summary>
    public int GetDropWasteCount
    {
        get { return _dropWasteCount.Value; }
    }

    /// <summary>
    /// 舰船残骸负载
    /// </summary>
    public float GetDropWasteLoad
    {
        get
        {
            var baseValue = DataManager.Instance.battleCfg.Drop_Waste_LoadCostBase;
            var loadcostPercent = 100 - MainPropertyData.GetPropertyFinal(PropertyModifyKey.WasteLoadPercent);
            loadcostPercent = Mathf.Clamp(loadcostPercent, 0, float.MaxValue);
            return GetDropWasteCount * baseValue * loadcostPercent / 100f;
        }
    }

    /// <summary>
    /// 增加局内掉落
    /// </summary>
    /// <param name="rarity"></param>
    public void AddInLevelDrop(GoodsItemRarity rarity)
    {
        _inLevelDropItems[rarity]++;
        RogueEvent.Trigger(RogueEventType.WreckageDropRefresh);
        AchievementManager.Instance.Trigger<GoodsItemRarity>(AchievementWatcherType.WreckageGain, rarity);
    }

    /// <summary>
    /// 出售残骸
    /// </summary>
    /// <param name="count"></param>
    public void SellWaste(int count)
    {
        int sellCount = Mathf.Clamp(count, 0, GetDropWasteCount);
        var sellPrice = GameHelper.CalculateWasteSellPrice(sellCount);
        AddCurrency(sellPrice);
        var newCount = GetDropWasteCount - sellCount;
        _dropWasteCount.Set(newCount);
        OnWasteCountChange?.Invoke(newCount);
        RogueEvent.Trigger(RogueEventType.WasteCountChange);
    }

    public WreckageItemInfo GetCurrentWreckageByUID(uint uid)
    {
        if (CurrentWreckageItems.ContainsKey(uid))
            return CurrentWreckageItems[uid];
        return null;
    }

    public WreckageItemInfo CreateAndAddNewWreckageInfo(int unitID)
    {
        if (wreckageItems.ContainsKey(unitID))
        {
            var info = wreckageItems[unitID].Clone();
            var uid = ModifyUIDManager.Instance.GetUID(PropertyModifyCategory.Wreckage, info);
            info.UID = uid;
            CurrentWreckageItems.Add(uid, info);
            CalculateTotalLoadCost();
            RogueEvent.Trigger(RogueEventType.RefreshWreckage);
        }
        return null;
    }

    public void RemoveWreckageByUID(uint uid)
    {
        var info = GetCurrentWreckageByUID(uid);
        if (info != null)
        {
            info.OnRemove();
        }
        CurrentWreckageItems.Remove(uid);
        ModifyUIDManager.Instance.RemoveUID(uid);
        CalculateTotalLoadCost();
    }

    public void AddDropWasteCount(int count)
    {
        int newCount = GetDropWasteCount + count;
        _dropWasteCount.Set(newCount);
        OnWasteCountChange?.Invoke(newCount);
        ///UpdateLoad
        CalculateTotalLoadCost();
        RogueEvent.Trigger(RogueEventType.WasteCountChange);
    }

    /// <summary>
    /// 获取局内所有残骸包总数
    /// </summary>
    /// <returns></returns>
    public int GetInLevelTotalWreckageDropCount()
    {
        int result = 0;
        foreach(var item in GetInLevelWreckageDrops.Values)
        {
            result += item;
        }
        return result;
    }

    /// <summary>
    /// 生成局内掉落物品
    /// </summary>
    /// <returns></returns>
    private List<WreckageItemInfo> GenerateWreckageItems()
    {
        List<WreckageItemInfo> result = new List<WreckageItemInfo>();
        foreach(var rarity in _inLevelDropItems)
        {
            var vaildItems = GetAllWreckageItemsByRarity(rarity.Key);
            var count = rarity.Value;
            for (int i = 0; i < count; i++) 
            {
                var outItem = Utility.GetRandomList<WreckageItemInfo>(vaildItems, 1);
                if(outItem != null && outItem.Count == 1)
                {
                    result.Add(outItem[0].Clone());
                }
            }
        }
#if UNITY_EDITOR
        StringBuilder sb = new StringBuilder();
        sb.Append("=======Generate WreckageItems ======= \n");
        for(int i = 0; i < result.Count; i++)
        {
            sb.Append(result[i].GetLog());
        }
        sb.Append("==========END==========");
        Debug.Log(sb.ToString());
#endif

        return result;
    }

    /// <summary>
    /// 初始化掉落
    /// </summary>
    private void InitWreckageData()
    {
        _dropWasteCount = new ChangeValue<int>(0, 0, int.MaxValue);
        CurrentWreckageItems = new Dictionary<uint, WreckageItemInfo>();
        wreckageItems = new Dictionary<int, WreckageItemInfo>();
        var allWreckgeData = DataManager.Instance.shopCfg.WreckageDrops;
        for(int i = 0; i < allWreckgeData.Count; i++)
        {
            WreckageItemInfo info = WreckageItemInfo.CreateInfo(allWreckgeData[i]);
            wreckageItems.Add(info.UnitID, info);
        }
        ///权重根据舰船修正
        InitModifyUnitWreckageTagWeight();
        CalculateTotalLoadValue();
        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.UnitWreckageLoadAdd, CalculateTotalLoadValue);
        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.UnitLoadCost, CalculateTotalLoadCost);
        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.WasteLoadPercent, CalculateTotalLoadCost);
    }

    private List<WreckageItemInfo> GetAllWreckageItemsByRarity(GoodsItemRarity rarity)
    {
        return wreckageItems.Values.ToList().FindAll(x => x.Rarity == rarity);
    }

    /// <summary>
    /// 根据选择舰船修正整体tag权重，相同权重为加法关系
    /// </summary>
    private void InitModifyUnitWreckageTagWeight()
    {
        if (currentShip == null)
            return;

        var modifyDic = currentShip.playerShipCfg.UnitRandomTagRatioDic;
        if (modifyDic == null)
            return;

        foreach (var item in wreckageItems.Values)
        {
            var unitCfg = DataManager.Instance.GetUnitConfig(item.UnitID);
            if (unitCfg == null)
                continue;

            float ratioBase = 1;
            foreach (var modify in modifyDic)
            {
                if (unitCfg.HasUnitTag(modify.Key))
                {
                    ratioBase += modify.Value;
                }
            }

            item.Weight = Mathf.RoundToInt(ratioBase * item.Weight);
        }

    }

    /// <summary>
    /// 计算总负载
    /// </summary>
    private void CalculateTotalLoadCost()
    {
        WreckageTotalLoadCost = 0;
        foreach(var item in CurrentWreckageItems.Values)
        {
            WreckageTotalLoadCost += item.LoadCost;
        }
        ///Calculate Waste
        WreckageTotalLoadCost += GetDropWasteLoad;
        OnWreckageLoadPercentChange?.Invoke(WreckageLoadPercent * 100);
        ShipPropertyEvent.Trigger(ShipPropertyEventType.WreckageLoadChange);
    }

    private void CalculateTotalLoadValue()
    {
        var loadRow = DataManager.Instance.battleCfg.ShipLoadBase;
        float totalLoad = 0;

        var UnitloadAdd = MainPropertyData.GetPropertyFinal(PropertyModifyKey.UnitWreckageLoadAdd);
        UnitloadAdd = (1 + UnitloadAdd / 100f);
        for (int i = 0; i < AllShipUnits.Count; i++) 
        {
            var unit = AllShipUnits[i];
            float unitLoad = unit._baseUnitConfig.LoadAdd;
            if(unit._baseUnitConfig.HasUnitTag(ItemTag.WareHouse))
            {
                unitLoad *= UnitloadAdd;
            }

            totalLoad += unitLoad;
        }

        WreckageTotalLoadValue = (int)Mathf.Clamp(totalLoad + loadRow, 0, int.MaxValue);
        OnWreckageLoadPercentChange?.Invoke(WreckageLoadPercent * 100);
    }

    #endregion

    
    #region Wave & HardLevel

    private int _currentHardLevelIndex;
    /// <summary>
    /// 当前难度等级
    /// </summary>
    public int GetHardLevelValueIndex
    {
        get { return _currentHardLevelIndex; }
    }

    /// <summary>
    /// 计算hardLevel等级
    /// </summary>
    /// <returns></returns>
    private void CalculateHardLevelIndex(int paramInt = 0)
    {
        var currentSecond = Timer.TotalSeconds;
        var secondsLevel = Mathf.RoundToInt(currentSecond / (float)(2 * 60));

        var multiple = GameGlobalConfig.HardLevelWaveIndexMultiple;
        _currentHardLevelIndex = multiple * (GetCurrentWaveIndex) + secondsLevel;
        Debug.Log("Update HardLevel , HardLevel = " + _currentHardLevelIndex);
    }

    private void InitWave()
    {
        _waveIndex = 1;
        _tempWaveTime = GetCurrentWaveTime();
        Timer = new LevelTimer();
        var totalTime = GetTempWaveTime;
        Timer.InitTimer(totalTime);

        var hardLevelDelta = DataManager.Instance.battleCfg.HardLevelDeltaSeconds;
        var trigger = LevelTimerTrigger.CreateTriger(0, hardLevelDelta, -1, "HardLevelUpdate");
        trigger.BindChangeAction(CalculateHardLevelIndex);
        Timer.AddTrigger(trigger);
        CalculateHardLevelIndex();
        AddWaveTrigger();
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
    public async void OnWaveFinish()
    {
        ///Reset TriggerDatas
        ResetAllPlugModifierTriggerDatas();
        ResetAllUnitModifierTriggerDatas();
        if (IsFinalWave())
        {
            await UniTask.Delay(1000);
            ///Level Success
            SetBattleSuccess();
            GameEvent.Trigger(EGameState.EGameState_GameOver);
            return;
        }

        if (!autoEnterHarbor)
        {
            LevelManager.Instance.CreateHarborPickUp();
        }

        Timer.PauseAndSetZero();
        _waveIndex++;
        LevelManager.Instance.CollectAllPickUps();
        await UniTask.Delay(1000);

        if (autoEnterHarbor)
        {
            GameStateTransitionEvent.Trigger(EGameState.EGameState_GameHarbor);
        }

        ///无限波次等处理
        OnWaveStateChange?.Invoke(false);
    }

    /// <summary>
    /// 波次开始
    /// </summary>
    public async void OnNewWaveStart()
    {
        _tempWaveTime = GetCurrentWaveTime();
        AddWaveTrigger();
        CalculateHardLevelIndex();
        Timer.InitTimer(_tempWaveTime);
        Timer.StartTimer();
       
        OnWaveStateChange?.Invoke(true);
    }

    private bool IsFinalWave()
    {
        var waveCount = CurrentHardLevel.WaveCount;
        return GetCurrentWaveIndex >= waveCount;
    }

    private void AddWaveTrigger()
    {
        Timer.RemoveAllTrigger();
        GenerateEnemyAIFactory();
        GenerateShopCreateTimer();
        ///增加船体值自动恢复Trigger
        var recoverTrigger = LevelTimerTrigger.CreateTriger(0, 1, -1, "UnitHPRecover");
        recoverTrigger.BindChangeAction(OnUpdateUnitHPRecover);
        Timer.AddTrigger(recoverTrigger);
    }

    /// <summary>
    /// 生成敌人生成器
    /// </summary>
    private void GenerateEnemyAIFactory()
    {
        var waveCfg = CurrentHardLevel.GetWaveConfig(GetCurrentWaveIndex);
        if (waveCfg == null)
            return;

        var enemyCfg = waveCfg.SpawnConfig;
        for(int i = 0; i < enemyCfg.Count; i++)
        {
            var cfg = enemyCfg[i];
            var trigger = LevelTimerTrigger.CreateTriger(cfg.StartTime, cfg.DurationDelta, cfg.LoopCount);
            trigger.BindChangeAction(CreateFactory, cfg.ID);
            Timer.AddTrigger(trigger);
        }
        ///Generate Meteorite Common
        var MeteoriteTrigger1 = LevelTimerTrigger.CreateTriger(0, _entitySpawnConfig.MeteoriteGenerate_Timer1, -1, "MeteoriteGenerate_1");
        MeteoriteTrigger1.BindChangeAction(CreateMeteorite_Common);
        Timer.AddTrigger(MeteoriteTrigger1);

        ///Generate Meteorite Smooth
        var MeteoriteTrigger2 = LevelTimerTrigger.CreateTriger(0, _entitySpawnConfig.MeteoriteGenerate_Timer2, -1, "MeteoriteGenerate_2");
        MeteoriteTrigger2.BindChangeAction(CreateMeteorite_Smooth);
        Timer.AddTrigger(MeteoriteTrigger2);
    }

    private void GenerateShopCreateTimer()
    {
        var waveCfg = CurrentHardLevel.GetWaveConfig(GetCurrentWaveIndex);
        if (waveCfg == null)
            return;

        var shopCfg = waveCfg.ShopRefreshTimeMap;
        if (shopCfg == null || shopCfg.Length <= 0)
            return;

        for (int i = 0; i < shopCfg.Length; i++)
        {
            var time = shopCfg[i];
            var trigger = LevelTimerTrigger.CreateTriger(time, 0, 1);
            trigger.BindChangeAction(CreateShopTeleport);
            Timer.AddTrigger(trigger);
        }
    }

    /// <summary>
    /// 陨石生成器
    /// </summary>
    private void CreateMeteorite_Common()
    {
        var count = MainPropertyData.GetPropertyFinal(PropertyModifyKey.Meteorite_Generate_Count) + 1;
        count = Mathf.Clamp(count, 1, GameGlobalConfig.MeteoriteGenerate_MaxCount);
        float targetCount = _entitySpawnConfig.MeteoriteGenerate_Rate1;

        while (targetCount >= 0)
        {
            if (targetCount < 1)
            {
                bool generate = Utility.CalculateRate100(targetCount * 100);
                if (generate)
                {
                    GenerateMeteorite();
                }
                break;
            }

            ///GenerateFactory
            GenerateMeteorite();
            targetCount--;
        }
    }

    private void CreateMeteorite_Smooth()
    {
        var count = MainPropertyData.GetPropertyFinal(PropertyModifyKey.Meteorite_Generate_Count) + 1;
        count = Mathf.Clamp(count, 1, GameGlobalConfig.MeteoriteGenerate_MaxCount);

        var rate = count / _entitySpawnConfig.MeteoriteGenerate_Rate2;
        bool generate = Utility.CalculateRate100(rate * 100);
        if (generate)
        {
            GenerateMeteorite();
        }
    }

    /// <summary>
    /// 生成陨石
    /// </summary>
    private void GenerateMeteorite()
    {
        ///Calculate MeteoriteGenerate Tier
        var waveCfg = CurrentHardLevel.GetWaveConfig(GetCurrentWaveIndex);
        if (waveCfg == null)
            return;

        ///随机陨石稀有度
        var rarityMap = waveCfg.MeteoriteGenerateRate_RarityMap;
        List<GeneralRarityRandomItem> randomItems = new List<GeneralRarityRandomItem>();
        foreach (var item in rarityMap)
        {
            if (item.Value <= 0)
                continue;

            GeneralRarityRandomItem ran = new GeneralRarityRandomItem
            {
                Rarity = item.Key,
                Weight = item.Value
            };
            randomItems.Add(ran);
        }

        var targetResult = Utility.GetRandomList<GeneralRarityRandomItem>(randomItems, 1);
        if (targetResult.Count != 1)
            return;

        var targetRarity = targetResult[0].Rarity;
        var typeID = _entitySpawnConfig.GetMeteoriteGenerate_TypeID(targetRarity);
        if (typeID == -1)
            return;

        WaveEnemySpawnConfig cfg = new WaveEnemySpawnConfig
        {
            AITypeID = typeID,
            DurationDelta = 0,
            LoopCount = 1,
            TotalCount = 1,
        };
        SpawnEntity(cfg);
    }

    private void CreateFactory(int ID)
    {
        var waveCfg = CurrentHardLevel.GetWaveConfig(GetCurrentWaveIndex);
        if (waveCfg == null)
            return;

        var cfg = waveCfg.SpawnConfig.Find(x => x.ID == ID);
        SpawnEntity(cfg);
    }

    private void SpawnEntity(WaveEnemySpawnConfig cfg)
    {
        if (cfg == null || currentShip == null)
            return;

        Vector2 spawnpoint = MathExtensionTools.GetRadomPosFromOutRange(_entitySpawnConfig.EnemyGenerate_Inner_Range, _entitySpawnConfig.EnemyGenerate_Outer_Range, currentShip.transform.position.ToVector2());
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.AIFactoryPath, true, (obj) =>
        {
            AIFactory aIFactory = obj.GetComponent<AIFactory>();
            aIFactory.PoolableSetActive(true);
            aIFactory.Initialization();
            RectAISpawnSetting spawnSetting = new RectAISpawnSetting(cfg.AITypeID, cfg.TotalCount, cfg.MaxRowCount);
            spawnSetting.spawnIntervalTime = cfg.SpawnIntervalTime;
            spawnSetting.spawnShape = cfg.SpawnShpe;

            aIFactory.StartSpawn(spawnpoint, spawnSetting, (list) =>
            {
                AIManager.Instance.AddAIRange(list);
                //LevelManager.Instance.airuntimedata.AddAIDataRange(list);
            });
        });
    }

    private void CreateShopTeleport()
    {
        _shopRefreshTotalCount++;
        LevelManager.Instance.CreateShopPickUp();
    }

    /// <summary>
    /// 计算关卡最终得分
    /// </summary>
    /// <param name="finalWaveWin"></param>
    /// <returns></returns>
    private int CalculateCurrentWaveScore(bool finalWaveWin)
    {
        int totalScore = 0;
        for (int i = GetCurrentWaveIndex - 1; i >= 1; i--) 
        {
            var waveCfg = CurrentHardLevel.GetWaveConfig(i);
            totalScore += waveCfg.waveScore;
        }

        ///检查是否最后一波是否胜利
        var waveTotalCount = CurrentHardLevel.WaveCount;
        if (GetCurrentWaveIndex == waveTotalCount && finalWaveWin)
        {
            ///FinalWave
            var waveCfg = CurrentHardLevel.GetWaveConfig(GetCurrentWaveIndex);
            totalScore += waveCfg.waveScore;
        }
        return totalScore;
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
        if (GetCurrentShipLevel >= GameGlobalConfig.Ship_MaxLevel)
            return;

        var expAddtion = MainPropertyData.GetPropertyFinal(PropertyModifyKey.EXPAddPercent);

        var oldLevel = GetCurrentShipLevel;
        var targetValue = GetCurrentExp + value * (1 + expAddtion / 100f);
        int levelUpCount = 0;
        while (targetValue >= CurrentRequireEXP)
        {
            ///LevelUp
            LevelUp();
            levelUpCount++;
            targetValue = targetValue - CurrentRequireEXP;
        }
        _currentEXP.Set(targetValue);
        ///Show UI
        if(levelUpCount > 0)
        {
            ShipLevelUp(oldLevel,levelUpCount);
        }
    }

    private void LevelUp()
    {
        if (GetCurrentShipLevel >= GameGlobalConfig.Ship_MaxLevel)
            return;

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

    /// <summary>
    /// 初始化超载和能源负载BUFF
    /// </summary>
    private void InitEnergyAndWreckageCommonBuff()
    {
        var energyBuff = DataManager.Instance.gameMiscCfg.EnergyOverloadBuff;
        var wreckageBuff = DataManager.Instance.gameMiscCfg.WreckageOverloadBuff;

        for(int i = 0; i < energyBuff.Length; i++)
        {
            PropertyModifySpecialData data = new PropertyModifySpecialData(energyBuff[i], GameGlobalConfig.PropertyModifyUID_EnergyOverload_GlobalBuff);
            globalModifySpecialDatas.Add(data);
        }

        for(int i = 0; i < wreckageBuff.Length; i++)
        {
            PropertyModifySpecialData data = new PropertyModifySpecialData(wreckageBuff[i], GameGlobalConfig.PropertyModifyUID_WreckageOverload_GlobalBuff);
            globalModifySpecialDatas.Add(data);
        }
    }

    #endregion

    #region Shop

    /// <summary>
    /// 进入商店
    /// </summary>
    public void EnterShop()
    {
        GameManager.Instance.PauseGame();
        RefreshShop(false);
        InputDispatcher.Instance.ChangeInputMode("UI");
        CameraManager.Instance.SetFollowPlayerShip(-10);
        CameraManager.Instance.SetOrthographicSize(20);
        UIManager.Instance.HiddenUI("ShipHUD");
        UIManager.Instance.ShowUI<ShopHUD>("ShopHUD", E_UI_Layer.Mid, this, (panel) => 
        {
            panel.Initialization();
        });
    }

    /// <summary>
    /// 离开商店
    /// </summary>
    public void ExitShop()
    {
        GameManager.Instance.UnPauseGame();
        InputDispatcher.Instance.ChangeInputMode("Player");
        CameraManager.Instance.SetFollowPlayerShip();
        CameraManager.Instance.SetOrthographicSize(40);
        UIManager.Instance.HiddenUI("ShopHUD");
        UIManager.Instance.ShowUI<ShipHUD>("ShipHUD", E_UI_Layer.Mid, this, (panel) =>
        {
            panel.Initialization();
        });
    }

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

    public bool ChangeShopItemLockState(ShopGoodsInfo info)
    {
        if (info == null)
            return false;

        info.IsLock = !info.IsLock;
        return info.IsLock;
    }

    private void GainShopItem(ShopGoodsInfo info)
    {
        AddNewShipPlug(info._cfg.TypeID, info.GoodsID);
    }
    
    /// <summary>
    /// 刷新商店
    /// </summary>
    /// <returns></returns>
    public bool RefreshShop(bool useCurrency)
    {
        if (useCurrency)
        {
            if (CurrentRerollCost > CurrentCurrency)
                return false;

            ///Cost
            CurrentRerollCost = GetCurrentRefreshCost();
            AddCurrency(-CurrentRerollCost);
        }

        byte itemLockCount = 0;

        ///Reset
        if (CurrentRogueShopItems != null && CurrentRogueShopItems.Count > 0) 
        {
            CurrentRogueShopItems.ForEach(x => 
            {
                x.Reset();
                if (x.IsLock)
                    itemLockCount++;
            });
        }

        _currentRereollCount++;
        ///RefreshShop
        var refreshCount = GetCurrentShopRefreshCount();
        ///去除已经锁定的物品
        refreshCount = (byte)Mathf.Max(0, refreshCount - itemLockCount);
        GenerateShopGoods(refreshCount, _shopRefreshTotalCount);
        RogueEvent.Trigger(RogueEventType.ShopReroll);

        OnShopRefresh?.Invoke(_currentRereollCount);

        Debug.Log("刷新商店，刷新次数 = " + _currentRereollCount);
        return true;
    }

    /// <summary>
    /// 获取当前拥有的插件数量
    /// </summary>
    /// <param name="goodsID"></param>
    /// <returns></returns>
    public byte GetCurrentPlugCount(int plugID)
    {
        byte count = 0;
        for(int i = 0; i < AllCurrentShipPlugs.Count; i++)
        {
            var id = AllCurrentShipPlugs[i].PlugID;
            if (id == plugID)
                count++;
        }
        return count;
    }

    /// <summary>
    /// 生成商店物品
    /// </summary>
    /// <param name="count"></param>
    /// <param name="enterCount"></param>
    /// <returns></returns>
    private void GenerateShopGoods(byte count, int enterCount)
    {
        List<ShopGoodsInfo> result = new List<ShopGoodsInfo>();
        var allVaild = goodsItems.Values.ToList().FindAll(x => x.IsVaild && !x.IsLock);

        var tier2Rate = GetWeightByRarityAndEnterCount(enterCount, GoodsItemRarity.Tier2);
        var tier3Rate = GetWeightByRarityAndEnterCount(enterCount, GoodsItemRarity.Tier3);
        var tier4Rate = GetWeightByRarityAndEnterCount(enterCount, GoodsItemRarity.Tier4);
        var tier1Rate = 100 - tier2Rate - tier3Rate - tier4Rate;
        
        List<GeneralRarityRandomItem> rarityItems = new List<GeneralRarityRandomItem>();
        rarityItems.Add(new GeneralRarityRandomItem(GoodsItemRarity.Tier2, tier2Rate));
        rarityItems.Add(new GeneralRarityRandomItem(GoodsItemRarity.Tier3, tier3Rate));
        rarityItems.Add(new GeneralRarityRandomItem(GoodsItemRarity.Tier4, tier4Rate));
        rarityItems.Add(new GeneralRarityRandomItem(GoodsItemRarity.Tier1, tier1Rate));

        for (int i = 0; i < count; i++) 
        {
            var rarityResult = Utility.GetRandomList<GeneralRarityRandomItem>(rarityItems, 1);
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
                ///如果数量到上限,从列表中去除
                if (goods._cfg.MaxBuyCount > 0)
                {
                    var currentCount = GetCurrentPlugCount(goods._cfg.TypeID);
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

        ///Combine ShopItems
        for(int i = CurrentRogueShopItems.Count - 1; i >= 0; i--)
        {
            ///已锁定的物品不变化
            if (CurrentRogueShopItems[i].IsLock)
                continue;

            if(result.Count > 0)
            {
                CurrentRogueShopItems[i] = result[result.Count - 1];
                result.RemoveAt(result.Count - 1);
            }
            else
            {
                CurrentRogueShopItems.RemoveAt(i);
            }
        }

        if(result.Count > 0)
        {
            CurrentRogueShopItems.AddRange(result);
        }
    }

    /// <summary>
    /// 获取随机X个当前商店的商品
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public List<ShopGoodsInfo> GetRandomCurrentRogueShopItems(int count)
    {
        if (CurrentRogueShopItems.Count >= count)
            return CurrentRogueShopItems;

        return Utility.GetRandomList<ShopGoodsInfo>(CurrentRogueShopItems, count);
    }

    private void InitShopData()
    {
        MainPropertyData.AddPropertyModifyValue(PropertyModifyKey.ShopRefreshCount, PropertyModifyType.Row, 0,DataManager.Instance.battleCfg.RogueShop_Origin_RefreshNum);
        _playerCurrency = new ChangeValue<float>(0, int.MinValue, int.MaxValue);
        _playerCurrency.BindChangeAction(OnCurrencyChange);
        CurrentRerollCost = GetCurrentRefreshCost();
        _playerCurrency.Set(CurrentHardLevel.Cfg.StartCurrency);
        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.ShopCostPercent, OnShopCostChange);
    }

    /// <summary>
    /// 根据选择舰船修正整体tag权重，相同权重为加法关系
    /// </summary>
    private void InitModifyPlugTagWeight()
    {
        if (currentShip == null)
            return;

        var modifyDic = currentShip.playerShipCfg.PlugRandomTagRatioDic;
        if (modifyDic == null)
            return;

        foreach(var item in goodsItems.Values)
        {
            var plugCfg = DataManager.Instance.GetShipPlugItemConfig(item._cfg.TypeID);
            if (plugCfg == null)
                continue;

            float ratioBase = 1;
            foreach (var modify in modifyDic)
            {
                if (plugCfg.HasPlugTag(modify.Key))
                {
                    ratioBase += modify.Value;
                }
            }

            item.Weight = Mathf.RoundToInt(ratioBase * item.Weight);
        }

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
        return rarityCfg.WeightAddPerEnterCount * waveIndexDelta + rarityCfg.BaseWeight * (1 + playerLuck / 100f);
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

    #region Ship Unit

    public void AddNewShipUnit(Unit unit)
    {
        var uid = ModifyUIDManager.Instance.GetUID(PropertyModifyCategory.ShipUnit, unit);
        unit.UID = uid;
        unit.OnAdded();
        OnItemCountChange?.Invoke();
        _currentShipUnits.Add(uid, unit);
        AllShipUnits.Add(unit);
    }

    public bool SellUnit(Unit unit)
    {
        if (unit == null)
            return false;

        ///主武器无法出售
        if (unit._baseUnitConfig.unitType == UnitType.MainWeapons)
            return false;

        if (!_currentShipUnits.ContainsKey(unit.UID))
            return false;

        var sellPrice = GameHelper.GetUnitSellPrice(unit);
        AddCurrency(sellPrice);
        return true;
    }


    public Unit GetPlayerShipUnit(uint UID)
    {
        if (_currentShipUnits.ContainsKey(UID))
            return _currentShipUnits[UID];
        return null;
    }

    public int GetShipUnitCountByItemRarity(GoodsItemRarity rarity)
    {
        return AllShipUnits.FindAll(x => x._baseUnitConfig.GeneralConfig.Rarity == rarity).Count;
    }

    public void RemoveShipUnit(Unit unit)
    {
        unit.OnRemove();
        OnItemCountChange?.Invoke();
        AllShipUnits.Remove(unit);
        _currentShipUnits.Remove(unit.UID);
    }

    /// <summary>
    /// 获取所有插件触发器
    /// </summary>
    /// <returns></returns>
    private List<ModifyTriggerData> GetAllUnitModifierTriggerDatas()
    {
        List<ModifyTriggerData> result = new List<ModifyTriggerData>();
        for (int i = 0; i < AllShipUnits.Count; i++)
        {
            var triggerDatas = AllShipUnits[i].AllTriggerDatas;
            if (triggerDatas.Count > 0)
            {
                result.AddRange(triggerDatas);
            }
        }
        return result;
    }

    /// <summary>
    /// 重置所有插件触发器
    /// </summary>
    private void ResetAllUnitModifierTriggerDatas()
    {
        var allTriggers = GetAllUnitModifierTriggerDatas();
        allTriggers.ForEach(x => x.Reset());
    }

    /// <summary>
    /// 每秒船体值自动恢复
    /// </summary>
    private void OnUpdateUnitHPRecover()
    {
        var recoverHP = MainPropertyData.GetPropertyFinal(PropertyModifyKey.UnitHPRecoverValue);
        if (recoverHP <= 0)
            return;

        for(int i = 0; i < AllShipUnits.Count; i++)
        {
            var unit = AllShipUnits[i];
            if(unit.state == DamagableState.Normal && unit.HpComponent.HPPercent < 100)
            {
                unit.HpComponent.ChangeHP(Mathf.RoundToInt(recoverHP));
            }
        }
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

    public int GetPlugCountByItemRarity(GoodsItemRarity rarity)
    {
        return AllCurrentShipPlugs.FindAll(x => x.Rarity == rarity).Count;
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
    /// 获取相同插件总数
    /// </summary>
    /// <param name="plugID"></param>
    /// <returns></returns>
    public int GetSameShipPlugTotalCount(int plugID)
    {
        if (plugItemCountDic.ContainsKey(plugID))
            return plugItemCountDic[plugID];

        return 0;
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

        if (plugItemCountDic.ContainsKey(plugID))
        {
            plugItemCountDic[plugID]++;
        }
        else
        {
            plugItemCountDic.Add(plugID, 1);
        }

        var uid = ModifyUIDManager.Instance.GetUID(PropertyModifyCategory.ShipPlug, plugInfo);
        plugInfo.UID = uid;
        plugInfo.OnAdded();
        _currentShipPlugs.Add(uid, plugInfo);
        AllCurrentShipPlugs.Add(plugInfo);
        OnShipPlugCountChange?.Invoke(plugID, GetSameShipPlugTotalCount(plugID));
        OnItemCountChange?.Invoke();
        RogueEvent.Trigger(RogueEventType.ShipPlugChange);
    }

    private int GetGoodsIDByPlugID(int plugID)
    {
        foreach(var item in goodsItems.Values)
        {
            var cfg = item._cfg;
            if (cfg.TypeID == plugID)
                return item.GoodsID;
        }
        return 0;
    }

    /// <summary>
    /// 重置所有插件触发器
    /// </summary>
    private void ResetAllPlugModifierTriggerDatas()
    {
        var allTriggers = GetAllPlugModifierTriggerDatas();
        allTriggers.ForEach(x => x.Reset());
    }

    /// <summary>
    /// 获取所有插件触发器
    /// </summary>
    /// <returns></returns>
    private List<ModifyTriggerData> GetAllPlugModifierTriggerDatas()
    {
        List<ModifyTriggerData> result = new List<ModifyTriggerData>();
        for (int i = 0; i < AllCurrentShipPlugs.Count; i++)
        {
            var triggerDatas = AllCurrentShipPlugs[i].AllTriggerDatas;
            if(triggerDatas.Count > 0)
            {
                result.AddRange(triggerDatas);
            }
        }
        return result;
    }

    /// <summary>
    /// 初始化舰船插件
    /// </summary>
    private void InitShipPlugs()
    {
        var shipCfg = currentShipSelection.itemconfig as PlayerShipConfig;
        AddNewShipPlug(shipCfg.CorePlugID);
        if (shipCfg.ShipOriginPlugs != null && shipCfg.ShipOriginPlugs.Count > 0) 
        {
            for (int i = 0; i < shipCfg.ShipOriginPlugs.Count; i++) 
            {
                AddNewShipPlug(shipCfg.ShipOriginPlugs[i]);
            }
        }
    }

    #endregion

    #region Ship LevelUp

    public List<ShipLevelUpItem> CurrentShipLevelUpItems
    {
        get;
        private set;
    }

    /// <summary>
    /// 当前升级刷新花费
    /// </summary>
    public int CurrentLevelUpitemRerollCost
    {
        get;
        private set;
    }

    /// <summary>
    /// 当前升级刷新次数
    /// </summary>
    public int CurrentLevelUpItemRerollCount
    {
        get;
        private set;
    }

    public void ShipLevelUp(byte oldLevel, int levelUpCount = 1)
    {
        RefreshShipLevelUpItems(false);

        ///重复升级保护
        var levelUpPanel = UIManager.Instance.GetGUIFromDic<ShipLevelUpPage>("ShipLevelUpPage");
        if(levelUpPanel != null)
        {
            levelUpPanel.AddUpgradeLevelCount(levelUpCount);
            return;
        }

        IsShowingShipLevelUp = true;
        GameManager.Instance.PauseGame();
        ///DisplayUI
        UIManager.Instance.ShowUI<ShipLevelUpPage>("ShipLevelUpPage", E_UI_Layer.Top, RogueManager.Instance, (panel) =>
        {
            panel.Initialization(oldLevel, levelUpCount);
            panel.Initialization();
            InputDispatcher.Instance.ChangeInputMode("UI");
        });
    }

    /// <summary>
    /// 刷新选择
    /// </summary>
    public void RefreshShipLevelUpItems(bool isReroll)
    {
        if (isReroll)
        {
            ///Check Cost
            if (CurrentCurrency < CurrentLevelUpitemRerollCost)
                return;

            AddCurrency(-CurrentLevelUpitemRerollCost);
        }
        else
        {
            ///Reset RerollCount
            CurrentLevelUpItemRerollCount = 0;
        }

        CurrentLevelUpItemRerollCount++;
        var randomCount = DataManager.Instance.battleCfg.ShipLevelUp_GrowthItem_Count;
        CurrentShipLevelUpItems = GenerateUpgradeItem(randomCount);
        RefreshCurrentLevelUpRerollCost();
    }

    /// <summary>
    /// Select Item
    /// </summary>
    /// <param name="item"></param>
    public void SelectShipLevelUpItem(ShipLevelUpItem item)
    {
        ///AddProperty
        MainPropertyData.AddPropertyModifyValue(item.Config.ModifyKey, PropertyModifyType.Row, 0, item.GetModifyValue());
    }

    /// <summary>
    /// 初始化属性随机池
    /// </summary>
    private void InitShipLevelUpItems()
    {
        _allShipLevelUpItems = new List<ShipLevelUpItem>();
        var allItems = DataManager.Instance.battleCfg.ShipLevelUpGrowthItems;
        for (int i = 0; i < allItems.Count; i++) 
        {
            var rarityMap = allItems[i].RarityValueMap;
            for(int j = 0; j < rarityMap.Length; j++)
            {
                ShipLevelUpItem item = new ShipLevelUpItem(allItems[i], (GoodsItemRarity)j);
                item.Weight = 10;
                _allShipLevelUpItems.Add(item);
            }
        }
    }

    /// <summary>
    /// 随机生成当前升级项
    /// </summary>
    /// <returns></returns>
    private List<ShipLevelUpItem> GenerateUpgradeItem(int count)
    {
        List<ShipLevelUpItem> result = new List<ShipLevelUpItem>();

        var weightMap = RefreshShipLevelUpItemWeight();
        float totalWeight = 0f;
        foreach(var item in weightMap.Values)
        {
            totalWeight += item;
        }

        if (count <= 0)
            return result;

        ///Random
        for (int i = 0; i < count; i++) 
        {
            float randomResult = UnityEngine.Random.Range(0, totalWeight);
            GoodsItemRarity outRarity = GoodsItemRarity.Tier1;
            foreach (var rarityItem in weightMap)
            {
                if (rarityItem.Value>= randomResult)
                {
                    outRarity = rarityItem.Key;
                    break;
                }
            }

            ///Random Items
            var allRarityItems = _allShipLevelUpItems.FindAll(x => x.Rarity == outRarity);
            ///剔除相同属性
            for(int j = 0; j < result.Count; j++)
            {
                allRarityItems.Remove(result[j]);
            }

            var outItem = Utility.GetRandomList<ShipLevelUpItem>(allRarityItems, 1);
            if (outItem.Count == 1) 
            {
                result.Add(outItem[0]);
            }
        }
       
        return result;
    }

    /// <summary>
    /// 刷新当前升级项的权重
    /// </summary>
    private Dictionary<GoodsItemRarity, float> RefreshShipLevelUpItemWeight()
    {
        Dictionary<GoodsItemRarity, float> result = new Dictionary<GoodsItemRarity, float>();
        var luck = MainPropertyData.GetPropertyFinal(PropertyModifyKey.Luck);
        var currentShipLevel = GetCurrentShipLevel;

        foreach(GoodsItemRarity rarity in System.Enum.GetValues(typeof(GoodsItemRarity)))
        {
            if (rarity == GoodsItemRarity.Tier5 || rarity == GoodsItemRarity.Tier1)
                continue;

            var cfg = DataManager.Instance.battleCfg.GetShipLevelUpItemRarityConfigByRarity(rarity);
            if (cfg == null)
                continue;

            var weight = (Mathf.Max(currentShipLevel - cfg.MinLevel, 0) * cfg.WeightAddPerLevel + cfg.WeightBase) * (1 + luck / 100f);
            weight = Mathf.Min(weight, cfg.WeightMax);
            result.Add(rarity, weight);
        }

        ///Calculate Tier1
        float totalWeight = 0;
        foreach(var item in result.Values)
        {
            totalWeight += item;
        }

        result.Add(GoodsItemRarity.Tier1, Mathf.Max(0, 100 - totalWeight));

        return result;
    }

    /// <summary>
    /// 刷新当前升级刷新花费
    /// </summary>
    private void RefreshCurrentLevelUpRerollCost()
    {
        var refreshCfg = DataManager.Instance.gameMiscCfg.RefreshConfig;
        int waveCost = (GetCurrentWaveIndex - 1) * refreshCfg.LevelUpCostWaveMultiple;
        float refreshCountCst = (CurrentLevelUpItemRerollCount * refreshCfg.LevelUpCostWaveMultiple) / 100f;
        CurrentLevelUpitemRerollCost = Mathf.RoundToInt((refreshCfg.LevelUpRefreshCostBase + waveCost) * (1 + refreshCountCst));
    }

    #endregion


    #region Save

    private void CreateNewSaveData()
    {
        var newIndex = SaveLoadManager.Instance.GetSaveIndex();
        SaveData sav = new SaveData(newIndex);
        sav.SaveName = string.Format("NEW_SAVE_{0}", newIndex);
        sav.ModeID = CurrentHardLevel.HardLevelID;
        sav.GameTime = 0;
        SaveLoadManager.Save<SaveData>(sav, sav.SaveName);
        SaveLoadManager.Instance.AddSaveData(sav);
    }
    #endregion
}

public class BattleResultInfo
{
    public int Score;
    public bool Success = false;

    public CampLevelUpInfo campLevelInfo;
}