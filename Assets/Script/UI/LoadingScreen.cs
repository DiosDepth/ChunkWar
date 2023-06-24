using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LoadingScreen : GUIBasePanel
{

    public RectTransform leftDoor;
    public RectTransform rightDoor;
    public RectTransform loadingText;

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
        LeanTween.move(loadingText, new Vector3(256, 64, 0), 0.25f).setOnComplete(()=> 
        {
            LeanTween.move(leftDoor, new Vector3(-960, 0, 0), 0.25f);
            LeanTween.move(rightDoor, new Vector3(960, 0, 0), 0.25f);
            LeanTween.delayedCall(0.25f, () => { callback?.Invoke();});
        });

    }

    public void CloseLoadingDoor(UnityAction callback)
    {
        LeanTween.move(leftDoor, new Vector3(0, 0, 0), 0.25f);
        LeanTween.move(rightDoor, new Vector3(0, 0, 0), 0.25f);
        LeanTween.delayedCall(0.25f, () => 
        {
            LeanTween.move(loadingText, new Vector3(-64, 64, 0), 0.25f).setOnComplete(() => { callback?.Invoke(); });
        });

    }

    public void SetDoorState(bool newstate)
    {
        state = newstate;
    }
}
