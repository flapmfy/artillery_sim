using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class ElevationWheelController : MonoBehaviour
{
    [Header("Settings")]
    public Transform barrelPivot;
    public Vector2 elevationRange = new Vector2(0, 45);
    public float elevationSpeed = 1.0f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Transform interactor;
    private Vector3 previousPosition;
    private float currentElevation;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        currentElevation = barrelPivot.localEulerAngles.x;
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        interactor = args.interactorObject.transform;
        previousPosition = interactor.position;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        interactor = null;
    }

    void Update()
    {
        if (!interactor) return;

        Vector3 currentPosition = interactor.position;
        Vector3 direction = currentPosition - transform.position;
        Vector3 prevDirection = previousPosition - transform.position;

        float angleDelta = Vector3.SignedAngle(
            prevDirection, 
            direction, 
            transform.right
        );

        currentElevation = Mathf.Clamp(
            currentElevation + angleDelta * elevationSpeed,
            elevationRange.x,
            elevationRange.y
        );

        barrelPivot.localRotation = Quaternion.Euler(currentElevation, 0, 0);
        previousPosition = currentPosition;
    }
}