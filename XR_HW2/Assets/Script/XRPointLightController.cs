using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class XRPointLightController : MonoBehaviour
{
    // Reference to the Light component
    private Light pointLight;

    // Input action for changing color
    public InputActionProperty changeColorAction;

    // Predefined colors
    public Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow };
    private int currentColorIndex = 0;

    void Start()
    {
        // Get the Light component attached to this GameObject
        pointLight = GetComponent<Light>();
        
        if (pointLight == null)
        {
            Debug.LogError("No Light component found on this GameObject!");
        }

        // Enable the input action
        if (changeColorAction != null)
        {
            changeColorAction.action.Enable();
        }
    }

    void Update()
    {
        if (pointLight == null) return;

        // Check if the button is pressed
        if (changeColorAction != null && changeColorAction.action.triggered)
        {
            // Cycle through predefined colors
            currentColorIndex = (currentColorIndex + 1) % colors.Length;
            pointLight.color = colors[currentColorIndex];
        }
    }
}
