using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopTeleport : PickableItem
{
    private bool _hasEnterShop = false;

    public override void Initialization()
    {
        base.Initialization();
        _hasEnterShop = false;
        DelayDestroy();
    }

    public override void PickUp(GameObject picker)
    {
        base.PickUp(picker);
        _hasEnterShop = true;
        Debug.Log("Enter Shop");
        RogueManager.Instance.EnterShop();
        AfterPickUp(picker);  
    }

    protected override void AfterPickUp(GameObject picker)
    {
        base.AfterPickUp(picker);
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

    private bool Vaild()
    {
        return gameObject != null && !_hasEnterShop;
    }
}
