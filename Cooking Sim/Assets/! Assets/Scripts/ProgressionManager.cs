using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class ProgressionManager : MonoBehaviour
{
    public int currentRecipeStep; // Current step in the recipe progression
    public enum StepType { Instant, Seconds, Actions }
    public StepType stepType; // Set this in the Inspector to match the item's action type
    public string stepID; // Set this in the Inspector to match the item's utensil
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
            // Debug.LogWarning("Object (" + other.name + ") does not have the required tag: " + requiredTag);
            return;
        }

        // If the game manager is not initialized or has no current recipe, exit
        if (gm == null || gm.currentRecipe.Count == 0)
        {
            Debug.LogWarning("GameManager is not initialized or current recipe is empty.");
            return;
        }

        // Check if the current step ID string matches the step ID of the current step in the recipe
        var step = gm.currentRecipe[gm.currentStepIndex];
        if (!string.Equals(stepID, step.stepID, System.StringComparison.Ordinal))
        {
            Debug.Log("This is not the correct step for this item. Current step ID: " + step.stepID + ", expected step ID: " + stepID);
            return;
        }

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
        if (other.CompareTag(requiredTag) == false) {
            return;
        }
        GameObject otherRoot = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        triggeredObjects.Remove(otherRoot);

        int currentStepIndex = gm.currentStepIndex;
        var step = gm.currentRecipe[currentStepIndex];

        switch (stepType)
        {
            case StepType.Seconds:
                if (step.actionType == ActionType.SecondsPassed)
                {
                    gm.StopTimedAction();
                }
                break;
            case StepType.Actions:
                if (step.actionType == ActionType.NumberOfActions)
                {
                    // If the item is taken out, reduce the action count (it will not proceed until ALL items are in)
                    gm.ReduceAction();
                }
                break;
        }
    }

    /// <summary>
    /// Call from the interaction or trigger event to attempt to progress the recipe step.
    /// </summary>
    public void TryProgressStep()
    {
        DebugStep();

        // If the game manager is not initialized or has no current recipe, exit
        if (gm == null || gm.currentRecipe.Count == 0)
            return;

        // Check if the current step index matches the step this item is responsible for
        int currentStepIndex = gm.currentStepIndex;
        if (currentRecipeStep != currentStepIndex)
            return;

        // Get the current step from the game manager's recipe
        var step = gm.currentRecipe[currentStepIndex];

        //if (step.utensil != utensilType)
        //    return;

        // Check if the action type matches the step type, then do the action on the game manager
        switch (stepType)
        {
            // Instant steps are for actions that complete immediately
            case StepType.Instant:
                // I really don't think it needs to double check the action type but better safe than sorry
                if (step.actionType == ActionType.InstantAction)
                {
                    Debug.Log("Performing instant action for step: " + step.description);
                    gm.CompleteCurrentStep();
                } else
                {
                    Debug.LogWarning("Step type mismatch: Expected InstantAction but got " + step.actionType + " for step: " + step.description);
                }
                    break;
            // Actions are for steps that require a certain number of actions to be completed
            case StepType.Actions:
                if (step.actionType == ActionType.NumberOfActions)
                {
                    Debug.Log("Performing action for step: " + step.description);
                    gm.IncrementAction();
                }
                else
                {
                    Debug.LogWarning("Step type mismatch: Expected NumberOfActions but got " + step.actionType + " for step: " + step.description);
                }
                break;
            // Seconds are for steps that require a certain amount of time to pass
            case StepType.Seconds:
                if (step.actionType == ActionType.SecondsPassed)
                {
                    Debug.Log("Timing action for step: " + step.description);
                    // Call gm.StartTimedAction() and gm.StopTimedAction() as needed from your trigger events
                    gm.StartTimedAction();
                }
                else
                {
                    Debug.LogWarning("Step type mismatch: Expected SecondsPassed but got " + step.actionType + " for step: " + step.description);
                }
                break;
            // If the step type is not recognized, log a warning
            default:
                Debug.LogWarning("Unknown step type: " + stepType + " for step: " + step.description);
                break;
        }
    }

    /// <summary>
    /// Attempt to complete the current step in the recipe.
    /// </summary>
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

        // If the game manager is not null, complete the current step
        if (gm != null)
        {
            gm.CompleteCurrentStep();
        }

        // If disableOnComplete is true, disable this GameObject (for some reason it's not working in the editor)
        if (disableOnComplete)
        {
            Debug.Log("Step completed: " + currentRecipeStep + ". Disabling object: " + gameObject.name);
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Just for debugging purposes, this will log the current step and the GameObject it's attached to, and will check if it's the correct step.
    /// </summary>
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
