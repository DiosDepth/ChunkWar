using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
#if GMDEBUG
using GM_Observer;
#endif

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
    LuckChange,
}

public class RogueManager : Singleton<RogueManager>, IPauseable
{
    /// <summary>
    /// ��Ҫ����
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
    /// ս�����
    /// </summary>
    public BattleResultInfo BattleResult
    {
        get;
        private set;
    }

    /// <summary>
    /// �Ƿ��ھ���
    /// </summary>
    public bool InBattle
    {
        get;
        private set;
    }

    /// <summary>
    /// �ڸۿ���
    /// </summary>
    public bool InHarbor
    {
        get;
        private set;
    }

    public bool InShop
    {
        get;
        private set;
    }

    /// <summary>
    /// �����Ƿ����
    /// </summary>
    public bool WaveEnd
    {
        get;
        private set;
    }

    /// <summary>
    /// �Ƿ���ʾ������������
    /// </summary>
    public bool IsShowingShipLevelUp = false;

    /// <summary>
    /// �����̵���Ʒ
    /// </summary>
    private Dictionary<int, ShopGoodsInfo> goodsItems;
    private Dictionary<int, WreckageItemInfo> wreckageItems;
    private Dictionary<int, int> plugItemCountDic;

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

    private List<int> _eliteSpawnWaves = new List<int>();
    private List<int> _alreadySpawnedEliteIDs = new List<int>();
    private List<int> _spawnBossTempLst = new List<int>();
    /// <summary>
    /// ���ڷ�ֹû��BOSSˢ����ֱ�ӹ��ص������temp fix
    /// </summary>
    private byte _totalKillBossCount = 0;
    private BattleSpecialEntitySpawnConfig _entitySpawnConfig;
    private List<ExtraSpawnInfo> _extraSpawnConfig = new List<ExtraSpawnInfo>();

    /// <summary>
    /// ��ǰˢ�´���
    /// </summary>
    private int _currentRereollCount = 0;

    /// <summary>
    /// ��ǰ����Զ�ŷɴ����������ڱ���
    /// </summary>
    private int _currentGenerateAncientUnitShipCount = 0;
    private int _currentAncientUnitProtectRateAdd;

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
    /// �̵����ˢ�´���
    /// </summary>
    private byte _shopFreeRollCount = 0;

    /// <summary>
    /// ��ǰ����̵���Ʒ
    /// </summary>
    public List<ShopGoodsInfo> CurrentRogueShopItems
    {
        get;
        private set;
    }

    /// <summary>
    /// ��ǰ����վ��ʱUnit0
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

    /// <summary>
    /// ���ν������Զ�����ۿ�
    /// </summary>
    private bool autoEnterHarbor = true;

#if GMDEBUG
    public BattleObserver GM_Observer;
#endif

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
    /* �̵�ˢ��  ˢ�´��� */
    public UnityAction<int> OnShopRefresh;
    /* ��Ʒ������� */
    public UnityAction OnItemCountChange;
    /* ����̫�ո� */
    public UnityAction OnEnterHarbor;
    /* �����̵� */
    public UnityAction OnEnterShop;
    /* �����̵���Ʒ */
    public UnityAction<int> OnBuyShopItem;
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
        OnEnterShop = null;
        OnBuyShopItem = null;
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
        _extraSpawnConfig.Clear();
        _eliteSpawnWaves.Clear();
        _alreadySpawnedEliteIDs.Clear();
        _spawnBossTempLst.Clear();

        _currentRereollCount = 0;
        _shopRefreshTotalCount = 0;
        _currentGenerateAncientUnitShipCount = 0;
        _currentAncientUnitProtectRateAdd = 0;
        _shopFreeRollCount = 0;
        _totalKillBossCount = 0;
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
    /// ����ս��
    /// </summary>
    public void OnUpdateBattle()
    {
        for (int i = 0; i < AllCurrentShipPlugs.Count; i++) 
        {
            AllCurrentShipPlugs[i].OnBattleUpdate();
        }
    }

    /// <summary>
    /// ����harbor
    /// </summary>
    public void OnEnterHarborInit()
    {
        InHarbor = true;
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

    public void OnExitHarbor()
    {
        InHarbor = false;
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
        BattleReset();

        ShipMapData = new ShipMapData((currentShipSelection.itemconfig as PlayerShipConfig).Map);

        InitDefaultProperty();
        InitWave();
        InitRandomEliteSpawn();
        InitWreckageData();
        InitShopData();
        InitAllGoodsItems();
        InitModifyPlugTagWeight();
        ///InitPlugs
        InitShipPlugs();
        InitOriginShipUnit();

#if GMDEBUG
        CreateBattleObserver();
#endif
    }

    public void PlayCurrentHardLevelBGM()
    {
        if (CurrentHardLevel == null)
            return;

        var bgmEvent = CurrentHardLevel.BGMEventName;
        SoundManager.Instance.PlayBGM(bgmEvent);
    }

    /// <summary>
    /// ���ùؿ�ʤ��
    /// </summary>
    public void SetBattleSuccess()
    {
        BattleResult.Success = true;
    }

    /// <summary>
    /// �ؿ�����
    /// </summary>
    public void RogueBattleOver()
    {
        GenerateBattleLog();
        currentShip?.controller.GameOver();
        Timer.Pause();
        Timer.RemoveAllTrigger();
        SettleCampScore();
    }

    private void BattleReset()
    {
        InBattle = true;
        MainPropertyData.Clear();
        _currentShipPlugs.Clear();
        AllCurrentShipPlugs.Clear();
        globalModifySpecialDatas.Clear();
        goodsItems.Clear();
        _extraSpawnConfig.Clear();
        MainPropertyData.BindRowPropertyChangeAction(PropertyModifyKey.ShopFreeRollCount, OnShopFreeRollCountChange);
        InitEnergyAndWreckageCommonBuff();
    }

    public override void Initialization()
    {
        base.Initialization();
        MainPropertyData = new UnitPropertyData();
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
    /// ����HardLevel
    /// </summary>
    /// <param name="info"></param>
    public void SetCurrentHardLevel(HardLevelInfo info)
    {
        if (info == null)
            return;

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
    /// ��ǰ�ؿ�ж�أ�һ��Ϊ����habor
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
            ECSManager.Instance.UnLoad();
            //(currentShip.controller as ShipController).shipUnitManager.Unload();
            GameObject.Destroy(currentShip.container.gameObject);
            currentShip = null;
        }
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
        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.Luck, OnPlayerLuckChange);
    }

