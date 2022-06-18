using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputKeyButton : MonoBehaviour
{
    public InputActionProperty inputAction;
    
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    // Start is called before the first frame update
    void Start()
    {
        inputAction.action.performed += context =>
        {
            if (button.isActiveAndEnabled && button.interactable)
            {
                button.onClick?.Invoke();
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        inputAction.action.Enable();
    }

    void OnDisable()
    {
        inputAction.action.Disable();
    }
}
