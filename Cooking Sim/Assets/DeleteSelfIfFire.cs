using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteSelfIfFire : MonoBehaviour
{
    CampFire campFire;

    // Start is called before the first frame update
    void Start()
    {
        campFire = FindObjectOfType<CampFire>();
        if (campFire.isFireOn)
            Destroy(this.gameObject);
    }
}
