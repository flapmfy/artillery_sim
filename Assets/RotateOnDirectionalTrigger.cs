using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnDirectionalTrigger : MonoBehaviour
{
    public GameObject targetObject; // The object you want to rotate
    public float rotationSpeed = 25f; // Rotation speed (degrees per second)

    // Update is called once per frame
    void Update()
    {
        // We don't need to check anything in Update because rotation is handled in triggers.
    }

    // Function to rotate the object
    void RotateObject(Vector3 direction)
    {
        // Rotate the object based on the provided direction
        targetObject.transform.Rotate(direction * rotationSpeed * Time.deltaTime);
    }

    // This method is triggered when the player enters any of the triggers
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger is tagged as "Player"
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered " + gameObject.name);
            // Determine which trigger was entered and apply rotation accordingly
            if (gameObject.name == "Up")
            {
                RotateObject(Vector3.right); // Rotate upwards
            }
            else if (gameObject.name == "Down")
            {
                RotateObject(Vector3.left); // Rotate downwards
            }
            else if (gameObject.name == "Left")
            {
                RotateObject(Vector3.down); // Rotate to the left
            }
            else if (gameObject.name == "Right")
            {
                RotateObject(Vector3.up); // Rotate to the right
            }
        }
    }
}