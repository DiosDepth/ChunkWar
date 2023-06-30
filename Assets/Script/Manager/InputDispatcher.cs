using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;






public class InputDispatcher : MonoBehaviour
{
    public static InputDispatcher Instance;
    public PlayerInput playerInput;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_Move;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_RushDown;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_Point;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_RightClick;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_LeftClick;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_MidClick;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_Pause;

    public UnityAction<InputAction.CallbackContext> Action_UI_UnPause;

    public InputDispatcher()
    {

  

    }

    public void Awake()
    {
        Instance = this;
        playerInput = GetComponent<PlayerInput>();
    }

    /// <summary>
    /// "Player" Or "UI"
    /// </summary>
    /// <param name="inputmapname"></param>
    public void ChangeInputMode(string inputmapname)
    {
        playerInput.SwitchCurrentActionMap(inputmapname);
        playerInput.currentActionMap.Enable();
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

    public void GamePlay_Pause(InputAction.CallbackContext context)
    {

        Action_GamePlay_Pause?.Invoke(context);
    }


    public void UI_UnPause(InputAction.CallbackContext context)
    {

        Action_UI_UnPause?.Invoke(context);
    }

}
