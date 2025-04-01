using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Namespace updated for newer XRIT versions
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Namespace for XRGrabInteractable
using UnityEngine.XR.Interaction.Toolkit.Interactors; // Namespace for XRBaseInteractor

[RequireComponent(typeof(XRGrabInteractable))]
public class ElevationWheelController : MonoBehaviour
{
    [Header("Target Object")]
    [Tooltip("The Transform to rotate for elevation (e.g., the barrel pivot)")]
    public Transform barrelPivot;

    [Header("Rotation Settings")]
    [Tooltip("The local axis around which this wheel visually rotates (e.g., Vector3.forward for Z)")]
    public Vector3 localRotationAxis = Vector3.forward; // Common for wheels
    [Tooltip("Min/Max elevation angle for the barrelPivot (degrees)")]
    public Vector2 elevationRange = new Vector2(0, 45);
    [Tooltip("How many degrees the barrel elevates per degree the wheel is turned")]
    public float sensitivity = 0.5f; // Adjust this for feel

    [Header("Visual Feedback (Optional)")]
    [Tooltip("If assigned, this wheel transform will visually rotate")]
    public Transform wheelVisualTransform; // Assign the wheel mesh transform here

    private XRGrabInteractable grabInteractable;
    private IXRSelectInteractor currentInteractor = null; // Store the specific interactor
    private Vector3 initialGrabPositionLocal = Vector3.zero; // Interactor pos relative to wheel center
    private float currentElevationAngle = 0f;
    private float currentWheelVisualAngle = 0f; // For visual rotation

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (barrelPivot == null)
        {
            Debug.LogError("ElevationWheelController: Barrel Pivot is not assigned!", this);
            enabled = false; // Disable script if target is missing
            return;
        }

        // Use the barrel's initial rotation as the starting elevation
        currentElevationAngle = GetClampedInitialElevation();

        // Apply initial elevation immediately
        ApplyElevation();

        // If no specific visual transform assigned, use this object's transform
        if (wheelVisualTransform == null)
        {
            wheelVisualTransform = transform;
        }
        // Initialize visual rotation based on elevation (optional, depends on setup)
        // currentWheelVisualAngle = currentElevationAngle / sensitivity; // Or some initial value
        // ApplyWheelVisualRotation();

    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabStart);
        grabInteractable.selectExited.AddListener(OnGrabEnd);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabStart);
        grabInteractable.selectExited.RemoveListener(OnGrabEnd);
    }

    private float GetClampedInitialElevation()
    {
        // Assuming elevation is primarily around the X axis of the barrelPivot
        float initialAngle = barrelPivot.localEulerAngles.x;

        // Convert angle range to -180 to 180 if necessary for comparison
        if (initialAngle > 180f)
        {
            initialAngle -= 360f;
        }

        return Mathf.Clamp(initialAngle, elevationRange.x, elevationRange.y);
    }


    private void OnGrabStart(SelectEnterEventArgs args)
    {
        // XRBaseInteractor provides the attachTransform
        if (args.interactorObject is IXRSelectInteractor interactor)
        {
            currentInteractor = interactor;
            // Calculate the initial vector from wheel center to hand attach point
            // in the wheel's local space, projected onto the rotation plane.
            initialGrabPositionLocal = CalculateLocalProjectedPosition(currentInteractor.GetAttachTransform(grabInteractable).position);
        }
         // Ensure kinematic while grabbed if not already set (good practice)
        // grabInteractable.GetComponent<Rigidbody>().isKinematic = true;
    }

    private void OnGrabEnd(SelectExitEventArgs args)
    {
        if (args.interactorObject == currentInteractor) // Ensure it's the same interactor releasing
        {
            currentInteractor = null;
        }
        // Consider setting Rigidbody back to non-kinematic if needed after release
        // grabInteractable.GetComponent<Rigidbody>().isKinematic = false;
    }

    void Update()
    {
        if (currentInteractor != null)
        {
            // Calculate the current vector from wheel center to hand
            Vector3 currentGrabPositionLocal = CalculateLocalProjectedPosition(currentInteractor.GetAttachTransform(grabInteractable).position);

            // Calculate the angle difference around the rotation axis
            float angleDelta = Vector3.SignedAngle(
                initialGrabPositionLocal, // Vector when grab started/last frame
                currentGrabPositionLocal, // Vector now
                localRotationAxis         // Axis to measure rotation around
            );

            // Only process if there's a meaningful angle change
            if (Mathf.Abs(angleDelta) > 0.01f) // Small threshold to avoid jitter
            {
                // --- Update Elevation ---
                float elevationChange = angleDelta * sensitivity;
                currentElevationAngle = Mathf.Clamp(
                    currentElevationAngle + elevationChange,
                    elevationRange.x,
                    elevationRange.y
                );
                ApplyElevation();

                // --- Update Wheel Visual ---
                currentWheelVisualAngle += angleDelta; // Accumulate visual rotation
                ApplyWheelVisualRotation();


                // Update the reference position for the next frame's delta calculation
                // This makes the rotation continuous instead of snapping back to the initial grab
                 initialGrabPositionLocal = currentGrabPositionLocal;
            }
        }
    }

    // Calculates the interactor's position relative to the wheel's center,
    // projected onto the plane perpendicular to the localRotationAxis.
    private Vector3 CalculateLocalProjectedPosition(Vector3 interactorWorldPosition)
    {
        // 1. Get world position relative to wheel center
        Vector3 worldOffset = interactorWorldPosition - transform.position;

        // 2. Convert world offset to wheel's local space
        Vector3 localOffset = transform.InverseTransformDirection(worldOffset);

        // 3. Project onto the plane perpendicular to the local rotation axis
        // This effectively removes any component along the rotation axis itself
        Vector3 projectedLocalOffset = Vector3.ProjectOnPlane(localOffset, localRotationAxis);

        return projectedLocalOffset;
    }

    // Applies the currentElevationAngle to the barrelPivot
    private void ApplyElevation()
    {
        if (barrelPivot != null)
        {
            // Get current rotation, only change X
            Vector3 currentLocalEuler = barrelPivot.localEulerAngles;
            barrelPivot.localRotation = Quaternion.Euler(currentElevationAngle, currentLocalEuler.y, currentLocalEuler.z);
        }
    }

    // Applies the currentWheelVisualAngle to the wheel visual
    private void ApplyWheelVisualRotation()
    {
         if (wheelVisualTransform != null)
         {
            // Rotate the wheel visual around its defined local axis
            wheelVisualTransform.localRotation = Quaternion.AngleAxis(currentWheelVisualAngle, localRotationAxis);
         }
    }
}
