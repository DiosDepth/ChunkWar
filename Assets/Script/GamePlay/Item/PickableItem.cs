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
    InventoryItem,//拾取后进入物品背包
    Immediately,//拾取后立刻发挥效果
    Modify,//拾取后修改对应属性
    Event,//拾取后发送事件，
}

public class PickableItem : MonoBehaviour, IPoolable
{
    public SpriteRenderer sprite;
    public Collider2D trigger;
    public PickableItemType itemType;
    public bool disableOnPick = true;


    public InventoryItem pickedItem;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    public virtual void Initialization()
    {

    }

    //用来判断是否满足拾取条件
    protected virtual bool IsPickUpValid()
    {
        return true;
    }


    //拾取事件的全局通知， 监听此事件的系统会收到消息，然后处理自己的逻辑，比如inventory
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
        //这里给Pick上一个Modify，需要实现Modify的类
    }

    protected virtual void PickableItemAction_Event(GameObject picker)
    {
        PickableItemEvent.Trigger(this, picker);
    }


    public  void PoolableReset()
    {

    }



    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }
    //用来做一些拾取之后的表现
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
    }
}
