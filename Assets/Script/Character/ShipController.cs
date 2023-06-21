using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipController : MonoBehaviour
{

    public Ship ship;
    public Rigidbody2D rb;
    public new CompositeCollider2D collider;

    // Start is called before the first frame update

    public Vector3 movement;

    void Start()
    {


        ship = GetComponent<Ship>();
        InputDispatcher.Instance.Action_GamePlay_Move += HandleMovement;
        InputDispatcher.Instance.Action_GamePlay_RushDown += HandleRushDown;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        InputDispatcher.Instance.Action_GamePlay_Move -= HandleMovement;
        InputDispatcher.Instance.Action_GamePlay_RushDown -= HandleRushDown;
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

                break;
            case InputActionPhase.Canceled:
                break;
        }
        //Debug.Log("HandleRushDown : " + context.phase.ToString() + " Value = " + context.ReadValueAsButton());
    }
    
}