using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static PlayerInputActions inputActions { get; private set; }

    private void Awake()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
        }
    }
    
    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

}
