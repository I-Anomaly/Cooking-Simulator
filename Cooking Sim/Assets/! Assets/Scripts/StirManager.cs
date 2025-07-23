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

        if (GameManager.Instance.currentStepIndex != 8 || GameManager.Instance.recipeComplete == true)
            return;

        if (!isInWater)
        {
            Debug.Log("Stirring in water");
            isInWater = true;
            GameManager.Instance.StartTimedAction();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (GameManager.Instance.recipeComplete == true)
        {
            isInWater = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Water"))
            return;

        if (GameManager.Instance.currentStepIndex != 8 || GameManager.Instance.recipeComplete == true)
            return;

        Debug.Log("Stopped stirring in water");
        isInWater = false;
        GameManager.Instance.StopTimedAction();
    }
}
