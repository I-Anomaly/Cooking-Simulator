using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StirManager : MonoBehaviour
{
    private bool isInWater = false;

    // The purpose of this script is to manage the stirring action in water.
    // This will only be active when the player is at step 8 in the game.
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Water"))
            return;

        if (GameManager.Instance != null && GameManager.Instance.currentStepIndex == 8)
        {
            if (!isInWater)
            {
                Debug.Log("Stirring in water");
                isInWater = true;
                GameManager.Instance.StartTimedAction();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Water"))
            return;

        if (GameManager.Instance != null && GameManager.Instance.currentStepIndex == 8)
        {
            Debug.Log("Stopped stirring in water");
            isInWater = false;
            GameManager.Instance.StopTimedAction();
        }
    }
}
