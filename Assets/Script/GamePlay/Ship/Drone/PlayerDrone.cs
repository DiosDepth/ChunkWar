using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDrone : BaseDrone
{



    protected override void Death(UnitDeathInfo info)
    {
        base.Death(info);
        //DestroyAIShipBillBoard();
        ResetAllAnimation();
        //LevelManager.Instance.pickupList.AddRange(Drop());
        if (!string.IsNullOrEmpty(droneCfg.DieAudio))
        {
            SoundManager.Instance.PlayBattleSound(droneCfg.DieAudio, transform);
        }
        ECSManager.Instance.UnRegisterJobData(OwnerType.Player, this);
        //AIManager.Instance.RemoveAI(this);

        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + deathVFXName, true, (vfx) =>
        {
            vfx.transform.position = this.transform.position;
            vfx.GetComponent<ParticleController>().PoolableSetActive(true);
            vfx.GetComponent<ParticleController>().PlayVFX();
            PoolableDestroy();
        });
    }
}