    /// <summary>
    /// ������Ӫ����
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
    /// ������ʼ��Я���Ķ���Building
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

    public void SellAllWaste()
    {
        SellWaste(GetDropWasteCount);
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
    /// ����ǰ�ٷֱ���������
    /// </summary>
    /// <param name="percent"></param>
    public void AddDropWasteCountByPercent(float percent)
    {
        var targetpercent = Mathf.Max(0, percent / 100f);
        var targetWaste = Mathf.RoundToInt(GetDropWasteCount * (1 + targetpercent));
        _dropWasteCount.Set(targetWaste);
        OnWasteCountChange?.Invoke(targetWaste);
        ///UpdateLoad
        CalculateTotalLoadCost();
        RogueEvent.Trigger(RogueEventType.WasteCountChange);
    }

    /// <summary>
    /// ��ȡ�������вк�������
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
        ///Ȩ�ظ��ݽ�������
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
    /// ����ѡ�񽢴���������tagȨ�أ���ͬȨ��Ϊ�ӷ���ϵ
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

    
    #region Wave & HardLevel & Spawn

    private int _currentHardLevelIndex;
    /// <summary>
    /// ��ǰ�Ѷȵȼ�
    /// </summary>
    public int GetHardLevelValueIndex
    {
        get { return _currentHardLevelIndex; }
    }

    /// <summary>
    /// �Ƿ���ԺϷ�����
    /// </summary>
    /// <returns></returns>
    public bool IsLevelSpawnVaild()
    {
        bool result = true;
        result &= (InBattle && !InHarbor && !InShop && !WaveEnd);
        return result;
    }

    public SectorThreadSortType GetCurrentWaveSectorSortType()
    {
        var waveCfg = CurrentHardLevel.GetWaveConfig(GetCurrentWaveIndex);
        if (waveCfg == null)
            return SectorThreadSortType.Balance;

        return waveCfg.SectorSortType;
    }

    /// <summary>
    /// ����hardLevel�ȼ�
    /// </summary>
    /// <returns></returns>
    private void CalculateHardLevelIndex(int paramInt = 0)
    {
        var currentSecond = Timer.TotalSeconds;
        var secondsLevel = Mathf.RoundToInt(currentSecond / GameGlobalConfig.HardLevelMultiple_TimeDelta);

        var multiple = GameGlobalConfig.HardLevelWaveIndexMultiple;
        _currentHardLevelIndex = multiple * (GetCurrentWaveIndex) + secondsLevel;
        Debug.Log("Update HardLevel , HardLevel = " + _currentHardLevelIndex);
    }

    private void InitWave(int waveIndex = 1)
    {
        _waveIndex = waveIndex;
        _tempWaveTime = GetCurrentWaveTime();
        Timer = new LevelTimer();
        var totalTime = GetTempWaveTime;
        Timer.InitTimer(totalTime);

        var hardLevelDelta = DataManager.Instance.battleCfg.HardLevelDeltaSeconds;
        var trigger = LevelTimerTrigger.CreateTrigger(0, hardLevelDelta, -1, -1, "HardLevelUpdate");
        trigger.BindChangeAction(CalculateHardLevelIndex);
        Timer.AddTrigger(trigger);
        CalculateHardLevelIndex();
        AddWaveTrigger();

        if (IsFinalWave())
        {
            LevelManager.Instance.Action_OnShipDie += CheckLevelWinByBoss;
        }

        SoundManager.Instance.PlayUISound("Wave_Start", 0.5f);
    }

    /// <summary>
    /// �����Ӣ���ɷֲ�
    /// </summary>
    private void InitRandomEliteSpawn()
    {
        if (!CurrentHardLevel.SpawnConfig.EnableSpecialEnemySpawn)
            return;

        ///Elite Spawn
        var spawnCount = CurrentHardLevel.SpawnConfig.TotalSpawnWaveCount;
        var spawnCfg = CurrentHardLevel.SpawnConfig.EliteRandomSpawnCfg;

        List<int> waveIndexs = spawnCfg.Select(x => x.WaveIndex).ToList();
        System.Random rand = new System.Random();
        var targetWaves = waveIndexs.OrderBy(index => rand.Next()).Take(spawnCount).ToList();
        _eliteSpawnWaves = targetWaves;
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
        WaveEnd = true;
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

        _extraSpawnConfig.Clear();
        Timer.PauseAndSetZero();
        ///ֹͣ����ƶ�
        currentShip.controller.SetControllerUpdate(false);
        ECSManager.Instance.SetProcessECS(false);
        LeanTween.cancelAll();
        ECSManager.Instance.GameOverProjectile();
        LevelManager.Instance.GameOver();
        await UniTask.Delay(500);
        
        LevelManager.Instance.CollectAllPickUps();
        await UniTask.Delay(1000);
        await UniTask.WaitUntil(() => !IsShowingShipLevelUp);
        SoundManager.Instance.PlayUISound("Wave_Finish");
        LevelManager.Instance.DoAllDespawn();

        await UniTask.Delay(3000);
        ECSManager.Instance.GameOverAgent();
        ECSManager.Instance.UnLoad();
        _waveIndex++;
        if (autoEnterHarbor)
        {
            GameStateTransitionEvent.Trigger(EGameState.EGameState_GameHarbor);
        }

        ///���޲��εȴ���
        OnWaveStateChange?.Invoke(false);
    }

    /// <summary>
    /// ���ο�ʼ
    /// </summary>
    public async void OnNewWaveStart()
    {
        WaveEnd = false;
        _currentGenerateAncientUnitShipCount = 0;
        _currentAncientUnitProtectRateAdd = 0;
        LevelManager.Instance.SpawnSector.RefreshAndClear();

        _tempWaveTime = GetCurrentWaveTime();
        AddWaveTrigger();
        CalculateHardLevelIndex();
        Timer.InitTimer(_tempWaveTime);
        Timer.StartTimer();
        ECSManager.Instance.SetProcessECS(true);
        OnWaveStateChange?.Invoke(true);

        SoundManager.Instance.PlayUISound("Wave_Start", 0.5f);
    }

    private bool IsFinalWave()
    {
        var waveCount = CurrentHardLevel.WaveCount;
        return GetCurrentWaveIndex >= waveCount;
    }

    /// <summary>
    /// �жϹؿ���ɸ���BOSS״̬
    /// </summary>
    private void CheckLevelWinByBoss(BaseShip targetShip, UnitDeathInfo info)
    {
        if (_spawnBossTempLst.Contains(targetShip.ShipID))
        {
            _spawnBossTempLst.Remove(targetShip.ShipID);
            _totalKillBossCount++;
        }

        ///Temp Fix ��ֹû�л�ɱBOSSֱ�ӹ���
        if (_spawnBossTempLst.Count <= 0 && _totalKillBossCount > 0)
        {
            ///SettleWin
            SetBattleSuccess();
            GameEvent.Trigger(EGameState.EGameState_GameOver);
        }
    }

    private void AddWaveTrigger()
    {
        Timer.RemoveAllTrigger();
        GenerateEnemyAIFactory();
        GenerateShopCreateTimer();
        GenerateAISpawnSectorTrigger();
        ///���Ӵ���ֵ�Զ��ָ�Trigger
        var recoverTrigger = LevelTimerTrigger.CreateTrigger(0, 1, -1, -1, "UnitHPRecover");
        recoverTrigger.BindChangeAction(OnUpdateUnitHPRecover);
        Timer.AddTrigger(recoverTrigger);
    }

    /// <summary>
    /// AI��������������
    /// </summary>
    private void GenerateAISpawnSectorTrigger()
    {
        ///ˢ������ǿ�Ȼ���
        var threadTrigger = LevelTimerTrigger.CreateTrigger(0, GameGlobalConfig.SectorThread_UpdateSecond, -1, -1, "SectorThreadUpdate");
        threadTrigger.BindChangeAction(UpdateSectorThread);
        Timer.AddTrigger(threadTrigger);

        ///���θ��������������Ȩ��
        var waveCfg = CurrentHardLevel.GetWaveConfig(GetCurrentWaveIndex);
        if (waveCfg == null)
            return;

        if (waveCfg.SectorAdjacentUpdateTime == null || waveCfg.SectorAdjacentWeightValue == null)
            return;

        if(waveCfg.SectorAdjacentWeightValue.Length != waveCfg.SectorAdjacentUpdateTime.Length)
        {
            Debug.LogError("�����������ݳ��Ȳ�ƥ�䣡WaveIndex = " + GetCurrentWaveIndex);
            return;
        }

        for (int i = 0; i < waveCfg.SectorAdjacentUpdateTime.Length; i++)  
        {
            var trigger = LevelTimerTrigger.CreateTrigger(waveCfg.SectorAdjacentUpdateTime[i], 0, 1);
            trigger.BindChangeAction(UpdateSectorAdjacentWeight, waveCfg.SectorAdjacentWeightValue[i]);
            Timer.AddTrigger(trigger);
        }
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
            var trigger = LevelTimerTrigger.CreateTrigger(cfg.StartTime, cfg.DurationDelta, cfg.LoopCount);
            trigger.BindChangeAction(CreateFactory, cfg.ID);
            Timer.AddTrigger(trigger);
        }
        ///������Ӣ������
        GenerateEliteSpawnTrigger();

        ///���Ϊ���һ��������BOSS
        if (IsFinalWave())
        {
            _spawnBossTempLst.Clear();
            GenerateBossSpawnTrigger();
        }

        var currentWaveTime = GetCurrentWaveTime();

        ///Generate Meteorite Common
        var MeteoriteTrigger1 = LevelTimerTrigger.CreateTrigger(_entitySpawnConfig.MeteoriteGenerate_Timer1, _entitySpawnConfig.MeteoriteGenerate_Timer1, -1, currentWaveTime - _entitySpawnConfig.MeteoriteGenerate_EndTime,  "MeteoriteGenerate_1");
        MeteoriteTrigger1.BindChangeAction(CreateMeteorite_Common);
        Timer.AddTrigger(MeteoriteTrigger1);

        ///Generate Meteorite Smooth
        var MeteoriteTrigger2 = LevelTimerTrigger.CreateTrigger(_entitySpawnConfig.MeteoriteGenerate_Timer2, _entitySpawnConfig.MeteoriteGenerate_Timer2, -1, currentWaveTime - _entitySpawnConfig.MeteoriteGenerate_EndTime,  "MeteoriteGenerate_2");
        MeteoriteTrigger2.BindChangeAction(CreateMeteorite_Smooth);
        Timer.AddTrigger(MeteoriteTrigger2);

        ///Generate Ancient Common
        var ancientTrigger1 = LevelTimerTrigger.CreateTrigger(_entitySpawnConfig.AncientUnitGenerate_Timer1, _entitySpawnConfig.AncientUnitGenerate_Timer1, -1, currentWaveTime - _entitySpawnConfig.AncientUnitGenerate_EndTime, "ancientGenerate_1");
        ancientTrigger1.BindChangeAction(CreateAncientUnitShip);
        Timer.AddTrigger(ancientTrigger1);

        int ancientSmoothDeltaTime = 0;
        if (waveCfg.OverrideAncientUnitSpawnSoomthTimeDelta)
        {
            ancientSmoothDeltaTime = waveCfg.OverrideSpawnTime;
        }
        else
        {
            ancientSmoothDeltaTime = _entitySpawnConfig.AncientUnitGenerate_Timer2;
        }

        ///Generate Ancient Smooth
        var ancientTrigger2 = LevelTimerTrigger.CreateTrigger(ancientSmoothDeltaTime, ancientSmoothDeltaTime, -1, currentWaveTime - _entitySpawnConfig.AncientUnitGenerate_EndTime, "ancientGenerate_2");
        ancientTrigger2.BindChangeAction(CreateAncientUnitShip_Smooth);
        Timer.AddTrigger(ancientTrigger2);
    }

