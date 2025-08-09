using UnityEngine;

public class FireFaceUp : MonoBehaviour
{
    void Update()
    {
        // Get the world up direction
        Vector3 worldUp = Vector3.up;

        // Calculate the rotation to align the fire's up direction with the world up
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, worldUp) * transform.rotation;

        // Apply the rotation to the fire
        transform.rotation = targetRotation;
    }
}
