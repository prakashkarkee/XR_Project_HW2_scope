using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class CustomGrab : MonoBehaviour
{
    [Header("Grabbing Settings")]
    [Tooltip("Enable double rotation speed (Extra Credit)")]
    public bool doubleRotation = false;
    
    [Header("Keyboard Toggle")]
    public KeyCode toggleKey = KeyCode.R;
    
    // Track all controllers grabbing this object
    private List<Transform> grabbingControllers = new List<Transform>();
    private List<Vector3> previousPositions = new List<Vector3>();
    private List<Quaternion> previousRotations = new List<Quaternion>();
    
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;
    
    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        if (grabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable component missing on " + gameObject.name);
            return;
        }
        
        // Subscribe to grab/release events
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }
    
    void Update()
    {
        // Toggle double rotation with keyboard
        if (Input.GetKeyDown(toggleKey))
        {
            doubleRotation = !doubleRotation;
            Debug.Log("Double Rotation: " + (doubleRotation ? "ON" : "OFF"));
        }
        
        // Apply transformations if any controller is grabbing
        if (isGrabbed && grabbingControllers.Count > 0)
        {
            ApplyTransformations();
        }
    }
    
    void OnGrabbed(SelectEnterEventArgs args)
    {
        Transform controller = args.interactorObject.transform;
        
        // Add this controller to our list
        grabbingControllers.Add(controller);
        previousPositions.Add(controller.position);
        previousRotations.Add(controller.rotation);
        
        isGrabbed = true;
        
        Debug.Log($"Grabbed! Total hands: {grabbingControllers.Count}");
    }
    
    void OnReleased(SelectExitEventArgs args)
    {
        Transform controller = args.interactorObject.transform;
        
        // Find and remove this controller
        int index = grabbingControllers.IndexOf(controller);
        if (index >= 0)
        {
            grabbingControllers.RemoveAt(index);
            previousPositions.RemoveAt(index);
            previousRotations.RemoveAt(index);
        }
        
        if (grabbingControllers.Count == 0)
        {
            isGrabbed = false;
        }
        
        Debug.Log($"Released! Remaining hands: {grabbingControllers.Count}");
    }
    
    void ApplyTransformations()
    {
        Vector3 totalTranslation = Vector3.zero;
        Quaternion totalRotation = Quaternion.identity;
        
        // Process each grabbing controller
        for (int i = 0; i < grabbingControllers.Count; i++)
        {
            Transform controller = grabbingControllers[i];
            
            // Calculate delta position (how much controller moved)
            Vector3 deltaPosition = controller.position - previousPositions[i];
            
            // Calculate delta rotation (how much controller rotated)
            Quaternion deltaRotation = controller.rotation * Quaternion.Inverse(previousRotations[i]);
            
            // --- ROTATION AROUND CONTROLLER ORIGIN ---
            // Vector from controller to object
            Vector3 toObject = transform.position - controller.position;
            
            // Rotate this vector by delta rotation
            Vector3 rotatedVector = deltaRotation * toObject;
            
            // The additional translation caused by rotation
            Vector3 rotationalTranslation = rotatedVector - toObject;
            
            // Combine both translations
            totalTranslation += deltaPosition + rotationalTranslation;
            
            // Combine rotations (quaternion multiplication)
            totalRotation *= deltaRotation;
            
            // Update previous values for next frame
            previousPositions[i] = controller.position;
            previousRotations[i] = controller.rotation;
        }
        
        // Average the translation if multiple hands
        if (grabbingControllers.Count > 1)
        {
            totalTranslation /= grabbingControllers.Count;
        }
        
        // Apply double rotation if enabled (Extra Credit)
        if (doubleRotation)
        {
            totalRotation = DoubleRotationMagnitude(totalRotation);
        }
        
        // Apply final transformations to object
        transform.position += totalTranslation;
        transform.rotation = totalRotation * transform.rotation;
    }
    
    // Extra Credit: Double the rotation magnitude
    Quaternion DoubleRotationMagnitude(Quaternion rotation)
    {
        // Convert quaternion to angle-axis representation
        float angle;
        Vector3 axis;
        rotation.ToAngleAxis(out angle, out axis);
        
        // Handle the case where there's no rotation
        if (axis == Vector3.zero)
        {
            return Quaternion.identity;
        }
        
        // Double the angle
        angle *= 2f;
        
        // Create new quaternion with doubled angle
        return Quaternion.AngleAxis(angle, axis);
    }
    
    void OnDestroy()
    {
        // Cleanup event listeners
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}
