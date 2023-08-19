using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpWreckage : PickableItem
{
    public GoodsItemRarity DropRarity;


    protected override void Start()
    {

    }

    // Update is called once per frame
    protected override void Update()
    {

    }

    public override void PickUp(GameObject picker)
    {
        base.PickUp(picker);
        RogueManager.Instance.AddInLevelDrop(DropRarity);

        LeanTween.value(0, 1, 0.75f).setOnUpdate((alpha) =>
        {
            this.transform.position = Vector3.Lerp(this.transform.position, picker.transform.position, alpha);
        }).setOnComplete(() =>
        {
            AfterPickUp(picker);
        });
    }
}
