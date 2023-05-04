using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UseKeys : MonoBehaviour {

    public PlayerInput inputcomp;
    private void Awake()
    {
       

    }

    void Update(){


    }

    public void Jump(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Jump button in " + callbackContext.phase);
    }

    public void WSAD(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("WSAD in " + callbackContext.phase + " | value = " + callbackContext.ReadValue<Vector2>());
        switch (callbackContext.phase)
        {
            case InputActionPhase.Disabled:
                break;
            case InputActionPhase.Waiting:
                break;
            case InputActionPhase.Started:
           
                break;
            case InputActionPhase.Performed:
                break;
            case InputActionPhase.Canceled:
                break;
        }
    }
    //end o class
}
