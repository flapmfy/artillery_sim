using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnButtonPress : MonoBehaviour

{
    public float rotationSpeed = 100f; // Rotation speed (degrees per second)

    private bool isInTrigger = false;

    // Update is called once per frame
    void Update()
    {
        // Rotate the object if the player is inside the trigger area
        if (isInTrigger)
        {
            RotateObject();
        }
    }

    // Function to rotate the object
    void RotateObject()
    {
        // Rotate the object around its Y-axis by the specified rotation speed
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    // This method is triggered when another collider enters the trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger area
        if (other.CompareTag("Player"))
        {
            isInTrigger = true; // Start rotating the object
        }
    }

    // This method is triggered when another collider exits the trigger collider    
    private void OnTriggerExit(Collider other)
    {
        // Check if the player left the trigger area
        if (other.CompareTag("Player"))
        {
            isInTrigger = false; // Stop rotating the object
        }
    }
}

