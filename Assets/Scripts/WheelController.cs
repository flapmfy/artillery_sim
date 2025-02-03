using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LeverControlledWheel : MonoBehaviour
{
    private Quaternion initialLeverRotation;
    [Header("References")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable leverGrabInteractable;
    public Transform wheelTransform;
    public Transform controlledPart; // Artillery base or barrel
    
    [Header("Settings")]
    public Vector3 rotationAxis = Vector3.up;
    public float rotationMultiplier = 1f;
    public float maxAngle = 360f;
    public float minAngle = -360f;

    private Vector3 initialGrabDirection;
    private float currentAngle;
    private bool isGrabbed;

    void Start()
    {
        initialLeverRotation = leverGrabInteractable.transform.localRotation;
        leverGrabInteractable.selectEntered.AddListener(StartGrab);
        leverGrabInteractable.selectExited.AddListener(EndGrab);
    }

    void StartGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        initialGrabDirection = args.interactorObject.transform.position - wheelTransform.position;
    }

    void EndGrab(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    void Update()
    {
        if (!isGrabbed) return;

        if (isGrabbed)
        {
        var controller = leverGrabInteractable.interactorsSelecting[0]
            .transform.GetComponent<XRController>();
        controller.SendHapticImpulse(0.5f, 0.1f);
        }
        
        Vector3 currentGrabDirection = leverGrabInteractable.interactorsSelecting[0].transform.position 
                                     - wheelTransform.position;
        float angleDelta = Vector3.SignedAngle(
            initialGrabDirection, 
            currentGrabDirection, 
            rotationAxis
        );

        currentAngle = Mathf.Clamp(
            currentAngle + angleDelta * rotationMultiplier * Time.deltaTime,
            minAngle,
            maxAngle
        );

        // Apply rotation to wheel and controlled part
        wheelTransform.localRotation = Quaternion.AngleAxis(currentAngle, rotationAxis);
        controlledPart.localRotation = Quaternion.AngleAxis(currentAngle, rotationAxis);

        initialGrabDirection = currentGrabDirection;
    }

    void LateUpdate()
    {
        if (!isGrabbed)
        {
            // Maintain lever position relative to wheel
            leverGrabInteractable.transform.localRotation = initialLeverRotation;
        }
    }
}