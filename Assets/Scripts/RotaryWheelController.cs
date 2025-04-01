using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Namespace updated
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class RotationWheelController : MonoBehaviour // Renamed class
{
    [Header("Target Object")]
    [Tooltip("The Transform to rotate horizontally (e.g., the artillery base)")]
    public Transform artilleryBase; // Renamed variable

    [Header("Rotation Settings")]
    [Tooltip("The local axis around which this wheel visually rotates (e.g., Vector3.forward for Z)")]
    public Vector3 localRotationAxis = Vector3.forward; // Axis the *wheel* spins on
    [Tooltip("How many degrees the base rotates per degree the wheel is turned")]
    public float sensitivity = 0.5f; // Adjust this for feel
    [Tooltip("Enable limits for the base rotation?")]
    public bool useRotationLimits = false;
    [Tooltip("Min/Max rotation angle for the artilleryBase (degrees) around Y-axis")]
    public Vector2 rotationLimits = new Vector2(-90, 90); // Example limits

    [Header("Visual Feedback (Optional)")]
    [Tooltip("If assigned, this wheel transform will visually rotate")]
    public Transform wheelVisualTransform; // Assign the wheel mesh transform here

    private XRGrabInteractable grabInteractable;
    private IXRSelectInteractor currentInteractor = null;
    private Vector3 initialGrabPositionLocal = Vector3.zero;
    private float currentRotationAngle = 0f; // Tracks the base's target Y rotation
    private float currentWheelVisualAngle = 0f; // For visual rotation

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (artilleryBase == null)
        {
            Debug.LogError("RotationWheelController: Artillery Base is not assigned!", this);
            enabled = false;
            return;
        }

        // Use the base's initial Y rotation as the starting angle
        currentRotationAngle = GetInitialRotation();

        // Apply initial rotation immediately
        ApplyRotation();

        if (wheelVisualTransform == null)
        {
            wheelVisualTransform = transform;
        }
        // Initialize visual rotation (optional)
        // currentWheelVisualAngle = currentRotationAngle / sensitivity;
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

    private float GetInitialRotation()
    {
        // Assuming rotation is primarily around the Y axis of the artilleryBase
        float initialAngle = artilleryBase.localEulerAngles.y;

        // Convert angle range to -180 to 180 if necessary, especially if using limits
        if (initialAngle > 180f)
        {
            initialAngle -= 360f;
        }

        // If using limits, clamp the initial angle immediately
        if (useRotationLimits)
        {
             initialAngle = Mathf.Clamp(initialAngle, rotationLimits.x, rotationLimits.y);
        }
        // If not using limits, the raw angle (potentially >180 or <-180 after wrapping) is fine

        return initialAngle;
    }


    private void OnGrabStart(SelectEnterEventArgs args)
    {
        if (args.interactorObject is IXRSelectInteractor interactor)
        {
            currentInteractor = interactor;
            initialGrabPositionLocal = CalculateLocalProjectedPosition(currentInteractor.GetAttachTransform(grabInteractable).position);
        }
    }

    private void OnGrabEnd(SelectExitEventArgs args)
    {
        if (args.interactorObject == currentInteractor)
        {
            currentInteractor = null;
        }
    }

    void Update()
    {
        if (currentInteractor != null)
        {
            Vector3 currentGrabPositionLocal = CalculateLocalProjectedPosition(currentInteractor.GetAttachTransform(grabInteractable).position);

            float angleDelta = Vector3.SignedAngle(
                initialGrabPositionLocal,
                currentGrabPositionLocal,
                localRotationAxis // Use the wheel's rotation axis
            );

            if (Mathf.Abs(angleDelta) > 0.01f)
            {
                // --- Update Base Rotation Angle ---
                float rotationChange = angleDelta * sensitivity;
                currentRotationAngle += rotationChange; // Accumulate rotation

                // Apply limits ONLY if enabled
                if (useRotationLimits)
                {
                    currentRotationAngle = Mathf.Clamp(
                        currentRotationAngle,
                        rotationLimits.x,
                        rotationLimits.y
                    );
                }
                // If no limits, currentRotationAngle can increase/decrease indefinitely

                ApplyRotation(); // Apply the calculated angle to the base

                // --- Update Wheel Visual ---
                currentWheelVisualAngle += angleDelta; // Accumulate visual rotation
                ApplyWheelVisualRotation();

                // Update reference position for continuous rotation
                initialGrabPositionLocal = currentGrabPositionLocal;
            }
        }
    }

    // Calculates the interactor's position relative to the wheel's center,
    // projected onto the plane perpendicular to the localRotationAxis.
    private Vector3 CalculateLocalProjectedPosition(Vector3 interactorWorldPosition)
    {
        Vector3 worldOffset = interactorWorldPosition - transform.position;
        Vector3 localOffset = transform.InverseTransformDirection(worldOffset);
        Vector3 projectedLocalOffset = Vector3.ProjectOnPlane(localOffset, localRotationAxis);
        return projectedLocalOffset;
    }

    // Applies the currentRotationAngle to the artilleryBase's Y-axis
    private void ApplyRotation()
    {
        if (artilleryBase != null)
        {
            // Get current rotation, only change Y
            Vector3 currentLocalEuler = artilleryBase.localEulerAngles;
            artilleryBase.localRotation = Quaternion.Euler(currentLocalEuler.x, currentRotationAngle, currentLocalEuler.z);
        }
    }

    // Applies the currentWheelVisualAngle to the wheel visual
    private void ApplyWheelVisualRotation()
    {
         if (wheelVisualTransform != null)
         {
            wheelVisualTransform.localRotation = Quaternion.AngleAxis(currentWheelVisualAngle, localRotationAxis);
         }
    }
}
