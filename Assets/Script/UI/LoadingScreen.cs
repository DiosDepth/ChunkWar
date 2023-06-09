﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LoadingScreen : GUIBasePanel
{

    public RectTransform leftDoor;
    public RectTransform rightDoor;
    public CanvasGroup loadingText;

    public bool state = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Initialization()
    {
        base.Initialization();

    }

    public void OpenLoadingDoor(UnityAction callback)
    {
        LeanTween.alphaCanvas(loadingText, 0, 0.5f).setOnComplete(()=> 
        {
            LeanTween.move(leftDoor, new Vector3(-1000, 0, 0), 0.5f);
            LeanTween.move(rightDoor, new Vector3(1000, 0, 0), 0.5f);
            LeanTween.delayedCall(0.5f, () => { callback?.Invoke();});
        });

    }

    public void CloseLoadingDoor(UnityAction callback)
    {
        LeanTween.move(leftDoor, new Vector3(0, 0, 0), 0.5f);
        LeanTween.move(rightDoor, new Vector3(0, 0, 0), 0.5f);
        LeanTween.delayedCall(0.5f, () => 
        {
            LeanTween.alphaCanvas(loadingText,1, 0.5f).setOnComplete(() => { callback?.Invoke(); });
        });

    }

    public void SetDoorState(bool newstate)
    {
        state = newstate;

        if (state)
        {

            loadingText.alpha = 0;
            //loadingText.anchoredPosition = new Vector2(256, 64);
            leftDoor.anchoredPosition = new Vector2(-960,0);
            rightDoor.anchoredPosition = new Vector2(960, 0);
        }
        else
        {
            loadingText.alpha = 1;
            //loadingText.anchoredPosition = new Vector2(-64, 64);
            leftDoor.anchoredPosition = new Vector2(0, 0);
            rightDoor.anchoredPosition = new Vector2(0, 0);
        }
    }
}
