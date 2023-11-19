using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpWaste : PickableItem
{
    public int WasteGain = 1;
    public float EXPGain = 1;

    private int tweenUID;

    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    public override void Initialization(PickUpData data)
    {
        base.Initialization(data);
        WasteGain = data.CountRef;
        EXPGain = data.EXPAdd;
    }

    public override void PickUp(GameObject picker)
    {
        base.PickUp(picker);
        SoundManager.Instance.PlayBattleSound("Ship/Waste_Pick", picker.transform);

        tweenUID = LeanTween.value(0, 1, 0.75f).setOnUpdate((alpha) =>
        {
            this.transform.position = Vector3.Lerp(this.transform.position, picker.transform.position, alpha);
        }).setOnComplete(() => 
        {
            AfterPickUp(picker);
        }).uniqueId;
    }

    public override void PoolableReset()
    {
        base.PoolableReset();
        LeanTween.cancel(tweenUID);
    }
}
