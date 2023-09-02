using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpWreckage : PickableItem
{
    public GoodsItemRarity DropRarity;
    public float EXPAdd;


    protected override void Start()
    {

    }

    public override void PickUp(GameObject picker)
    {
        base.PickUp(picker);
        RogueManager.Instance.AddInLevelDrop(DropRarity);
        ///Add EXP
        RogueManager.Instance.AddEXP(EXPAdd);

        LeanTween.value(0, 1, 0.75f).setOnUpdate((alpha) =>
        {
            this.transform.position = Vector3.Lerp(this.transform.position, picker.transform.position, alpha);
        }).setOnComplete(() =>
        {
            AfterPickUp(picker);
        });
    }
}
