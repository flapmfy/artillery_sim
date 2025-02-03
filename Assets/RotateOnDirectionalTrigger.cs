using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GridBrushBase;

public class RotateOnDirectionalTrigger : MonoBehaviour
{
    public GameObject targetObject; // The object you want to rotate
    public float rotationSpeed = 25f; // Rotation speed (degrees per second)

    private bool isPlayerInTrigger = false; // Flag to check if player is inside the trigger
    private Vector3 rotationDirection = Vector3.zero; // Rotation direction

    // Update is called once per frame
    void Update()
    {
        if (isPlayerInTrigger)
        {
            // Rotate the target object while keeping the Z rotation fixed
            Vector3 currentRotation = targetObject.transform.rotation.eulerAngles;

            // Apply the desired rotation only on X and Y axes, keep Z fixed
            float newRotationX = currentRotation.x + rotationDirection.x * rotationSpeed * Time.deltaTime;
            float newRotationY = currentRotation.y + rotationDirection.y * rotationSpeed * Time.deltaTime;
            float newRotationZ = currentRotation.z; // Keep the current Z value fixed

            targetObject.transform.rotation = Quaternion.Euler(newRotationX, newRotationY, newRotationZ);
        }
    }

    // This method is triggered when the player enters any of the triggers
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger is tagged as "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            Debug.Log("Player entered " + gameObject.name);
            // Determine which trigger was entered and apply rotation accordingly
            if (gameObject.name == "Up")
            {
                rotationDirection = Vector3.right; // Rotate upwards
            }
            else if (gameObject.name == "Down")
            {
                rotationDirection = Vector3.left; // Rotate downwards
            }
            else if (gameObject.name == "Left")
            {
                rotationDirection = Vector3.down; // Rotate to the left
            }
            else if (gameObject.name == "Right")
            {
                rotationDirection = Vector3.up; // Rotate to the right
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        // Check if the player exits the trigger area
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            rotationDirection = Vector3.zero; // Stop rotation when the player exits
        }
    }
}