using System.Collections;
using UnityEngine;

public class Respawnable : MonoBehaviour
{
    public bool respawnOnGround;

    public GameObject respawnParticleEffect; // Optional particle effect for respawn
    public AudioClip respawnSound; // Optional sound effect for respawn

    private AudioSource audioSource;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    [SerializeField]
    private float respawnDelay = 1f;

    private Coroutine respawnCoroutine;
    private Rigidbody rb;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null && respawnSound != null)
        {
            // Add an AudioSource automatically if not present
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Respawn()
    {
        if (respawnParticleEffect != null)
        {
            Instantiate(respawnParticleEffect, transform.position, Quaternion.identity);
        }

        // Play sound at the *current* world position, not tied to object movement
        if (respawnSound != null)
        {
            AudioSource.PlayClipAtPoint(respawnSound, transform.position);
        }

        // Reset transform
        transform.SetPositionAndRotation(initialPosition, initialRotation);

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Respawn") && respawnCoroutine == null)
        {
            respawnCoroutine = StartCoroutine(RespawnAfterDelay());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Respawn") && respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (respawnOnGround && collision.gameObject.CompareTag("Ground") && respawnCoroutine == null)
        {
            respawnCoroutine = StartCoroutine(RespawnAfterDelay());
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
        respawnCoroutine = null;
    }
}
