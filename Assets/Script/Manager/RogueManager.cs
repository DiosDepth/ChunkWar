using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using UnityEngine.Events;

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
    RefreshShopWeaponInfo,
    ShowUnitDetailPage,
    HideUnitDetailPage,
    HoverUnitDisplay,
    HideHoverUnitDisplay,
    HoverUnitUpgradeDisplay,
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

    private Dictionary<GoodsItemRarity, int> _inLevelDropItems;
    /// <summary>
    /// ��ȡ���ڲк�����
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
    /// �����̵���Ʒ
    /// </summary>
    private Dictionary<int, ShopGoodsInfo> goodsItems;
    private Dictionary<int, WreckageItemInfo> wreckageItems;

    private Dictionary<int, byte> _playerCurrentGoods;

    /// <summary>
    /// ��ǰ���������Ʒ
    /// </summary>
    private Dictionary<uint, ShipPlugInfo> _currentShipPlugs = new Dictionary<uint, ShipPlugInfo>();
    public List<ShipPlugInfo> AllCurrentShipPlugs = new List<ShipPlugInfo>();

    /// <summary>
    /// ��������ӳ�
    /// </summary>
    private List<PropertyModifySpecialData> globalModifySpecialDatas = new List<PropertyModifySpecialData>();

    /// <summary>
    /// ��ǰ�����������
    /// </summary>
    private Dictionary<uint, Unit> _currentShipUnits = new Dictionary<uint, Unit>();
    public List<Unit> AllShipUnits = new List<Unit>();

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
    private byte _shopRefreshTotalCount = 0;

    /// <summary>
    /// ��ǰ����̵���Ʒ
    /// </summary>
    public List<ShopGoodsInfo> CurrentRogueShopItems
    {
        get;
        private set;
    }

    /// <summary>
    /// ��ǰ����վ��ʱUnit
    /// </summary>
    public Dictionary<uint, WreckageItemInfo> CurrentWreckageItems
    {
        get;
        private set;
    }

    /// <summary>
    /// ��������ʱ���������
    /// </summary>
    private List<ShipLevelUpItem> _allShipLevelUpItems;

    #region Action 

    /* ���ذٷֱȱ仯 */
    public UnityAction<float> OnWreckageLoadPercentChange;
    /* ��Դ�ٷֱȱ仯 */
    public UnityAction<float> OnEnergyPercentChange;
    /* �к������仯 */
    public UnityAction<int> OnWasteCountChange;
    /* �к���������仯  ID : Count*/
    public UnityAction<int, int> OnShipPlugCountChange;
    /* ���α仯  bool : ���ο�ʼ�����*/
    public UnityAction<bool> OnWaveStateChange;

    #endregion

    public RogueManager()
    {
        Initialization();
    }

    /// <summary>
    /// ����ѡ������ʼ��
    /// </summary>
    public void InitShipSelection()
    {
        MainPropertyData = new UnitPropertyData();
        InitDefaultProperty();
    }

    /// <summary>
    /// ����ս��
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
    /// ����harbor
    /// </summary>
    public void OnEnterHarborInit()
    {
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
    /// ����ѡ�������ʱԤ������
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
    /// ����ս����ʼ��
    /// </summary>
    public void InitRogueBattle()
    {
        _inLevelDropItems = new Dictionary<GoodsItemRarity, int>
        {
            { GoodsItemRarity.Tier1, 2 },
            { GoodsItemRarity.Tier2, 1 },
            { GoodsItemRarity.Tier3, 4 },
            { GoodsItemRarity.Tier4, 1 },
        };
        MainPropertyData.Clear();
        _currentShipPlugs.Clear();
        AllCurrentShipPlugs.Clear();
        globalModifySpecialDatas.Clear();

        InitEnergyAndWreckageCommonBuff();
        InitWave();
        InitWreckageData();
        InitShopData();
        InitAllGoodsItems();
        AddNewShipPlug((currentShipSelection.itemconfig as PlayerShipConfig).CorePlugID);
    }

    /// <summary>
    /// �ؿ�ʤ��
    /// </summary>
    public void RogueBattleSuccess()
    {
        Timer.Pause();
        Timer.RemoveAllTrigger();
    }

    public void RogueBattleOver()
    {
        Timer.Pause();
        Timer.RemoveAllTrigger();
    }

    /// <summary>
    /// ��ͣ
    /// </summary>
    public void Pause()
    {
        Timer.Pause();
    }

    public void Resume()
    {
        Timer.StartTimer();
    }

    public override void Initialization()
    {
        base.Initialization();
        goodsItems = new Dictionary<int, ShopGoodsInfo>();
        _playerCurrentGoods = new Dictionary<int, byte>();
        InitShipLevelUpItems();
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
        GenerateShopGoods(3, _shopRefreshTotalCount);
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

        ///BindProperty Change
        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.DamagePercent, () => { RefreshWeaponItemInfo(UI_WeaponUnitPropertyType.Damage); });
        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.PhysicsDamage, () => { RefreshWeaponItemInfo(UI_WeaponUnitPropertyType.Damage); });
        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.EnergyDamage, () => { RefreshWeaponItemInfo(UI_WeaponUnitPropertyType.Damage); });
    }

    /// <summary>
    /// ������Ӫ����
    /// </summary>
    private void SettleCampScore()
    {
        var totalScore = CalculateCurrentWaveScore(true);
        var hardLevelRatio = CurrentHardLevel.Cfg.ScoreRatio;
        totalScore = Mathf.RoundToInt(totalScore * hardLevelRatio);
        var playerCampID = currentShip.playerShipCfg.PlayerShipCampID;
        var campData = GameManager.Instance.GetCampDataByID(playerCampID);
        if(campData != null)
        {
            campData.AddCampScore(totalScore);
        }
    }

    #region Drop

    /// <summary>
    /// �ܸ�������
    /// </summary>
    public float WreckageTotalLoadCost
    {
        get;
        private set;
    }

    /// <summary>
    /// �ܸ���ֵ
    /// </summary>
    public int WreckageTotalLoadValue
    {
        get;
        private set;
    }

    /// <summary>
    /// ���ذٷֱ�
    /// </summary>
    public float WreckageLoadPercent
    {
        get { return (WreckageTotalLoadValue * 100) / (float)WreckageTotalLoadCost; }
    }

    private ChangeValue<int> _dropWasteCount;

    /// <summary>
    /// �����к�����
    /// </summary>
    public int GetDropWasteCount
    {
        get { return _dropWasteCount.Value; }
    }

    /// <summary>
    /// �����к�����
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
    /// ���Ӿ��ڵ���
    /// </summary>
    /// <param name="rarity"></param>
    public void AddInLevelDrop(GoodsItemRarity rarity)
    {
        _inLevelDropItems[rarity]++;
        RogueEvent.Trigger(RogueEventType.WreckageDropRefresh);
        AchievementManager.Instance.Trigger<GoodsItemRarity>(AchievementWatcherType.WreckageGain, rarity);
    }

    /// <summary>
    /// ���۲к�
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
    /// ���ɾ��ڵ�����Ʒ
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
    /// ��ʼ������
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

        CalculateTotalLoadValue();
        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.ShipWreckageLoadTotal, CalculateTotalLoadValue);
        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.UnitLoadCost, CalculateTotalLoadCost);
        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.WasteLoadPercent, CalculateTotalLoadCost);
    }

    private List<WreckageItemInfo> GetAllWreckageItemsByRarity(GoodsItemRarity rarity)
    {
        return wreckageItems.Values.ToList().FindAll(x => x.Rarity == rarity);
    }

    /// <summary>
    /// �����ܸ���
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
        var loadAdd = MainPropertyData.GetPropertyFinal(PropertyModifyKey.ShipWreckageLoadTotal);
        WreckageTotalLoadValue = (int)Mathf.Clamp(loadAdd + loadRow, 0, int.MaxValue);
        OnWreckageLoadPercentChange?.Invoke(WreckageLoadPercent * 100);
    }

    #endregion

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
    private void CalculateHardLevelIndex(int paramInt = 0)
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
    /// ���ν���
    /// </summary>
    public async void OnWaveFinish()
    {
        ///Reset TriggerDatas
        ResetAllPlugModifierTriggerDatas();
        ResetAllUnitModifierTriggerDatas();
        if (IsFinalWave())
        {
            ///Level Success
            return;
        }
        LevelManager.Instance.CreateHarborPickUp();
        
        Timer.PauseAndSetZero();
        ///���޲��εȴ���
        OnWaveStateChange?.Invoke(false);
    }

    /// <summary>
    /// ���ο�ʼ
    /// </summary>
    public async void OnNewWaveStart()
    {
        _waveIndex++;
        _tempWaveTime = GetCurrentWaveTime();
        AddWaveTrigger();
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
        ///���Ӵ���ֵ�Զ��ָ�Trigger
        var recoverTrigger = LevelTimerTrigger.CreateTriger(0, 1, -1, "UnitHPRecover");
        recoverTrigger.BindChangeAction(OnUpdateUnitHPRecover);
        Timer.AddTrigger(recoverTrigger);
    }

    /// <summary>
    /// ���ɵ���������
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

    private void CreateFactory(int ID)
    {
        var waveCfg = CurrentHardLevel.GetWaveConfig(GetCurrentWaveIndex);
        if (waveCfg == null)
            return;

        var cfg = waveCfg.SpawnConfig.Find(x => x.ID == ID);

        Vector2 spawnpoint = MathExtensionTools.GetRadomPosFromOutRange(20f, 50f, RogueManager.Instance.currentShip.transform.position.ToVector2());
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.AIFactoryPath, true, (obj) =>
        {
            AIFactory aIFactory = obj.GetComponent<AIFactory>();
            aIFactory.PoolableSetActive(true);
            aIFactory.Initialization();
            RectAISpawnSetting spawnSetting = new RectAISpawnSetting((int)cfg.AIType, cfg.TotalCount, cfg.MaxRowCount);
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
    /// ����ؿ����յ÷�
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

        ///����Ƿ����һ���Ƿ�ʤ��
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
            ShipLevelUp(levelUpCount);
        }
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

    /// <summary>
    /// ��ʼ�����غ���Դ����BUFF
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
        if (info._cfg.ItemType == GoodsItemType.ShipUnit)
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
        GenerateShopGoods(refreshCount, _shopRefreshTotalCount);
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
        MainPropertyData.AddPropertyModifyValue(PropertyModifyKey.ShopRefreshCount, PropertyModifyType.Row, 0,DataManager.Instance.battleCfg.RogueShop_Origin_RefreshNum);
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

    public void AddNewShipUnit(Unit unit)
    {
        var uid = ModifyUIDManager.Instance.GetUID(PropertyModifyCategory.ShipUnit, unit);
        unit.UID = uid;
        unit.OnAdded();
        _currentShipUnits.Add(uid, unit);
        AllShipUnits.Add(unit);
    }

    public void RemoveShipUnit(Unit unit)
    {
        unit.OnRemove();
        AllShipUnits.Remove(unit);
        _currentShipUnits.Remove(unit.UID);
    }

    /// <summary>
    /// ����Unit
    /// </summary>
    /// <param name="targetUnit"></param>
    /// <param name="currentUnit"></param>
    public void TryEvolveUnit(Unit targetUnit, Unit currentUnit)
    {
        byte outRank = 0;
        var baseRarity = targetUnit._baseUnitConfig.GeneralConfig.Rarity;
        var targetRarity = GameHelper.GetTargetEvolveRarity(baseRarity, targetUnit.currentEvolvePoints, currentUnit._baseUnitConfig.GeneralConfig.Rarity, out outRank);

        ///û������
        if(baseRarity == targetRarity)
        {
            targetUnit.currentEvolvePoints = outRank;
        }
        else
        {
            ///Add New
            var unitGroupID = targetUnit._baseUnitConfig.UpgradeGroupID;
            var newUnitCfg = DataManager.Instance.GetUnitConfigByGroupAndRarity(unitGroupID, targetRarity);
            if(newUnitCfg == null)
            {
                Debug.LogError(string.Format("Find EvolveTarget Null!, GroupID = {0}, TargetRarity = {1}", unitGroupID, targetRarity));
                return;
            }

            currentShip.RemoveUnit(targetUnit);
            //currentShip.AddUnit(newUnitCfg,)
        }
    }

    /// <summary>
    /// ��ȡ���в��������
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
    /// �������в��������
    /// </summary>
    private void ResetAllUnitModifierTriggerDatas()
    {
        var allTriggers = GetAllUnitModifierTriggerDatas();
        allTriggers.ForEach(x => x.Reset());
    }

    /// <summary>
    /// ÿ�봬��ֵ�Զ��ָ�
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
    /// ��ȡ��ͬ�������
    /// </summary>
    /// <param name="plugID"></param>
    /// <returns></returns>
    public int GetSameShipPlugTotalCount(int plugID)
    {
        int result = 0;
        for(int i =0;i< AllCurrentShipPlugs.Count; i++)
        {
            if (AllCurrentShipPlugs[i].PlugID == plugID)
                result++;
        }
        return result;
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

        var uid = ModifyUIDManager.Instance.GetUID(PropertyModifyCategory.ShipPlug, plugInfo);
        plugInfo.UID = uid;
        plugInfo.OnAdded();
        _currentShipPlugs.Add(uid, plugInfo);
        AllCurrentShipPlugs.Add(plugInfo);
        OnShipPlugCountChange?.Invoke(plugID, GetSameShipPlugTotalCount(plugID));
        RogueEvent.Trigger(RogueEventType.ShipPlugChange);
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

    /// <summary>
    /// �������в��������
    /// </summary>
    private void ResetAllPlugModifierTriggerDatas()
    {
        var allTriggers = GetAllPlugModifierTriggerDatas();
        allTriggers.ForEach(x => x.Reset());
    }

    /// <summary>
    /// ��ȡ���в��������
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

    #endregion

    #region Ship LevelUp

    public List<ShipLevelUpItem> CurrentShipLevelUpItems
    {
        get;
        private set;
    }

    public int CurrentLevelUpitemRerollCost
    {
        get;
        private set;
    }

    public void ShipLevelUp(int levelUpCount = 1)
    {
        RefreshShipLevelUpItems(false);
        Pause();
        ///DisplayUI
        UIManager.Instance.ShowUI<ShipLevelUpPage>("ShipLevelUpPage", E_UI_Layer.Top, RogueManager.Instance, (panel) =>
        {
            panel.Initialization(levelUpCount);
            panel.Initialization();
            InputDispatcher.Instance.ChangeInputMode("UI");
        });
    }

    /// <summary>
    /// ˢ��ѡ��
    /// </summary>
    public void RefreshShipLevelUpItems(bool isReroll)
    {
        var randomCount = DataManager.Instance.battleCfg.ShipLevelUp_GrowthItem_Count;
        CurrentShipLevelUpItems = GenerateUpgradeItem(randomCount);
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
    /// ��ʼ�����������
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
    /// ������ɵ�ǰ������
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
            ///�޳���ͬ����
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
    /// ˢ�µ�ǰ�������Ȩ��
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
