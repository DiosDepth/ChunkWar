using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : GUIBasePanel,EventListener<LevelEvent>
{
    public Slider loadingBar;
    public TMP_Text loadingText;
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
        this.EventStartListening<LevelEvent>();
        loadingBar = GetGUIComponent<Slider>("ProgressBar");
        loadingText = GetGUIComponent<TMP_Text>("LoadingText");
    }

    public void OnEvent(LevelEvent evt)
    {
      switch(evt.evtType)
        {
            case LevelEventType.LevelLoading:
                loadingBar.value = evt.asy.progress;
                break;
            case LevelEventType.LevelLoaded:
                loadingBar.value = 1;
                loadingText.text = "Press Space Key";
                StartCoroutine(WaitForActiveScene(evt.levelIndex, evt.asy));
                break;
            case LevelEventType.LevelActive:
                break;
        }
    }

    public IEnumerator WaitForActiveScene(int index, AsyncOperation asy)
    {
        bool isPressAnyKey = false;
        while (!isPressAnyKey)
        {
            if (UnityEngine.InputSystem.Keyboard.current.anyKey.wasPressedThisFrame)
            {
                isPressAnyKey = true;
            }
            yield return null;
        }
        asy.allowSceneActivation = true;

        yield return new WaitForEndOfFrame();
        LevelEvent.Trigger(LevelEventType.LevelActive, index, asy);
    }
}
