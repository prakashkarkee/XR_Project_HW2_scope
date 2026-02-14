using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class LightSwitch : MonoBehaviour
{
    public InputActionReference changeColorAction;
    
    private Light myLight;

    void Start()
    {
        myLight = GetComponent<Light>();
    }

    void Update()
    {
        bool vrPressed = (changeColorAction != null && changeColorAction.action != null && changeColorAction.action.WasPressedThisFrame());
        bool keyboardPressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);

        if (vrPressed || keyboardPressed)
        {
            ChangeLightColor();
        }
    }

    void ChangeLightColor()
    {
        if (myLight != null)
        {
            myLight.color = new Color(Random.value, Random.value, Random.value);
        }
    }
}
