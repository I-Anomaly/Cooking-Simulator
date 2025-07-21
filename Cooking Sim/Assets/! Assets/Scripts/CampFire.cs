using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampFire : MonoBehaviour
{
    GameManager gm;

    int count;

    public GameObject fireGameObject; // GameObject to activate when the action count is reached

    // Start is called before the first frame update
    void Start()
    {
        // Get the game manager instance
        gm = GameManager.Instance;
    }

    public void IncrementProgress()
    {
        count++;
        gm.IncrementAction();
        if (count >= gm.currentRecipe[gm.currentStepIndex].actionCount)
        {
            fireGameObject.SetActive(true); // Activate the fire effect
        }
    }

    public void DecrementProgress()
    {
        if (count > 0)
        {
            count--;
            gm.ReduceAction();
        }
            
    }
}
