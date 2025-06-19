using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawnable : MonoBehaviour
{
    public GameObject respawnParticleEffect; // Optional particle effect for respawn

    // Store the object's original position and rotation
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // Set a threshold distance. If the object is farther than this
    // from its starting point, it will respawn.
    [SerializeField]
    private float respawnDistanceThreshold = 15f;  // Adjust as needed

    // Optional delay (in seconds) before the respawn occurs
    [SerializeField]
    private float respawnDelay = 1f;

    void Start()
    {
        // Record the initial position and rotation when the game starts
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // Check if the object has drifted too far from its original spawn point
        if (Vector3.Distance(transform.position, initialPosition) > respawnDistanceThreshold)
        {
            // Optionally add a delay for the respawn
            if (respawnDelay > 0f)
                Invoke("Respawn", respawnDelay);
            else
                Respawn();
        }
    }

    // Resets the object's position, rotation, and physics (if applicable)
    void Respawn()
    {
        if (respawnParticleEffect != null)
        {
            // Instantiate a particle effect at the object's current position
            Instantiate(respawnParticleEffect, transform.position, Quaternion.identity);
        }

        // Reset position and rotation
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // If the object has a Rigidbody, reset its velocity to avoid continued motion
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }



    private void OnTriggerExit(Collider other)
    {
        // Check if the object has left a designated respawn zone
        if (other.CompareTag("Respawn"))
        {
            Respawn();
        }
    }
}
