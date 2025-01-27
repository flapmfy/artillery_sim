using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RotateOnTouch : MonoBehaviour
{
    // The object to rotate
    public GameObject objectToRotate;
    // The rotation speed (degrees per second)
    public float rotationSpeed = 50f;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that collided is the player or specific object
        if (other.CompareTag("Player"))
        {
            RotateObject();
        }
    }

    private IEnumerator RotateObject()
    {
        // Rotate the object as long as the player stays in contact
        while (true)
        {
            objectToRotate.transform.Rotate(Vector3.left * rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }
}

