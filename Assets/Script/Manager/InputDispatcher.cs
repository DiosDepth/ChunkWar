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
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_Attack;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_Point;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_RightClick;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_LeftClick;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_MidClick;
    public UnityAction<InputAction.CallbackContext> Action_GamePlay_Pause;

    public UnityAction<InputAction.CallbackContext> Action_UI_UnPause;
    public UnityAction<InputAction.CallbackContext> Action_UI_Click;
    public UnityAction<InputAction.CallbackContext> Action_UI_Point;

    public void Awake()
    {
        Instance = this;
        playerInput = GetComponent<PlayerInput>();
    }

    public void OnDestroy()
    {
        
    }

    /// <summary>
    /// "Player" Or "UI"
    /// </summary>
    /// <param name="inputmapname"></param>
    public void ChangeInputMode(string inputmapname)
    {
        if (string.Compare(playerInput.currentActionMap.name, inputmapname) == 0)
            return;

        playerInput.SwitchCurrentActionMap(inputmapname);
        playerInput.currentActionMap.Enable();
    }

    public void GamePlay_Move(InputAction.CallbackContext context)
    {
        Action_GamePlay_Move?.Invoke(context);
    }
    public void GamePlay_Attack(InputAction.CallbackContext context)
    {
        Action_GamePlay_Attack?.Invoke(context);
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

    public void UI_Click(InputAction.CallbackContext context)
    {
        Action_UI_Click?.Invoke(context);
    }

    public void UI_Point(InputAction.CallbackContext context)
    {
        Action_UI_Point?.Invoke(context);
    }
}
