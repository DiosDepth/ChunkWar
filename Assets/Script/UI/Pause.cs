using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pause : GUIBasePanel
{


    public override void Initialization()
    {
        base.Initialization();
        GetGUIComponent<Button>("Resume").onClick.AddListener(OnResumeBtnPressed);
        GetGUIComponent<Button>("Options").onClick.AddListener(OnOptionsBtnPressed);

    }

    private void OnResumeBtnPressed()
    {
        LeanTween.value(9, 0, 0.25f).setOnUpdate((value) =>
        {
            CameraManager.Instance.vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x = value;
            CameraManager.Instance.vcam.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x = value;

        });
        LeanTween.value(0,512,0.25f).setOnUpdate((value)=> 
        {
            uiGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(value, 0);
        }).setOnComplete(()=> 
        {
            InputDispatcher.Instance.ChangeInputMode("Player");
            UIManager.Instance.HiddenUI("Pause");
        });
    }

    private void OnOptionsBtnPressed()
    {
      
    }

    private void OnQuitBtnPressed()
    {
        GameStateTransitionEvent.Trigger( EGameState.EGameState_GameEnd);
    }

    public override void Hidden()
    {
        base.Hidden();
        GetGUIComponent<Button>("Resume").onClick.RemoveListener(OnResumeBtnPressed);
        GetGUIComponent<Button>("Options").onClick.RemoveListener(OnOptionsBtnPressed);
        GetGUIComponent<Button>("Quit").onClick.RemoveListener(OnQuitBtnPressed);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
