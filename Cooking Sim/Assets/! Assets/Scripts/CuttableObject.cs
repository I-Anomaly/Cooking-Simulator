using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttableObject : MonoBehaviour
{
    public GameObject HalfsPrefab;
    public bool isCut;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name.Contains("Knife") && !isCut)
        {
            GameObject temp = GameObject.Instantiate(HalfsPrefab,transform.position,transform.rotation);
            temp = GameObject.Instantiate(HalfsPrefab,transform.position, transform.rotation);
            temp.transform.Rotate(Vector3.up * 180);
            
            isCut = true;
            Destroy(this.gameObject);

        }
    }
}
