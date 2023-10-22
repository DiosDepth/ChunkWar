using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopTeleport : TriggerOptionItem
{
    private bool _hasEnterShop = false;

    private const string BattleOption_EnterShop = "BattleOption_EnterShop";

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Init()
    {
        base.Init();
        Option = new BattleOptionItem()
        {
            OptionName = LocalizationManager.Instance.GetTextValue(BattleOption_EnterShop),
            OptionSprite = OptionSprite
        };
        _hasEnterShop = false;
        SoundManager.Instance.PlayBattleSound("Ship/Shop_Appear", transform);
        DelayDestroy();
    }

    protected override void OnTrigger()
    {
        base.OnTrigger();
        OnEnterShop();
    }

    private async void DelayDestroy()
    {
        var refreshCfg = DataManager.Instance.gameMiscCfg.RefreshConfig;
        int delta = 1000 * (refreshCfg.Shop_Teleport_StayTime - refreshCfg.Shop_Teleport_WarningTime);
        delta = Mathf.Clamp(delta, 0, delta);

        await UniTask.Delay(refreshCfg.Shop_Teleport_WarningTime * 1000);
        if (!Vaild())
            return;

        RogueEvent.Trigger(RogueEventType.ShopTeleportWarning);

        await UniTask.Delay(delta);
        if (!Vaild())
            return;

        PoolableDestroy();
    }

    private void OnEnterShop()
    {
        if (_hasEnterShop)
            return;

        _hasEnterShop = true;
        RogueManager.Instance.EnterShop();
        PoolableDestroy();
    }

    private bool Vaild()
    {
        return gameObject != null && !_hasEnterShop;
    }
}
