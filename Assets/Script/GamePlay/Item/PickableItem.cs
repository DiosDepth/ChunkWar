using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


public struct PickableItemEvent
{
   
    public GameObject Picker;
    public PickableItem PickedItem;

    public PickableItemEvent(PickableItem pickedItem, GameObject picker)
    {
        Picker = picker;
        PickedItem = pickedItem;
    }
    static PickableItemEvent e;
    public static void Trigger(PickableItem pickedItem, GameObject picker)
    {
        e.Picker = picker;
        e.PickedItem = pickedItem;
        EventCenter.Instance.TriggerEvent<PickableItemEvent>(e);
    }
}


public enum PickableItemType
{
    InventoryItem,//ʰȡ�������Ʒ����
    Immediately,//ʰȡ�����̷���Ч��
    Modify,//ʰȡ���޸Ķ�Ӧ����
    Event,//ʰȡ�����¼���
}

public class PickableItem : PoolableObject
{
    public SpriteRenderer sprite;
    public Collider2D trigger;
    public PickableItemType itemType;
    public bool disableOnPick = true;


    public InventoryItem pickedItem;

    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }

    public override void Initialization()
    {

    }

    //�����ж��Ƿ�����ʰȡ����
    protected virtual bool IsPickUpValid()
    {
        return true;
    }


    //ʰȡ�¼���ȫ��֪ͨ�� �������¼���ϵͳ���յ���Ϣ��Ȼ�����Լ����߼�������inventory
    public virtual void PickUp(GameObject picker)
    {
        if (!IsPickUpValid()) { return; }

        switch (itemType)
        {
            case PickableItemType.InventoryItem:
                PickableItemAction_InventoryItem(picker);
                break;
            case PickableItemType.Immediately:
                PickableItemAction_Immediately(picker);
                break;
            case PickableItemType.Modify:

                PickableItemAction_Modify(picker);

                break;
            case PickableItemType.Event:
                PickableItemAction_Event(picker);
                break;
        }
  
    }
    protected virtual void PickableItemAction_InventoryItem(GameObject picker)
    {

    }

    protected virtual void PickableItemAction_Immediately(GameObject picker)
    {

    }


    protected virtual void PickableItemAction_Modify(GameObject picker)
    {
        //�����Pick��һ��Modify����Ҫʵ��Modify����
    }

    protected virtual void PickableItemAction_Event(GameObject picker)
    {
        PickableItemEvent.Trigger(this, picker);
    }


    public override void Reset()
    {
        base.Reset();

    }



    public override void Destroy()
    {
        base.Destroy();
    }
    //������һЩʰȡ֮��ı���
    protected virtual void AfterPickUp(GameObject picker)
    {
        if (disableOnPick)
        {
            trigger.enabled = false;
            Destroy();
        }
    }


    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {

    }
}