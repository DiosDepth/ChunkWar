using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipHUD : GUIBasePanel, EventListener<ShipPropertyEvent>, EventListener<RogueEvent>
{
    private TextMeshProUGUI _waveTextID;
    private TextMeshProUGUI _timerTextID;
    private TextMeshProUGUI _normalDropText;
    private TextMeshProUGUI _Wreckage_T1_Text;
    private TextMeshProUGUI _Wreckage_T2_Text;
    private TextMeshProUGUI _Wreckage_T3_Text;
    private TextMeshProUGUI _Wreckage_T4_Text;

    private ShipPropertySliderCmpt _hpSliderCmpt;
    private ShipPropertySliderCmpt _expCmpt;
    private ShipPropertySliderCmpt _energyCmpt;
    private ShipPropertySliderCmpt _loadCmpt;

    private ShipHUDMessagePanel _messagePanel;

    private List<WeaponRuntimeItemCmpt> _weaponItemCmpts = new List<WeaponRuntimeItemCmpt>();
    private List<BOSSHPSlider> _bossHPSliderCmpts = new List<BOSSHPSlider>();

    private static string WeaponRuntimeItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/WeaponRuntimeItem";
    private static string EnemyBoss_HPSlider_PrefabPath = "Prefab/GUIPrefab/CmptItems/BOSSHPSlider";
    private const string Shop_Teleport_ShowText = "Shop_Teleport_ShowText";
    private const string Shop_Teleport_WarningText = "Shop_Teleport_WarningText";

    protected override void Awake()
    {
        base.Awake();
        _waveTextID = transform.Find("WaveIndex/WaveInfo/Value").SafeGetComponent<TextMeshProUGUI>();
        _Wreckage_T1_Text = transform.Find("Currency/Wreckage_T1/Value").SafeGetComponent<TextMeshProUGUI>();
        _Wreckage_T2_Text = transform.Find("Currency/Wreckage_T2/Value").SafeGetComponent<TextMeshProUGUI>();
        _Wreckage_T3_Text = transform.Find("Currency/Wreckage_T3/Value").SafeGetComponent<TextMeshProUGUI>();
        _Wreckage_T4_Text = transform.Find("Currency/Wreckage_T4/Value").SafeGetComponent<TextMeshProUGUI>();
        _timerTextID = transform.Find("WaveIndex/Time").SafeGetComponent<TextMeshProUGUI>();
        _normalDropText = transform.Find("Currency/NormalDrop/Value").SafeGetComponent<TextMeshProUGUI>();
        _hpSliderCmpt = transform.Find("InfoContent/ShipInfo/HPSlider").SafeGetComponent<ShipPropertySliderCmpt>();
        _energyCmpt = transform.Find("OtherInfo/EnergySlider").SafeGetComponent<ShipPropertySliderCmpt>();
        _loadCmpt = transform.Find("OtherInfo/LoadSlider").SafeGetComponent<ShipPropertySliderCmpt>();
        _expCmpt = transform.Find("InfoContent/ShipInfo/EXPSlider").SafeGetComponent<ShipPropertySliderCmpt>();
        _messagePanel = transform.Find("MessageContent").SafeGetComponent<ShipHUDMessagePanel>();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<ShipPropertyEvent>();
        this.EventStartListening<RogueEvent>();
        SetUpGeneral();
        BindTimerChange(true);
        SetUpSlider();
        RefreshWasteCount();
        RefreshWreckageDrops();
        InitShipWeaponCmpts();

#if GMDEBUG
        UIManager.Instance.ShowUI<BattleLogDialog>("BattleLogDialog", E_UI_Layer.Top, null, (panel) =>
        {
            panel.Initialization();
        });
#endif
    }

    public override void Hidden()
    {
        this.EventStopListening<ShipPropertyEvent>();
        this.EventStopListening<RogueEvent>();
        BindTimerChange(false);

        for (int i = _weaponItemCmpts.Count - 1; i >= 0; i--) 
        {
            _weaponItemCmpts[i].PoolableDestroy();
        }
        _weaponItemCmpts.Clear();

#if GMDEBUG
        UIManager.Instance.HiddenUI("BattleLogDialog");
#endif

        base.Hidden();
    }

    private void SetUpGeneral()
    {
        var currentWaveIndex = RogueManager.Instance.GetCurrentWaveIndex;
        _waveTextID.text = currentWaveIndex.ToString();
    }

    private void SetUpSlider()
    {
        _hpSliderCmpt.RefreshHP();
        _expCmpt.RefreshEXP();
        _expCmpt.RefreshLevelUp();
        _energyCmpt.RefreshEnergy();
        _loadCmpt.RefreshLoad();
    }

    public void OnEvent(ShipPropertyEvent evt)
    {
        switch (evt.type)
        {
            case ShipPropertyEventType.EXPChange:
                RefreshEXP();
                break;

            case ShipPropertyEventType.LevelUp:
                RefreshLevelUp();
                break;

            case ShipPropertyEventType.CoreHPChange:
                RefreshShipCoreHP();
                break;

            case ShipPropertyEventType.EnergyChange:
                RefreshShipEnergy();
                break;

            case ShipPropertyEventType.ReloadCDStart:
                OnWeaponReloadCDStart((uint)evt.param[0]);
                break;

            case ShipPropertyEventType.ReloadCDEnd:
                OnWeaponReloadCDEnd((uint)evt.param[0]);
                break;

        }
    }

    public void OnEvent(RogueEvent evt)
    {
        switch (evt.type)
        {
            case RogueEventType.WasteCountChange:
                RefreshWasteCount();
                break;

            case RogueEventType.WreckageDropRefresh:
                RefreshWreckageDrops();
                break;

            case RogueEventType.ShopTeleportSpawn:
                var text1 = LocalizationManager.Instance.GetTextValue(Shop_Teleport_ShowText);
                _messagePanel.ShowTopMessage(text1);
                break;

            case RogueEventType.ShopTeleportWarning:
                var text2 = LocalizationManager.Instance.GetTextValue(Shop_Teleport_WarningText);
                _messagePanel.ShowTopMessage(text2);
                break;

            case RogueEventType.RegisterBossHPBillBoard:
                var aiShip = (AIShip)evt.param[0];
                AddBossHPBillboard(aiShip);
                break;

            case RogueEventType.RemoveBossHPBillBoard:
                var aiShip2 = (AIShip)evt.param[0];
                RemoveBossHPBillBoard(aiShip2);
                break;

            case RogueEventType.RefreshTimerDisplay:
                var timer = RogueManager.Instance.Timer;
                UpdateWaveTimer(timer.CurrentSecond);
                break;
        }
    }

    /// <summary>
    /// 初始化舰船武器信息
    /// </summary>
    private void InitShipWeaponCmpts()
    {
        var currentShip = RogueManager.Instance.currentShip;
        if (currentShip == null)
            return;

        var parnetTrans = transform.Find("InfoContent/WeaponContent");
        var allWeapons = currentShip.GetAllShipUnitByType<Weapon>();
        for(int i = 0; i < allWeapons.Count; i++)
        {
            PoolManager.Instance.GetObjectSync(WeaponRuntimeItem_PrefabPath, true, (obj) =>
            {
                var cmpt = obj.transform.SafeGetComponent<WeaponRuntimeItemCmpt>();
                cmpt.SetUp(allWeapons[i]);
                _weaponItemCmpts.Add(cmpt);
            }, parnetTrans);
        }
    }

    private void RefreshEXP()
    {
        _expCmpt.RefreshEXP();
    }

    private void RefreshWasteCount()
    {
        _normalDropText.text = RogueManager.Instance.GetDropWasteCount.ToString();
    }

    private void RefreshWreckageDrops()
    {
        var drops = RogueManager.Instance.GetInLevelWreckageDrops;
        _Wreckage_T1_Text.text = drops[GoodsItemRarity.Tier1].ToString();
        _Wreckage_T2_Text.text = drops[GoodsItemRarity.Tier2].ToString();
        _Wreckage_T3_Text.text = drops[GoodsItemRarity.Tier3].ToString();
        _Wreckage_T4_Text.text = drops[GoodsItemRarity.Tier4].ToString();
    }

    private void RefreshLevelUp()
    {
        _expCmpt.RefreshLevelUp();
    }

    private void RefreshShipCoreHP()
    {
        _hpSliderCmpt.RefreshHP();
    }

    private void RefreshShipEnergy()
    {
        _energyCmpt.RefreshEnergy();
    }

    private void BindTimerChange(bool bind)
    {
        var timer = RogueManager.Instance.Timer;
        if (bind)
        {
            timer.OnTimeSecondUpdate += UpdateWaveTimer;
            UpdateWaveTimer(timer.CurrentSecond);
        }
        else
        {
            timer.OnTimeSecondUpdate -= UpdateWaveTimer;
        }
    }

    private void UpdateWaveTimer(int totalSecond)
    {
        _timerTextID.text = string.Format("{0:d2} : {1:d2}", totalSecond / 60, totalSecond % 60);
    }

    private void OnWeaponReloadCDStart(uint weaponUID)
    {
        var cmpt = GetWeaponItemCmpt(weaponUID);
        if(cmpt != null)
        {
            cmpt.StartReloadCDTimer();
        }
    }

    private void OnWeaponReloadCDEnd(uint weaponUID)
    {
        var cmpt = GetWeaponItemCmpt(weaponUID);
        if (cmpt != null)
        {
            cmpt.EndReloadCDTimer();
        }
    }

    private WeaponRuntimeItemCmpt GetWeaponItemCmpt(uint weaponUID)
    {
        for (int i = 0; i < _weaponItemCmpts.Count; i++) 
        {
            var cmpt = _weaponItemCmpts[i];
            if (cmpt != null && cmpt._targetWeapon != null && cmpt._targetWeapon.UID == weaponUID)
                return cmpt;
        }
        return null;
    }

    private void AddBossHPBillboard(AIShip ship)
    {
        var slider = GetBossHPSlider(ship);
        if (slider != null)
            return;

        var root = transform.Find("BossHPContent");
        var obj = ResManager.Instance.Load<GameObject>(EnemyBoss_HPSlider_PrefabPath);
        if(obj != null)
        {
            var cmpt = obj.transform.SafeGetComponent<BOSSHPSlider>();
            cmpt.transform.SetParent(root, false);
            cmpt.SetUpSlider(ship);
            _bossHPSliderCmpts.Add(cmpt);
        }
    }

    private void RemoveBossHPBillBoard(AIShip ship)
    {
        var slider = GetBossHPSlider(ship);
        if(slider != null)
        {
            slider.RemoveSlider();
            _bossHPSliderCmpts.Remove(slider);
        }
    }

    private BOSSHPSlider GetBossHPSlider(AIShip ship)
    {
        return _bossHPSliderCmpts.Find(x => x._targetShip == ship);
    }
}
