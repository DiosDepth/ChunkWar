using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChaController : MonoBehaviour
{

    public Character character;
    public InputDispatcher dispatcher;
    // Start is called before the first frame update

    public Vector3 movement;

    void Start()
    {

        dispatcher = GameObject.Find("EventSystem(Clone)").GetComponent<InputDispatcher>();
        character = GetComponent<Character>();
        dispatcher.Action_GamePlay_Move += HandleMovement;
        dispatcher.Action_GamePlay_RushDown += HandleRushDown;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        dispatcher.Action_GamePlay_Move -= HandleMovement;
        dispatcher.Action_GamePlay_RushDown -= HandleRushDown;
    }
    public void HandleMovement(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Disabled:
                break;
            case InputActionPhase.Waiting:
                break;
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Performed:
                if((character.state.CurrentState != CharacterState.RushDown && character.state.CurrentState != CharacterState.RushingDown) || character.state.CurrentState != CharacterState.Death)
                {
                    movement.x = context.ReadValue<Vector2>().x;
                }
                break;
            case InputActionPhase.Canceled:
                movement.x = context.ReadValue<Vector2>().x;
                break;
        }
        
       // Debug.Log("HandleMovement : " + context.phase.ToString() + " Value = " + context.ReadValue<Vector2>());
    }
    public void HandleRushDown(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Disabled:
                break;
            case InputActionPhase.Waiting:
                break;
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Performed:
                if ((character.state.CurrentState != CharacterState.RushDown && character.state.CurrentState != CharacterState.RushingDown) || character.state.CurrentState != CharacterState.Death)
                {
                    character.state.ChangeState(CharacterState.RushDown);
                }
                break;
            case InputActionPhase.Canceled:
                break;
        }
        //Debug.Log("HandleRushDown : " + context.phase.ToString() + " Value = " + context.ReadValueAsButton());
    }
    
}