    private void GenerateEliteSpawnTrigger()
    {
        if (!_eliteSpawnWaves.Contains(GetCurrentWaveIndex))
            return;

        var eliteSpawnCfg = CurrentHardLevel.SpawnConfig.GetEliteSpawnConfig(GetCurrentWaveIndex);
        if (eliteSpawnCfg == null)
            return;

        var spawnLst = eliteSpawnCfg.EliteSpawnTimeList;
        if (spawnLst != null && spawnLst.Length > 0) 
        {
            ///Create Timer
            for (int i = 0; i < spawnLst.Length; i++)
            {
                var trigger = LevelTimerTrigger.CreateTrigger(spawnLst[i], 0, 1, -1);
                trigger.BindChangeAction(CreateElite);
                Timer.AddTrigger(trigger);
            }
        }
    }

    /// <summary>
    /// ����BOSS������
    /// </summary>
    private void GenerateBossSpawnTrigger()
    {
        var spawnCfg = CurrentHardLevel.SpawnConfig.BossConfig;
        System.Random ran = new System.Random();
        if (spawnCfg.RandomOne)
        {
            ///Generate One
            var allBoss = DataManager.Instance.GetAllBossIDs();
            if (allBoss.Count <= 0)
                return;

            var outID = allBoss[ran.Next(0, allBoss.Count)];
            var trigger = LevelTimerTrigger.CreateTrigger(spawnCfg.OneSpawnStartTime, 0, 1, -1);
            trigger.BindChangeAction(CreateBoss, outID);
            Timer.AddTrigger(trigger);
            _spawnBossTempLst.Add(outID);
        }
        else
        {
            if (spawnCfg.BOSSSpawnIDs == null || spawnCfg.BOSSSpawnTimeList == null)
                return;

            if(spawnCfg.BOSSSpawnIDs.Length != spawnCfg.BOSSSpawnTimeList.Length)
            {
                Debug.LogError("BOSS����������ƥ�䣡");
                return;
            }

            for(int i = 0; i < spawnCfg.BOSSSpawnIDs.Length; i++)
            {
                var id = spawnCfg.BOSSSpawnIDs[i];
                var time = spawnCfg.BOSSSpawnTimeList[i];
                _spawnBossTempLst.Add(id);
                var trigger = LevelTimerTrigger.CreateTrigger(time, 0, 1, -1);
                trigger.BindChangeAction(CreateBoss, id);
                Timer.AddTrigger(trigger);
            }
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
            var trigger = LevelTimerTrigger.CreateTrigger(time, 0, 1);
            trigger.BindChangeAction(CreateShopTeleport);
            Timer.AddTrigger(trigger);
        }
    }

