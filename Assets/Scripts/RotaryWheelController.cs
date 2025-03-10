using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class RotaryWheelController : MonoBehaviour
{
    [Header("Settings")]
    public Transform artilleryBase;
    public float rotationSensitivity = 1.0f;
    
    // Optional: Add rotation limits
    public bool useRotationLimits = false;
    public Vector2 rotationLimits = new Vector2(-180f, 180f);

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Transform interactor;
    private Quaternion lastInteractorRotation;
    private float currentRotation;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        
        // Initialize current rotation
        currentRotation = artilleryBase.localEulerAngles.y;
        if (currentRotation > 180f)
            currentRotation -= 360f;
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        interactor = args.interactorObject.transform;
        lastInteractorRotation = interactor.rotation;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        interactor = null;
    }

    void Update()
    {
        if (interactor == null) return;

        // Calculate rotation change
        Quaternion currentRotation = interactor.rotation;
        float rotationDelta = CalculateRotationChange(lastInteractorRotation, currentRotation);
        
        // Apply rotation to the artillery base
        if (useRotationLimits)
        {
            // Keep track of total rotation and apply limits
            this.currentRotation += rotationDelta * rotationSensitivity;
            this.currentRotation = Mathf.Clamp(this.currentRotation, rotationLimits.x, rotationLimits.y);
            artilleryBase.localRotation = Quaternion.Euler(0, this.currentRotation, 0);
        }
        else
        {
            // Just rotate directly - no limits
            artilleryBase.Rotate(Vector3.up, rotationDelta * rotationSensitivity);
        }
        
        // Store current rotation for next frame
        lastInteractorRotation = currentRotation;
    }
    
    private float CalculateRotationChange(Quaternion from, Quaternion to)
    {
        // Calculate rotation around the wheel's up axis
        // This is appropriate for a horizontal wheel that rotates the artillery base
        Vector3 fromDirection = from * Vector3.forward;
        Vector3 toDirection = to * Vector3.forward;
        
        // Remove any vertical component to focus on horizontal rotation
        fromDirection.y = 0;
        toDirection.y = 0;
        
        fromDirection.Normalize();
        toDirection.Normalize();
        
        // Calculate the angle between the two directions
        return Vector3.SignedAngle(fromDirection, toDirection, Vector3.up);
    }
}
