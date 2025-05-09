using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MinimapToggle : MonoBehaviour
{
    public GameObject minimap;
    private PlayerInputActions inputActions;
    
    void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Gameplay.ToggleMinimap.performed += ToggleMinimap;
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    private void ToggleMinimap(InputAction.CallbackContext context)
    {
        minimap.SetActive(!minimap.activeSelf);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
