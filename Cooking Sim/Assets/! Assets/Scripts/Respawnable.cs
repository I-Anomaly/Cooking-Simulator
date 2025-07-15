using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawnable : MonoBehaviour
{
    public GameObject respawnParticleEffect; // Optional particle effect for respawn
    public AudioClip respawnSound; // Optional sound effect for respawn

    AudioSource audioSource; // Reference to the AudioSource component, if applicable

    // Store the object's original position and rotation
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // Optional delay (in seconds) before the respawn occurs
    [SerializeField]
    private float respawnDelay = 1f;

    private Coroutine respawnCoroutine; // Reference to the running coroutine

    private Rigidbody rb; // Reference to the Rigidbody component, if applicable

    void Start()
    {
        // Record the initial position and rotation when the game starts
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // If the object has a Rigidbody, reset its velocity to avoid continued motion
        rb = GetComponent<Rigidbody>();

        audioSource = GetComponent<AudioSource>();
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
        transform.SetPositionAndRotation(initialPosition, initialRotation);
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (audioSource != null && respawnSound != null)
        {
            // Play the respawn sound effect
            audioSource.PlayOneShot(respawnSound);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the object has left a designated respawn zone
        if (other.CompareTag("Respawn"))
        {
            Debug.Log("Hit the respawn zone, respawning object.");
            if (respawnCoroutine == null)
                respawnCoroutine = StartCoroutine(RespawnAfterDelay());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Respawn"))
        {
            // Cancel the respawn if the object re-enters before the delay is up
            if (respawnCoroutine != null)
            {
                StopCoroutine(respawnCoroutine);
                respawnCoroutine = null;
                Debug.Log("Respawn cancelled, object re-entered the zone.");
            }
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
        respawnCoroutine = null; // Reset the coroutine reference
    }
}