    /// <summary>
    /// Զ�����˻�����
    /// </summary>
    private void CreateAncientUnitShip()
    {
        var count = MainPropertyData.GetPropertyFinal(PropertyModifyKey.AncientUnit_Generate_Count) + 1;
        count = Mathf.Clamp(count, 1, GameGlobalConfig.AncientUnit_Generate_MaxCount);
        float targetCount = _entitySpawnConfig.AncientUnitGenerate_Rate1;

        while(targetCount >= 0)
        {
            if(targetCount < 1)
            {
                bool generate = Utility.CalculateRate100(targetCount * 100);
                if (generate)
                {
                    GenerateAncientUnit();
                }
            }

            GenerateAncientUnit();
            targetCount--;
        }
    }

    private void CreateAncientUnitShip_Smooth()
    {
        var count = MainPropertyData.GetPropertyFinal(PropertyModifyKey.AncientUnit_Generate_Count) + 1;
        count = Mathf.Clamp(count, 1, GameGlobalConfig.AncientUnit_Generate_MaxCount);

        var rate = count / _entitySpawnConfig.AncientUnitGenerate_Rate2;
        bool generate = Utility.CalculateRate100(rate * 100);

        var waveCfg = CurrentHardLevel.GetWaveConfig(GetCurrentWaveIndex);

        ///���׻���
        if (!generate && waveCfg != null && waveCfg.UseAncientUnitSpawnProtect)
        {
            if(_currentGenerateAncientUnitShipCount < waveCfg.AncientUnitSpawnProtectedCount)
            {
                ///���������С����������������һ�α��ף�ÿ����һ�θ���++
                _currentAncientUnitProtectRateAdd += DataManager.Instance.battleCfg.EntitySpawnConfig.AncientUnit_ProtectRate_Add;
                var newRate = rate * 100 + _currentAncientUnitProtectRateAdd;
                generate = Utility.CalculateRate100(newRate);
            }
        }

        if (generate)
        {
            GenerateAncientUnit();
        }
    }

