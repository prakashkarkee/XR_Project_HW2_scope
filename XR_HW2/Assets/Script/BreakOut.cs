using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class BreakOut : MonoBehaviour
{
    public InputActionReference moveAction;

    private Vector3 insidePos = new Vector3(0, 1.6f, -3);
    private Vector3 outsidePos = new Vector3(0, 20, -10); 

    private bool isOutside = false; 

    void Update()
    {
        bool vrPressed = (moveAction != null && moveAction.action != null && moveAction.action.WasPressedThisFrame());
        bool keyboardPressed = (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame);

        if (vrPressed || keyboardPressed)
        {
            TogglePosition();
        }
    }

    void TogglePosition()
    {
        if (isOutside)
        {
            transform.position = insidePos;
            isOutside = false;
        }
        else
        {
            transform.position = outsidePos;
            isOutside = true;
        }
    }
}
