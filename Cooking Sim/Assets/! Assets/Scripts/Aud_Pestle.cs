using UnityEngine;

public class PoundingSoundTrigger : MonoBehaviour
{
    public AudioClip[] poundingClips;
    public AudioSource audioSource;
    public string targetTag = "Mortar"; // Tag of the mortar

    public float minDelay = 0.1f;  // Optional cooldown between sounds
    private float lastPlayTime;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;

        if (Time.time - lastPlayTime < minDelay) return;

        if (poundingClips.Length > 0 && audioSource != null)
        {
            audioSource.clip = poundingClips[Random.Range(0, poundingClips.Length)];
            audioSource.pitch = Random.Range(0.95f, 1.05f); // Optional variation
            audioSource.Play();
            lastPlayTime = Time.time;
        }
    }
}
