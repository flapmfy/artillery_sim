using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class ElevationWheelController : MonoBehaviour
{
    [Header("Settings")]
    public Transform barrelPivot; // This is your barrel with the pivot already set in Blender
    public Vector2 elevationRange = new Vector2(0, 45);
    public float rotationSensitivity = 1.0f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Transform interactor;
    private Quaternion lastInteractorRotation;
    private float currentElevation;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        
        // Get the initial elevation angle
        currentElevation = barrelPivot.localEulerAngles.x;
        
        // Make sure elevation is within range
        if (currentElevation > 180)
            currentElevation -= 360; // Handle negative angles properly
        
        currentElevation = Mathf.Clamp(currentElevation, elevationRange.x, elevationRange.y);
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

        // Calculate the rotation change
        Quaternion currentRotation = interactor.rotation;
        float rotationDelta = CalculateRotationChange(lastInteractorRotation, currentRotation);
        
        // Apply to elevation
        currentElevation = Mathf.Clamp(
            currentElevation + rotationDelta * rotationSensitivity,
            elevationRange.x,
            elevationRange.y
        );

        // Apply to barrel
        barrelPivot.localRotation = Quaternion.Euler(currentElevation, 0, 0);
        
        // Store rotation for next frame
        lastInteractorRotation = currentRotation;
    }
    
    private float CalculateRotationChange(Quaternion from, Quaternion to)
    {
        // We'll calculate rotation primarily around the x-axis of the wheel
        // This assumes the wheel rotates around its local x-axis
        Vector3 fromVector = from * Vector3.up;
        Vector3 toVector = to * Vector3.up;
        
        // Use the wheel's right axis as the rotation reference
        return Vector3.SignedAngle(fromVector, toVector, transform.right);
    }
}
