using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MinimapToggle : MonoBehaviour
{
    public GameObject minimap;
    private PlayerInputActions inputActions;
    
    void Start()
    {
        inputActions = InputManager.inputActions;
        inputActions.Gameplay.ToggleMinimap.performed += ToggleMinimap;
    }
    
    private void ToggleMinimap(InputAction.CallbackContext context)
    {
        minimap.SetActive(!minimap.activeSelf);
    }
}