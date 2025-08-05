using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttableObject : MonoBehaviour
{
    public GameObject HalfsPrefab;
    public bool isCut;

    [Header("Chop Sound Settings")]
    public AudioClip[] chopSounds; // Assign 3 cutting sounds in Inspector

    ProgressionManager progressionManager;

    private void Start()
    {
        progressionManager = GetComponent<ProgressionManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("Knife") && !isCut)
        {
            // Play random chopping sound at object's position
            if (chopSounds != null && chopSounds.Length > 0)
            {
                int index = Random.Range(0, chopSounds.Length);
                AudioSource.PlayClipAtPoint(chopSounds[index], transform.position);
            }

            // Instantiate chopped halves
            GameObject temp = Instantiate(HalfsPrefab, transform.position, transform.rotation);
            temp = GameObject.Instantiate(HalfsPrefab, transform.position, transform.rotation);
            temp.transform.Rotate(Vector3.up * 180);

            // Trigger next recipe step
            if (progressionManager != null)
                progressionManager.TryProgressStep();

            isCut = true;
            Destroy(this.gameObject);
        }
    }
}
