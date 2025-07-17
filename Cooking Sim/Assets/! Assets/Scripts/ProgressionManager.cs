using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class ProgressionManager : MonoBehaviour
{
    public int currentRecipeStep; // Current step in the recipe progression
    public enum StepType { Instant, Seconds, Actions }
    public StepType stepType; // Set this in the Inspector to match the item's action type
    public string utensilType; // Set this in the Inspector to match the item's utensil
    public string requiredTag; // Set this in the Inspector to match the required tag for the item

    [Space(10)]
    public bool disableOnComplete = true; // Disable this object when the step is completed

    GameManager gm;

    // Track which GameObjects have already triggered this frame
    private HashSet<GameObject> triggeredObjects = new HashSet<GameObject>();

    private void Start()
    {
        // Get the game manager instance
        gm = GameManager.Instance;
        if (gm == null || gm.currentRecipe.Count == 0) return;
    }

    /// <summary>
    /// On trigger enter, check if the object has the required tag and if it matches the current step.
    /// </summary>
    /// <param name="other">The item that it's colliding with</param>
    private void OnTriggerEnter(Collider other)
    {
        // Nasty method to try to not trigger multiple times in a single frame
        GameObject otherRoot = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;

        // Only process if this is the first collider from the other GameObject
        if (triggeredObjects.Contains(otherRoot))
            return;

        triggeredObjects.Add(otherRoot);

        // Check if the entering object has the required tag
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
        {
            Debug.LogWarning("Object (" + other.name + ") does not have the required tag: " + requiredTag);
            return;
        }

        // If the game manager is not initialized or has no current recipe, exit
        if (gm == null || gm.currentRecipe.Count == 0)
        {
            Debug.LogWarning("GameManager is not initialized or current recipe is empty.");
            return;
        }
         
        // Check if the current step index matches the step this item is responsible for
        int currentStepIndex = gm.currentStepIndex;
        if (currentRecipeStep != currentStepIndex)
        {
            Debug.Log("This is not the correct step for this item. Current step: " + currentStepIndex + ", expected step: " + currentRecipeStep);
            return;
        }

        var step = gm.currentRecipe[currentStepIndex];

        // Optionally check utensil type
        //if (!string.IsNullOrEmpty(utensilType) && step.utensil != utensilType)
        //    return;

        Debug.Log("Proceeding with step: " + step.description);

        // Proceed based on step type
        switch (stepType)
        {
            case StepType.Instant:
                Debug.Log("Performing instant action for step: " + step.description);
                gm.CompleteCurrentStep();
                break;
            case StepType.Actions:
                Debug.Log("Performing action for step: " + step.description);
                gm.IncrementAction();
                break;
            case StepType.Seconds:
                Debug.Log("Timing action for step: " + step.description);
                gm.StartTimedAction();
                break;
        }
    }

    /// <summary>
    /// Stop timing action when the object exits the trigger.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        GameObject otherRoot = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        triggeredObjects.Remove(otherRoot);

        int currentStepIndex = gm.currentStepIndex;
        var step = gm.currentRecipe[currentStepIndex];

        switch (stepType)
        {
            case StepType.Seconds:
                if (step.actionType == ActionType.SecondsPassed)
                {
                    Debug.Log("Timing action for step: " + step.description);
                    gm.StartTimedAction();
                }
                break;
        }
    }

    /// <summary>
    /// Call this from your interaction or trigger event to attempt to progress the recipe step.
    /// </summary>
    public void TryProgressStep()
    {
        DebugStep();
        if (gm == null || gm.currentRecipe.Count == 0)
            return;

        int currentStepIndex = gm.currentStepIndex;
        if (currentRecipeStep != currentStepIndex)
            return;

        var step = gm.currentRecipe[currentStepIndex];

        //if (step.utensil != utensilType)
        //    return;

        switch (stepType)
        {
            case StepType.Instant:
                // I really don't think it needs to double check the action type but better safe than sorry
                if (step.actionType == ActionType.InstantAction)
                {
                    Debug.Log("Performing instant action for step: " + step.description);
                    gm.CompleteCurrentStep();
                }
                break;
            case StepType.Actions:
                if (step.actionType == ActionType.NumberOfActions)
                {
                    Debug.Log("Performing action for step: " + step.description);
                    gm.IncrementAction();
                }
                break;
            case StepType.Seconds:
                if (step.actionType == ActionType.SecondsPassed)
                {
                    Debug.Log("Timing action for step: " + step.description);
                    // Call gm.StartTimedAction() and gm.StopTimedAction() as needed from your trigger events
                }
                break;
        }
    }

    public void CompleteStep()
    {
        // Check if the current step index matches the current recipe step
        int currentStepIndex = gm.currentStepIndex;
        if (currentRecipeStep != currentStepIndex)
        {
            Debug.LogWarning("This is not the correct step to complete. Current step: " + currentStepIndex + ", expected step: " + currentRecipeStep);
            if (gm != null)
            {
                Debug.Log("Current Step: " + gm.currentStepIndex + " and this is attached to " + gameObject.name);
            }
            return;
        }

        if (gm != null)
        {
            gm.CompleteCurrentStep();
        }

        if (disableOnComplete)
        {
            Debug.Log("Step completed: " + currentRecipeStep + ". Disabling object: " + gameObject.name);
            gameObject.SetActive(false);
        }
    }

    public void DebugStep()
    {
        if (gm == null || gm.currentRecipe.Count == 0) {
            Debug.LogWarning("GameManager is not initialized or current recipe is empty.");
            return;
        }

        int currentStepIndex = gm.currentStepIndex;
        if (currentRecipeStep != currentStepIndex)
        {
            Debug.Log("This is not the correct step to debug.");
            return;
        }

        Debug.Log("Current Step: " + currentRecipeStep + " and this is attached to " + gameObject.name);
    }
}
