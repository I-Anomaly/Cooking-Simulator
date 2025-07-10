using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttableObject : MonoBehaviour
{
    public GameObject HalfsPrefab;
    public bool isCut;

    AudioSource audioSource; // Reference to the AudioSource component for sound effects
    public AudioClip[] cuttingSounds; // Array to hold different cutting sound effects

    ProgressionManager progressionManager;

    private void Start()
    {
        progressionManager = GetComponent<ProgressionManager>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name.Contains("Knife") && !isCut)
        {
            GameObject temp = GameObject.Instantiate(HalfsPrefab,transform.position,transform.rotation);
            temp = GameObject.Instantiate(HalfsPrefab,transform.position, transform.rotation);
            temp.transform.Rotate(Vector3.up * 180);
            if (audioSource != null && cuttingSounds.Length > 0)
            {
                // Play a random cutting sound from the array
                int randomIndex = Random.Range(0, cuttingSounds.Length);
                audioSource.PlayOneShot(cuttingSounds[randomIndex]);
            }
            if (progressionManager != null)
                progressionManager.TryProgressStep(); // Attempt to progress the recipe step
            isCut = true;
            Destroy(this.gameObject);

        }
    }
}
