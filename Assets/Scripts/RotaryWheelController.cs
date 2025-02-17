using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class RotaryWheelController : MonoBehaviour
{
    [Header("Settings")]
    public Transform artilleryBase;
    public float rotationSpeed = 2.0f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Transform interactor;
    private Vector3 previousPosition;
    private float currentRotation;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
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
            transform.up
        );

        artilleryBase.Rotate(Vector3.up, angleDelta * rotationSpeed);
        previousPosition = currentPosition;
    }
}