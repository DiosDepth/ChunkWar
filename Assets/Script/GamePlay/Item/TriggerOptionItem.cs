using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class BattleOptionItem
{
    public string OptionName;
    public Sprite OptionSprite;
}

/// <summary>
/// 触发制物件
/// </summary>
public class TriggerOptionItem : MonoBehaviour, IPoolable
{

    public Sprite OptionSprite;

    private Collider2D trigger;

    protected BattleOptionItem Option;
    private BattleOptionItemUI itemUI;

    protected virtual void Awake()
    {
        trigger = transform.Find("Trigger").SafeGetComponent<Collider2D>();
    }

    public virtual void Init()
    {
        trigger.enabled = true;
 
    }

    protected virtual void OnTrigger()
    {

    }

    public virtual void OnEnter()
    {
        if (Option == null)
            return;

        InputDispatcher.Instance.Action_GamePlay_Interact += HandlePressInput;
        UIManager.Instance.CreatePoolerUI<BattleOptionItemUI>("BattleOptionItem", true, E_UI_Layer.Top, this, (panel) =>
        {
            panel.Initialization(Option);
            itemUI = panel;
        });
    }

    public virtual void OnExit()
    {
        if (itemUI == null)
            return;

        InputDispatcher.Instance.Action_GamePlay_Interact += HandlePressInput;
        UIManager.Instance.BackPoolerUI("BattleOptionItemUI", itemUI.gameObject);
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        if (itemUI != null)
        {
            UIManager.Instance.BackPoolerUI("BattleOptionItemUI", itemUI.gameObject);
        }
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableReset()
    {
        trigger.enabled = false;
        InputDispatcher.Instance.Action_GamePlay_Interact -= HandlePressInput;
    }

    public void PoolableSetActive(bool isactive = true)
    {

    }

    private void HandlePressInput(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.IsPauseGame()) { return; }

        OnTrigger();
    }

}
