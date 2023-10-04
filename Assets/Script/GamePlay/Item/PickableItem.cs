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

public enum AvaliablePickUp
{
    None = 0,
    WastePickup = 1,
    HarborTeleportPickup = 2,
    Wreckage = 3,
}



public class PickableItem : MonoBehaviour, IPoolable
{
    public SpriteRenderer sprite;
    public Collider2D trigger;
    public PickableItemType itemType;
    public bool disableOnPick = true;
    public bool CanAutoPickUp = false;


    public InventoryItem pickedItem;

    protected virtual void Start()
    {

    }

    public virtual void Initialization()
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


    public virtual void PoolableReset()
    {
        ///Reset
        trigger.enabled = true;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.pickupList.Remove(this);
        }
    }



    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }
    //������һЩʰȡ֮��ı���
    protected virtual void AfterPickUp(GameObject picker)
    {
        if (disableOnPick)
        {
            trigger.enabled = false;
            PoolableDestroy();
        }
    }


    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {

    }

    protected virtual void OnTriggerStay2D (Collider2D collider)
    {

    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
        trigger.enabled = true;
    }
}
