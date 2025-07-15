using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the other object has a ProgressionManager
        if (other.TryGetComponent<ProgressionManager>(out var progressionManager))
        {
            progressionManager.TryProgressStep();
        }
        else
        {
            Debug.LogWarning("ProgressionManager not found on the object that entered the trigger: " + other.name);
        }
    }
}
