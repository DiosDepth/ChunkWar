using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;






public class InputDispatcher : MonoBehaviour
{ 
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_Move;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_RushDown;

    public InputDispatcher()
    {

  

    }

    public void GamePlay_Move(InputAction.CallbackContext context)
    {
        Action_GamePlay_Move?.Invoke(context);
    }

    public void GamePlay_RushDown(InputAction.CallbackContext context)
    {
        Action_GamePlay_RushDown?.Invoke(context);
    }

}
