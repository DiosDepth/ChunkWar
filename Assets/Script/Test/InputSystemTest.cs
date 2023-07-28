using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemTest : MonoBehaviour
{
    public PlayerInput playerinput;

    // Start is called before the first frame update
    void Start()
    {

        InputDispatcher.Instance.Action_GamePlay_Attack += HandleAttack;
        //Debug.Log("currentActionMap = " + playerinput.currentActionMap);
        //Debug.Log("Move Action = " + playerinput.currentActionMap.FindAction("Move").GetBindingIndex());
    }

    private void HandleAttack(InputAction.CallbackContext context)
    {
       // Debug.Log("HandleAttack : " + context.phase);
        switch (context.phase)
        {
            case InputActionPhase.Disabled:
                
                break;
            case InputActionPhase.Waiting:
                break;
            case InputActionPhase.Started:
                Debug.Log("Started");
                break;
            case InputActionPhase.Performed:
                Debug.Log("Performed");
                break;
            case InputActionPhase.Canceled:
                Debug.Log("Canceled");
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
