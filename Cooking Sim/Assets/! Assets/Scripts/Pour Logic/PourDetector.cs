using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PourDetector : MonoBehaviour
{
    public float pouringThreshold = 0.75f; // Threshold to detect pouring
    public Transform pourPoint; // Point where the liquid is poured

    public Stream streamPrefab; // Prefab for the stream effect
    private Stream currentStream;

    float maxFill = 1f; // Maximum fill level of the object
    public float fillLevel = 1f; // Current fill level of the object, 1 means full, 0 means empty
    public float drainRate = 0.1f; // Rate at which the object drains, 10% per second
    public float currentThreshold; // Current threshold for the fill level

    bool isPouring = false;

    // Update is called once per frame
    void Update()
    {
        currentThreshold = Mathf.Lerp(-1f, pouringThreshold, fillLevel); // Adjust the threshold based on the fill level

        bool pourCheck = CalculateDotProduct() < currentThreshold;

        // if (pourCheck) Debug.Log("POUR WATER");
        if (isPouring)
        {
            // fillLevel -= drainRate * Time.deltaTime; // Drain the fill level based on the drain rate and time
            fillLevel = Mathf.Clamp(fillLevel, 0f, maxFill); // Ensure the fill level does not exceed the maximum or go below zero
        }

        if (isPouring != pourCheck)
        {
            isPouring = pourCheck;
            if (isPouring)
            {
                currentStream = Instantiate<Stream>(streamPrefab, pourPoint.position, Quaternion.identity, transform);
                currentStream.Begin(); // Start the stream effect
            }
            else
            {
                currentStream.End(); // End the stream effect
                currentStream = null; // Clear the reference to the current stream
            }
        }
    }

    private float CalculateDotProduct()
    {
        // Check the dot product between the object's up direction and the world up direction, if it passes a certain threshold, regardless of the rotation, we consider it pouring
        Vector3 upDirection = Vector3.up;
        float dotProduct = Vector3.Dot(transform.up, upDirection);
        return dotProduct;
    }

    private void Refill()
    {
        fillLevel = maxFill; // Reset the fill level to maximum
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(150, 10, 100, 20), "Refill"))
        {
            Refill(); // Button to refill the object
        }
    }
}
