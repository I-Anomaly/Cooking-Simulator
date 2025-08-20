using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableSelf : MonoBehaviour
{
    // The step index at which this veggie should be disabled
    [SerializeField] private int disableAtStep = 5;

    // I don't like this method of checking the step index, but it works for now
    void Update()
    {
        // Make sure GameManager instance exists and recipe is running
        if (GameManager.Instance != null && GameManager.Instance.currentStepIndex >= disableAtStep)
        {
            Debug.Log("Disabling object: " + gameObject.name);
            gameObject.SetActive(false);
            Destroy(this);
        }
    }
}
