using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ChamberBlockController : MonoBehaviour
{
    [Header("Movement Constraints")]
    [Tooltip("Minimum X position (closed position)")]
    public float minXPosition = 0.014f;
    
    [Tooltip("Maximum X position (open position)")]
    public float maxXPosition = 0.02f;
    
    [Header("Physics Settings")]
    [Tooltip("Movement smoothing factor")]
    public float smoothing = 10f;
    
    [Tooltip("Return to closed position when released")]
    public bool returnToClosedWhenReleased = true;
    
    // Components
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Vector3 originalLocalPosition;
    private bool isGrabbed = false;
    private Transform originalParent;
    
    void Awake()
    {
        // Get the XR Grab Interactable component
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        // Store the original position
        originalLocalPosition = transform.localPosition;
        originalParent = transform.parent;
        
        // Configure the interactable
        if (grabInteractable != null)
        {
            // Use a custom movement type that only allows movement along X axis
            grabInteractable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.VelocityTracking;
            
            // Subscribe to interaction events
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }
    
    void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }
    
    void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        
        // Return to closed position if enabled
        if (returnToClosedWhenReleased)
        {
            // Create a vector with only the Y and Z from the original position
            Vector3 closedPosition = originalLocalPosition;
            closedPosition.x = minXPosition;
            StartCoroutine(SmoothMoveTo(closedPosition));
        }
    }
    
    void Update()
    {
        if (isGrabbed)
        {
            // Constrain movement to X-axis within limits
            Vector3 currentLocalPos = transform.localPosition;
            float clampedX = Mathf.Clamp(currentLocalPos.x, minXPosition, maxXPosition);
            
            // Only change the X value, keep original Y and Z
            Vector3 constrainedPosition = new Vector3(
                clampedX,
                originalLocalPosition.y,
                originalLocalPosition.z
            );
            
            transform.localPosition = constrainedPosition;
        }
    }
    
    System.Collections.IEnumerator SmoothMoveTo(Vector3 targetLocalPosition)
    {
        float elapsedTime = 0;
        Vector3 startingPos = transform.localPosition;
        
        while (elapsedTime < 1.0f)
        {
            transform.localPosition = Vector3.Lerp(startingPos, targetLocalPosition, elapsedTime * smoothing);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.localPosition = targetLocalPosition;
    }
    
    // Optional: Add sound effects
    public void PlaySoundEffect(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
    
    // For testing in editor
    public void SimulateMove(float normalizedPosition)
    {
        float position = Mathf.Lerp(minXPosition, maxXPosition, normalizedPosition);
        transform.localPosition = new Vector3(
            position,
            originalLocalPosition.y,
            originalLocalPosition.z
        );
    }
}