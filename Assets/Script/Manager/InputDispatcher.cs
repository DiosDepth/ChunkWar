using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;






public class InputDispatcher : MonoBehaviour
{
    public static InputDispatcher Instance;

    public UnityAction<InputAction.CallbackContext> Action_GamePlay_Move;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_RushDown;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_Point;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_RightClick;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_LeftClick;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_MidClick;

    public InputDispatcher()
    {

  

    }

    public void Awake()
    {
        Instance = this;
    }

    public void GamePlay_Move(InputAction.CallbackContext context)
    {
        Action_GamePlay_Move?.Invoke(context);
    }

    public void GamePlay_RushDown(InputAction.CallbackContext context)
    {
        Action_GamePlay_RushDown?.Invoke(context);
    }

    public void GamePlay_Point(InputAction.CallbackContext context)
    {
        Action_GamePlay_Point?.Invoke(context);
    }

    public void GamePlay_RightClick(InputAction.CallbackContext context)
    {
        
        Action_GamePlay_RightClick?.Invoke(context);
    }

    public void GamePlay_LeftClick(InputAction.CallbackContext context)
    {
     
        Action_GamePlay_LeftClick?.Invoke(context);
    }

    public void GamePlay_MidClick(InputAction.CallbackContext context)
    {

        Action_GamePlay_MidClick?.Invoke(context);
    }

}
