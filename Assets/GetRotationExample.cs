using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GridBrushBase;

public class GetRotationExample : MonoBehaviour
{
    float targetX = 0;
    float targetY = 0;

    bool xCorrect = false;
    bool yCorrect = false;

    private RotateOnDirectionalTrigger Triggers;

    void Start()
    {
        // Target Values
        targetX = UnityEngine.Random.Range(20, 80);
        targetY = UnityEngine.Random.Range(0, 180);
    }
    void Update()
    {
        

        // Get the rotation in Euler angles (degrees)
        Vector3 eulerRotation = transform.rotation.eulerAngles;

        // Get the X and Y rotation values
        float rotationX = eulerRotation.x;
        float rotationY = eulerRotation.y;

        if (targetX - rotationX < 1 && targetX - rotationX > -1)
        {
            xCorrect = true;
        }
        else
        {
            xCorrect = false;
        }

        if (targetY - rotationY < 1 && targetY - rotationY > -1)
        {
            yCorrect = true;
        }
        else
        {
            yCorrect = false;
        }

        if (Result(xCorrect, yCorrect) == "BOTH CORRECT")
        {
            NewTarget();
        }

        // Print the rotation values to the console
        Debug.Log(Result(xCorrect, yCorrect));

        Debug.Log("Target X: " + targetX);
        Debug.Log("Target Y: " + targetY);

        Debug.Log("Rotation X: " + rotationX);
        Debug.Log("Rotation Y: " + rotationY);
    }
    String Result(bool a, bool b)
    {
        if (a == true && b == true)
        {
            return "BOTH CORRECT";
        }
        else if (a == true)
        {
            return "X CORRECT";
        }
        else if (b == true)
        {
            return "Y CORRECT";
        }
        else
        {
            return "Incorrect";
        }
    }
    void NewTarget()
    {
        // Target Values
        targetX = UnityEngine.Random.Range(20, 80);
        targetY = UnityEngine.Random.Range(0, 180);
    }
}