    /// <summary>
    /// ��ʯ������
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
    /// ������ʯ
    /// </summary>
    private void GenerateMeteorite()
    {
        ///Calculate MeteoriteGenerate Tier
        var waveCfg = CurrentHardLevel.GetWaveConfig(GetCurrentWaveIndex);
        if (waveCfg == null)
            return;

        ///�����ʯϡ�ж�
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
            MaxRowCount = 1,
        };
        SpawnEntity(cfg);
    }

    /// <summary>
    /// ����Զ�ŵ�λ
    /// </summary>
    private void GenerateAncientUnit()
    {
        _currentAncientUnitProtectRateAdd = 0;
        var waveCfg = CurrentHardLevel.GetWaveConfig(GetCurrentWaveIndex);
        if (waveCfg == null)
            return;

        ///��������������ֵ������ͣ����
        if (_currentGenerateAncientUnitShipCount >= waveCfg.AncientUnitSpawnMax)
            return;

        _currentGenerateAncientUnitShipCount++;
        WaveEnemySpawnConfig cfg = new WaveEnemySpawnConfig
        {
            AITypeID = _entitySpawnConfig.AncientUnit_ShipID,
            DurationDelta = 0,
            LoopCount = 1,
            TotalCount = 1,
            MaxRowCount = 1,
        };
        SpawnEntity(cfg);
    }

    /// <summary>
    /// ������Ӣ
    /// </summary>
    private void CreateElite()
    {
        ///�ظ��Ż�
        var allEliteLst = DataManager.Instance.GetAllAIEliteIDs();
        var vaildList = allEliteLst.FindAll(x => !_alreadySpawnedEliteIDs.Contains(x));
        int outID = -1;

        System.Random ran = new System.Random();
        if(vaildList.Count <= 0)
        {
            ///����һ��
            _alreadySpawnedEliteIDs.Clear();
            outID = allEliteLst[ran.Next(0, allEliteLst.Count)];
            _alreadySpawnedEliteIDs.Add(outID);
        }
        else
        {
            outID = vaildList[ran.Next(0, vaildList.Count)];
            _alreadySpawnedEliteIDs.Add(outID);
        }

        if (outID == -1)
            return;

        ///Create Elite
        WaveEnemySpawnConfig tempCfg = new WaveEnemySpawnConfig
        {
            AITypeID = outID,
            DurationDelta = 0,
            LoopCount = 1,
            TotalCount = 1,
            MaxRowCount = 1,
        };

        SpawnEntity(tempCfg);
    }

    private void CreateBoss(int id)
    {
        ///Create Boss
        WaveEnemySpawnConfig tempCfg = new WaveEnemySpawnConfig
        {
            AITypeID = id,
            DurationDelta = 0,
            LoopCount = 1,
            TotalCount = 1,
            MaxRowCount = 1,
        };
        SpawnEntity(tempCfg);
    }

    private void CreateFactory(int ID)
    {
        var waveCfg = CurrentHardLevel.GetWaveConfig(GetCurrentWaveIndex);
        if (waveCfg == null)
            return;

        var cfg = waveCfg.SpawnConfig.Find(x => x.ID == ID);
        SpawnEntity(cfg);
    }

    private void CreateExtraFactory(int ID)
    {
        var info = _extraSpawnConfig.FindAll(x => x.ID == ID).FirstOrDefault();
        if (info == null)
            return;

        ///GetRandomNode
        if(info.ownerShip != null && info.PointCfg != null)
        {
            var pointCfg = info.PointCfg;

            Transform attachPoint = null;
            if (pointCfg.RandomNode)
            {
                attachPoint = info.ownerShip.GetRandomAttachPoint();
            }
            else
            {
                attachPoint = info.ownerShip.GetAttachPoint(pointCfg.NodeName);
            }

            SpawnEntity(info.Cfg, -1, attachPoint);
        }
        else
        {
            SpawnEntity(info.Cfg, -1);
        }
    }

    /// <summary>
    /// ���Ӷ���AI������
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="key"></param>
    public void AddExtraAIFactory(List<WaveEnemySpawnConfig> spawns, string key, AISkillShip owner = null, AttachPointConfig pointCfg = null)
    {
        for (int i = 0; i < spawns.Count; i++)
        {
            var cfg = spawns[i];
            var trigger = LevelTimerTrigger.CreateTrigger(cfg.StartTime, cfg.DurationDelta, cfg.LoopCount, -1, key);
            trigger.BindChangeAction(CreateExtraFactory, cfg.ID);
            Timer.AddTrigger(trigger);
            ExtraSpawnInfo info = new ExtraSpawnInfo
            {
                Cfg = cfg,
                ID = cfg.ID,
                ownerShip = owner,
                PointCfg = pointCfg
            };

            _extraSpawnConfig.Add(info);
        }
    }

    /// <summary>
    /// �Ƴ�����AI������
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="key"></param>
    public void RemoveExtraAIFactory(List<WaveEnemySpawnConfig> spawns, string key)
    {
        for (int i = 0; i < spawns.Count; i++) 
        {
            RemoveExtraSpawnData(spawns[i].ID);
        }
        Timer.RemoveTriggersByKey(key);
    }

    public void SpawnEntity(WaveEnemySpawnConfig cfg, int overrideHardLevelID = -1, Transform overrideSpawnPoint = null)
    {
        if (cfg == null || currentShip == null)
            return;

        Vector2 spawnpoint = Vector2.zero;
        if (overrideSpawnPoint != null)
        {
            spawnpoint = overrideSpawnPoint.position;
        }
        else
        {
            if (cfg.OverrideDistanceMax)
            {
                spawnpoint = LevelManager.Instance.SpawnSector.GetAIShipSpawnPosition(cfg.DistanceMax);
            }
            else
            {
                spawnpoint = LevelManager.Instance.SpawnSector.GetAIShipSpawnPosition();
            }
        }

        if(spawnpoint == Vector2.positiveInfinity)
        {
            Debug.LogError("�������������");
            return;
        }

        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.AIShipSpawnAgentPath, true, (obj) =>
        {
            AIShipSpawnAgent aIFactory = obj.GetComponent<AIShipSpawnAgent>();
            aIFactory.PoolableSetActive(true);
            aIFactory.Initialization();
            aIFactory.transform.position = spawnpoint;

            RectSpawnSetting spawnSetting = new RectSpawnSetting(cfg.TotalCount, cfg.MaxRowCount);
            spawnSetting.spawnIntervalTime = cfg.SpawnIntervalTime;
            spawnSetting.sizeInterval = new Vector2(cfg.SpawnSizeInterval, cfg.SpawnSizeInterval);

            AIShipSpawnInfo aishipspawninfo = new AIShipSpawnInfo(cfg.AITypeID, spawnpoint, RogueManager.Instance.currentShip.transform, overrideHardLevelID, spawnSetting, (baseship) => 
            {
                ECSManager.Instance.RegisterJobData(OwnerType.AI, baseship);
            });
            aIFactory.StartSpawn(aishipspawninfo);
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
 
    private void RemoveExtraSpawnData(int index)
    {
        for(int i = _extraSpawnConfig.Count - 1; i >= 0; i--)
        {
            if (_extraSpawnConfig[i].ID == index)
                _extraSpawnConfig.RemoveAt(i);
        }
    }

    /// <summary>
    /// �����������Ȩ��
    /// </summary>
    /// <param name="weight"></param>
    private void UpdateSectorAdjacentWeight(int weight)
    {
        LevelManager.Instance.SpawnSector.SetGlobalAdjacentSectorWeight(weight);
    }

    private void UpdateSectorThread()
    {
        LevelManager.Instance.SpawnSector.RefreshSectorThreadCache();
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

    private void OnPlayerLuckChange()
    {
        ShipPropertyEvent.Trigger(ShipPropertyEventType.LuckChange);
    }

    #endregion

    #region Shop

    /// <summary>
    /// �����̵�
    /// </summary>
    public void EnterShop()
    {
        InShop = true;
        GameManager.Instance.PauseGame();
        RefreshFreeRollCount();
        RefreshShop(false);
        OnEnterShop?.Invoke();

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
    /// �뿪�̵�
    /// </summary>
    public void ExitShop()
    {
        InShop = false;
        GameManager.Instance.UnPauseGame();
        InputDispatcher.Instance.ChangeInputMode("Player");
        CameraManager.Instance.SetFollowPlayerShip();
        CameraManager.Instance.SetOrthographicSize(GameGlobalConfig.CameraDefault_OrthographicSize);
        UIManager.Instance.HiddenUI("ShopHUD");
        UIManager.Instance.ShowUI<ShipHUD>("ShipHUD", E_UI_Layer.Mid, this, (panel) =>
        {
            panel.Initialization();
        });
        ///��������޵�ʱ��
        var time = DataManager.Instance.battleCfg.ShopExit_Player_Immortal_Time;
        if(currentShip != null)
        {
            currentShip.ForeceSetAllUnitState(DamagableState.Immortal, time);
        }
    }

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

        var cost = info.Cost;
        AddCurrency(-cost);
        info.OnItemSold();
        GainShopItem(info);

        OnBuyShopItem?.Invoke(info.GoodsID);

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
        if (info.IsWreckage)
        {
            AddInLevelDrop(info.Rarity);
        }
        else
        {
            AddNewShipPlug(info._cfg.TypeID, info.GoodsID);
        }
    }
    
    /// <summary>
    /// ˢ���̵�
    /// </summary>
    /// <returns></returns>
    public bool RefreshShop(bool useCurrency)
    {
        if (useCurrency)
        {
            if(_shopFreeRollCount > 0)
            {
                _shopFreeRollCount--;
                CurrentRerollCost = 0;
            }
            else
            {
                if (CurrentRerollCost > CurrentCurrency)
                    return false;

                ///Cost
                CurrentRerollCost = GetCurrentRefreshCost();
                AddCurrency(-CurrentRerollCost);
            }
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
        ///ȥ���Ѿ���������Ʒ
        refreshCount = (byte)Mathf.Max(0, refreshCount - itemLockCount);
        GenerateShopGoods(refreshCount, _shopRefreshTotalCount);

        OnShopRefresh?.Invoke(_currentRereollCount);
        RogueEvent.Trigger(RogueEventType.ShopReroll);
        Debug.Log("ˢ���̵꣬ˢ�´��� = " + _currentRereollCount);
        return true;
    }

    /// <summary>
    /// ��ȡ��ǰӵ�еĲ������
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
    /// �����̵���Ʒ
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
        var tier1Rate = 1000 - tier2Rate - tier3Rate - tier4Rate;
        
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
                ///�������������,���б���ȥ��
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
            ///����������Ʒ���仯
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
    /// ��ȡ���X����ǰ�̵����Ʒ
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public List<ShopGoodsInfo> GetRandomCurrentRogueShopItems(int count)
    {
        if (CurrentRogueShopItems.Count <= count)
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
    /// ����ѡ�񽢴���������tagȨ�أ���ͬȨ��Ϊ�ӷ���ϵ
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

        var rarityCfg = rarityMap[rarity];
        if (enterCount < rarityCfg.MinAppearEneterCount)
            return 0;

        ///LuckModify
        float luckRate = 1;
        if(enterCount >= rarityCfg.LuckModifyMinEnterCount)
        {
            var playerLuck = MainPropertyData.GetPropertyFinal(PropertyModifyKey.Luck);
            luckRate = (1 + playerLuck / 100f);
        }

        var result = rarityCfg.WeightAddPerEnterCount * enterCount + rarityCfg.BaseWeight * luckRate;
        result = Mathf.Clamp(result, 0, rarityCfg.WeightMax);
        return Mathf.RoundToInt(result * 10);
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

        ///Init Wreckage Shop Item
        var wreckageItems = DataManager.Instance.shopCfg.WreckageShopBuyItemCfg;
        if(wreckageItems != null && wreckageItems.Count > 0)
        {
            for(int i = 0; i < wreckageItems.Count; i++)
            {
                if (!goodsItems.ContainsKey(wreckageItems[i].GetGoodsID()))
                {
                    var goodsInfo = ShopGoodsInfo.CreateWreckageGoods(wreckageItems[i]);
                    goodsItems.Add(goodsInfo.GoodsID, goodsInfo);
                }
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
        return (byte)Mathf.Clamp(refreshCount, 1, GameGlobalConfig.ShopGoods_MaxCount);
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
    /// ���ˢ�´���
    /// </summary>
    private void RefreshFreeRollCount()
    {
        var rollCount = MainPropertyData.GetPropertyFinal(PropertyModifyKey.ShopFreeRollCount);
        _shopFreeRollCount = (byte)Mathf.Clamp(rollCount, 0, byte.MaxValue);
    }

    private void OnShopFreeRollCountChange(float oldValue, float newValue)
    {
        float delta = newValue - oldValue;
        if(delta > 0)
        {
            byte targey = (byte)Mathf.Clamp(_shopFreeRollCount + delta, 0, byte.MaxValue);
            _shopFreeRollCount = targey;
        }
    }

    /// <summary>
    /// �̵껨��ˢ��
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

        ///�������޷�����
        if (unit._baseUnitConfig.unitType == UnitType.MainWeapons || unit._baseUnitConfig.unitType == UnitType.MainBuilding) 
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
            if(unit.damageState == DamagableState.Normal && unit.HpComponent.HPPercent < 100)
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
        if (plugItemCountDic.ContainsKey(plugID))
            return plugItemCountDic[plugID];

        return 0;
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

    /// <summary>
    /// ��ʼ���������
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
    /// ��ǰ����ˢ�»���
    /// </summary>
    public int CurrentLevelUpitemRerollCost
    {
        get;
        private set;
    }

    /// <summary>
    /// ��ǰ����ˢ�´���
    /// </summary>
    public int CurrentLevelUpItemRerollCount
    {
        get;
        private set;
    }

    public void ShipLevelUp(byte oldLevel, int levelUpCount = 1)
    {
        RefreshShipLevelUpItems(false);

        ///�ظ���������
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
    /// ˢ��ѡ��
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

    /// <summary>
    /// ˢ�µ�ǰ����ˢ�»���
    /// </summary>
    private void RefreshCurrentLevelUpRerollCost()
    {
        var refreshCfg = DataManager.Instance.gameMiscCfg.RefreshConfig;
        float waveCost = (GetCurrentWaveIndex - 1) * refreshCfg.LevelUpCostWaveMultiple;
        float refreshCountCst = (CurrentLevelUpItemRerollCount * refreshCfg.LevelUpCostWaveMultiple) / 100f;
        CurrentLevelUpitemRerollCost = Mathf.RoundToInt((refreshCfg.LevelUpRefreshCostBase + waveCost) * (1 + refreshCountCst));
    }

    #endregion

    #region Save

    /// <summary>
    /// ��ȡ�浵
    /// </summary>
    /// <param name="sav"></param>
    public bool LoadSaveData(SaveData sav)
    {
        InBattle = true;
        bool result = true;
        BattleReset();

        ///Init HardLevel
        var hardLevelData = GameManager.Instance.GetHardLevelInfoByID(sav.ModeID);
        SetCurrentHardLevel(hardLevelData);

        result &= LoadShipBaseData(sav.ShipID, sav.MainWeaponID);

        if(sav.WaveIndex <= 0 || sav.WaveIndex > hardLevelData.WaveCount)
        {
            Debug.LogError("�浵���δ��� ����ID =" + sav.WaveIndex);
            result &= false;
        }
        ///Check Load Node
        if (!result)
            return result;

        _eliteSpawnWaves = sav.EliteSpawnWaves;
        _alreadySpawnedEliteIDs = sav.AlreadySpawnedEliteIDs;

        ///InitMap
        ShipMapData = new ShipMapData((currentShipSelection.itemconfig as PlayerShipConfig).Map);
        LoadPropertySave(sav);
        ///�ָ���������
        InitWave(sav.WaveIndex);

        LoadPlugRuntimeSaves(sav.PlugRuntimeSaves);
        LoadWreckageDataSave(sav.WasteCount);

        ///EnterBattle
        PlayCurrentHardLevelBGM();

        GameStateTransitionEvent.Trigger(EGameState.EGameState_GamePrepare);

#if GMDEBUG
        CreateBattleObserver();
#endif

        return true;
    }

    public void CreateInLevelSaveData(string saveName = "", bool formatJson = false)
    {
        var newIndex = SaveLoadManager.Instance.GetSaveIndex();
        if (string.IsNullOrEmpty(saveName))
        {
            saveName =  string.Format("NEW_SAVE_{0}", newIndex);
        }

        _CreateNewSaveData(newIndex, saveName, formatJson);
    }

    private void _CreateNewSaveData(int index, string saveName, bool formatJson = false)
    {
        SaveData sav = new SaveData(index);
        sav.SaveName = saveName;
        sav.ModeID = CurrentHardLevel.HardLevelID;
        sav.ShipID = currentShipSelection.itemconfig.ID;
        sav.MainWeaponID = currentWeaponSelection.itemconfig.ID;
        sav.GameTime = 0;
        sav.WaveIndex = GetCurrentWaveIndex;
        sav.WasteCount = GetDropWasteCount;
        sav.Currency = CurrentCurrency;
        sav.ShipLevel = GetCurrentShipLevel;
        sav.EXP = GetCurrentExp;
        sav.EliteSpawnWaves = _eliteSpawnWaves;
        sav.AlreadySpawnedEliteIDs = _alreadySpawnedEliteIDs;

        sav.PlugRuntimeSaves = CreatePlugRuntimeSaveData();
        sav.PropertyRowSav = MainPropertyData.CreatePropertyModifyRowSaveData();

        ///Create
        SaveLoadManager.Save<SaveData>(sav, sav.SaveName, formatJson);
        SaveLoadManager.Instance.AddSaveData(sav);
    }

    private List<PlugRuntimeSaveData> CreatePlugRuntimeSaveData()
    {
        List<PlugRuntimeSaveData> savs = new List<PlugRuntimeSaveData>();
        foreach(var plug in _currentShipPlugs.Values)
        {
            var sav = plug.CreateSaveData();
            savs.Add(sav);
        }
        return savs;
    }

    /// <summary>
    /// ����ship����
    /// </summary>
    private bool LoadShipBaseData(int shipID, int mainWeaponID)
    {
        var shipCfg = DataManager.Instance.GetShipConfig(shipID);
        if (shipCfg == null)
            return false;

        var mainWeaponCfg = DataManager.Instance.GetUnitConfig(mainWeaponID);
        if (mainWeaponCfg == null)
            return false;

        currentShipSelection = new InventoryItem(shipCfg);
        currentWeaponSelection = new InventoryItem(mainWeaponCfg);

        return true;
    }

    private void LoadWreckageDataSave(int wasteCount)
    {
        InitWreckageData();
        AddDropWasteCount(wasteCount);
    }

    private void LoadPropertySave(SaveData sav)
    {
        var maxLevel = DataManager.Instance.battleCfg.ShipMaxLevel;
        _shipLevel = new ChangeValue<byte>(sav.ShipLevel, 1, maxLevel);
        _currentEXP = new ChangeValue<float>(0, sav.EXP, int.MaxValue);
        CurrentRequireEXP = GameHelper.GetEXPRequireMaxCount(GetCurrentShipLevel);

        _currentEXP.BindChangeAction(OnCurrrentEXPChange);
        _shipLevel.BindChangeAction(OnShipLevelUp);

        MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.Luck, OnPlayerLuckChange);

        ///Init MainProperty
        MainPropertyData.LoadPropertyModifyRowSaveData(sav);
    }

    private void LoadPlugRuntimeSaves(List<PlugRuntimeSaveData> savs)
    {
        if (savs == null || savs.Count <= 0)
            return;

        for(int i = 0; i < savs.Count; i++)
        {
            ShipPlugInfo info = ShipPlugInfo.CreateInfo(savs[i]);
            if(info == null)
            {
                Debug.LogError("Plug Save Load Error ! ID = " + savs[i].ID);
                continue;
            }
            AddPlug_Sav(info);
        }
    }

    private void AddPlug_Sav(ShipPlugInfo info)
    {
        if (plugItemCountDic.ContainsKey(info.PlugID))
        {
            plugItemCountDic[info.PlugID]++;
        }
        else
        {
            plugItemCountDic.Add(info.PlugID, 1);
        }

        info.OnAdded();
        if (!_currentShipPlugs.ContainsKey(info.UID))
        {
            _currentShipPlugs.Add(info.UID, info);
            AllCurrentShipPlugs.Add(info);
        }
    }

    #endregion

    #region Log

    public void CreateBattleLog()
    {
        GenerateBattleLog();
    }

    private void GenerateBattleLog()
    {
        if (currentShip == null)
            return;

        BattleLog log = new BattleLog();
        log.EndTime = System.DateTime.Now.ToString("yyyy-MM-dd HH_MM_ss");
        var unitLogDatas = currentShip.GenerateAllUnitLogData();
        log.UnitLogDatas = unitLogDatas;

#if UNITY_EDITOR
        var fileName = string.Format("BattleLog_{0}", log.EndTime);

        DataManager.WriteCSV(fileName, log);
        Debug.Log("����ս����־�ɹ�");
#endif

    }


    #endregion

#if GMDEBUG
    private void CreateBattleObserver()
    {
        if(GM_Observer == null)
        {
            var obj = new GameObject();
            obj.name = "Battle_Observer";
            var cmpt = obj.AddComponent<BattleObserver>();
            cmpt.Refresh();
            GM_Observer = cmpt;
        }
        else
        {
            GM_Observer.Refresh();
        }
    }
#endif

}

public class BattleResultInfo
{
    public int Score;
    public bool Success = false;

    public CampLevelUpInfo campLevelInfo;
}