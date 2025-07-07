using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public int currentRecipeStep; // Current step in the recipe progression
    public enum StepType { Instant, Seconds, Actions }
    public StepType stepType; // Set this in the Inspector to match the item's action type
    public string utensilType; // Set this in the Inspector to match the item's utensil

    GameManager gm;

    private void Start()
    {
        // Get the game manager instance
        gm = GameManager.Instance;
        if (gm == null || gm.currentRecipe.Count == 0) return;
    }

    /// <summary>
    /// Call this from your interaction or trigger event to attempt to progress the recipe step.
    /// </summary>
    public void TryProgressStep()
    {
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
                if (step.actionType == "Instant action")
                {
                    Debug.Log("Performing instant action for step: " + step.description);
                    gm.CompleteCurrentStep();
                }
                break;
            case StepType.Actions:
                if (step.actionType == "Number of actions")
                {
                    Debug.Log("Performing action for step: " + step.description);
                    gm.IncrementAction();
                }
                break;
            case StepType.Seconds:
                if (step.actionType == "Seconds passed")
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
            return;

        if (gm != null)
        {
            gm.CompleteCurrentStep();
        }
    }
}
