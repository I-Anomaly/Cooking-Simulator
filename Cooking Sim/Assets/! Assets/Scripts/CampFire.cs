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
        if (count >= gm.currentRecipe[gm.currentStepIndex].actionCount)
        {
            Debug.Log("Enable fire effect");
            if (fireGameObject == null)
            {
                Debug.LogError("fireGameObject is not assigned!");
                return;
            }

            fireGameObject.SetActive(true); // Ensure the object is enabled

            //// Play all particle systems on the fireGameObject and its children
            //var particleSystems = fireGameObject.GetComponentsInChildren<ParticleSystem>(true);
            //foreach (var ps in particleSystems)
            //{
            //    ps.Play();
            //}
        }
        gm.IncrementAction();
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
