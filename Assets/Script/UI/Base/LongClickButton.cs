using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class LongClickButton : Button
{
    [SerializeField]
    private float ClickDelta = 0.2f;

    private bool _isPointerDown = false;
    private float timePressStarted;

    public Action ClickAction;

    private void Update()
    {
        if(_isPointerDown)
        {
            timePressStarted += Time.deltaTime;
            if (timePressStarted >= ClickDelta)
            {
                timePressStarted = 0f;
                ClickAction?.Invoke();
            }
        }
    }


    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        timePressStarted = 0f;
        _isPointerDown = true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        _isPointerDown = false;
        timePressStarted = 0f;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        _isPointerDown = false;
        timePressStarted = 0f;
    }

}
