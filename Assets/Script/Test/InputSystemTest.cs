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

        Debug.Log("currentActionMap = " + playerinput.currentActionMap);
        Debug.Log("Move Action = " + playerinput.currentActionMap.FindAction("Move").GetBindingIndex());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
