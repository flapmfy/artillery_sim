using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors; // Added for clarity
using System.Collections; // Added for Coroutine

[RequireComponent(typeof(XRGrabInteractable))]
public class ChamberBlockController : MonoBehaviour
{
    [Header("Movement Constraints")]
    [Tooltip("The local axis along which the block slides (relative to its parent)")]
    public Vector3 slideAxis = Vector3.right; // Usually (1,0,0) for local X
    [Tooltip("Minimum distance along the slideAxis from the start position (e.g., 0 for closed)")]
    public float minSlideDistance = 0f;
    [Tooltip("Maximum distance along the slideAxis from the start position (e.g., 0.05 for open)")]
    public float maxSlideDistance = 0.02f; // Adjust based on your model

    [Header("Return Behaviour")]
    [Tooltip("Return to closed position (minSlideDistance) when released")]
    public bool returnToClosedWhenReleased = true;
    [Tooltip("Time taken to smoothly return to closed position")]
    public float returnDuration = 0.4f;

    // Components & State
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb; // Optional Rigidbody reference
    private Vector3 initialLocalPosition; // Store the starting point relative to parent
    private IXRSelectInteractor currentInteractor = null; // Store the specific interactor
    // Offset from block origin to hand attach point in the block's local space
    private Vector3 grabAttachOffsetLocal;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>(); // Get Rigidbody if present
        initialLocalPosition = transform.localPosition; // Store starting position

        // Normalize the slide axis just in case it wasn't entered as a unit vector
        slideAxis.Normalize();

        // --- Configure Interactable ---
        // Instantaneous or Kinematic are best suited when manually controlling/constraining position.
        // Let's stick with Instantaneous as you had it, but Kinematic is also a good choice.
        grabInteractable.movementType = XRBaseInteractable.MovementType.Instantaneous;

        // IMPORTANT: If using Instantaneous or Kinematic, the Rigidbody (if present)
        // MUST be marked IsKinematic to avoid conflicts with physics.
        if (rb != null)
        {
            rb.isKinematic = true;
            // Consider collision detection mode if needed (e.g., Continuous Speculative)
        }
        else
        {
            Debug.LogWarning("ChamberBlockController: No Rigidbody found. Movement might behave unexpectedly without one, especially regarding collisions.", this);
        }
    }

    // Use OnEnable/OnDisable for listeners - more robust
    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabStart);
        grabInteractable.selectExited.AddListener(OnGrabEnd);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabStart);
        grabInteractable.selectExited.RemoveListener(OnGrabEnd);
        // Ensure coroutine stops if object is disabled while returning
        StopAllCoroutines();
    }

    private void OnGrabStart(SelectEnterEventArgs args)
    {
        // Check if the interactor is valid
        if (args.interactorObject is IXRSelectInteractor interactor)
        {
            currentInteractor = interactor;

            // Calculate the offset from the object's origin to the grab point,
            // but store it in the object's local space. This helps maintain
            // the relative position of the hand on the object while sliding.
            Transform attachTransform = currentInteractor.GetAttachTransform(grabInteractable);
            grabAttachOffsetLocal = transform.InverseTransformPoint(attachTransform.position);

            // Stop any return movement if it was in progress
            StopAllCoroutines();
        }
    }

    private void OnGrabEnd(SelectExitEventArgs args)
    {
        // Only process if the releasing interactor is the one currently holding it
        if (args.interactorObject == currentInteractor)
        {
            currentInteractor = null; // Clear the interactor reference

            // Start the return coroutine if enabled and the object is active
            if (returnToClosedWhenReleased && gameObject.activeInHierarchy)
            {
                StartCoroutine(SmoothMoveToTargetDistance(minSlideDistance));
            }
        }
    }

    // Use LateUpdate to apply constraints *after* the XR Interaction Toolkit
    // has processed its updates for the frame.
    void LateUpdate()
    {
        if (currentInteractor != null)
        {
            // --- Calculate Constrained Position ---

            // 1. Get the world position where the hand/interactor is trying to move the grab point to.
            Transform attachTransform = currentInteractor.GetAttachTransform(grabInteractable);

            // 2. Calculate where the object's *origin* should be in world space to satisfy the hand's position,
            // considering the initial grab offset.
            Vector3 desiredWorldPosition = attachTransform.position - transform.TransformDirection(grabAttachOffsetLocal);

            // 3. Convert this desired world position into the coordinate system of the object's parent.
            // If there's no parent, use world space directly.
            Vector3 desiredLocalPosition = transform.parent != null ?
                                           transform.parent.InverseTransformPoint(desiredWorldPosition) :
                                           desiredWorldPosition;

            // 4. Calculate the vector representing the total desired movement from the initial position.
            Vector3 movementVector = desiredLocalPosition - initialLocalPosition;

            // 5. Project this total movement vector onto the allowed slide axis.
            // This finds the component of the movement that is along the allowed direction.
            Vector3 projectedMovement = Vector3.Project(movementVector, slideAxis);

            // 6. Calculate the signed distance along the slide axis. Dot product gives magnitude and sign.
            float currentDistance = Vector3.Dot(projectedMovement, slideAxis);

            // 7. Clamp this distance within the defined limits.
            float clampedDistance = Mathf.Clamp(currentDistance, minSlideDistance, maxSlideDistance);

            // 8. Calculate the final constrained local position by starting at the initial position
            // and adding the allowed movement along the slide axis.
            Vector3 constrainedLocalPosition = initialLocalPosition + slideAxis * clampedDistance;

            // 9. Apply the constrained position.
            transform.localPosition = constrainedLocalPosition;
        }
    }

    // Coroutine for smooth return movement
    IEnumerator SmoothMoveToTargetDistance(float targetDistance)
    {
        // Calculate the target local position based on the distance
        Vector3 targetLocalPosition = initialLocalPosition + slideAxis * targetDistance;
        Vector3 startLocalPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < returnDuration)
        {
            // Use SmoothStep for a nicer ease-in/ease-out effect
            float t = elapsedTime / returnDuration;
            t = t * t * (3f - 2f * t); // Smoothstep formula

            transform.localPosition = Vector3.Lerp(startLocalPosition, targetLocalPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure the final position is exactly the target
        transform.localPosition = targetLocalPosition;
    }

    // --- Keep your optional/utility methods ---
    public void PlaySoundEffect(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }

    public void SimulateMove(float normalizedPosition) // NormalizedPosition 0=min, 1=max
    {
        float distance = Mathf.Lerp(minSlideDistance, maxSlideDistance, normalizedPosition);
        transform.localPosition = initialLocalPosition + slideAxis * distance;
    }
}
