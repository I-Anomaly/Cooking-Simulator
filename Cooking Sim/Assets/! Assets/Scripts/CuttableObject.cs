using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttableObject : MonoBehaviour
{
    public GameObject HalfsPrefab;
    public bool isCut;

    ProgressionManager progressionManager;

    private void Start()
    {
        progressionManager = GetComponent<ProgressionManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name.Contains("Knife") && !isCut)
        {
            GameObject temp = GameObject.Instantiate(HalfsPrefab,transform.position,transform.rotation);
            temp = GameObject.Instantiate(HalfsPrefab,transform.position, transform.rotation);
            temp.transform.Rotate(Vector3.up * 180);
            if (progressionManager != null)
                progressionManager.TryProgressStep(); // Attempt to progress the recipe step
            isCut = true;
            Destroy(this.gameObject);

        }
    }
}
