using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiceFillableController : MonoBehaviour
{
    public Transform FillableObject;
    public float CurrentFill = 0.01f;
    public float FillRate = 0.1f;
    float MaxFill;

    public GameObject ricePile; // The rice pile GameObject

    GameManager gm;

    private bool isStepTimerActive = false;
    private float stepTimer = 0f;
    private float stepTimerDuration = 0f;
    private void Start()
    {
        // Get the game manager instance
        gm = GameManager.Instance;
    }

    private void Update()
    {
        // Timer logic for "SecondsPassed" steps
        if (isStepTimerActive)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepTimerDuration)
            {
                isStepTimerActive = false;
                gm.CompleteCurrentStep();
                if (ricePile != null) ricePile.SetActive(true);
            }
        }
    }

    public void Fill(float amount)
    {
        //CurrentFill += amount * FillRate * Time.deltaTime;
        //CurrentFill = CurrentFill>MaxFill?MaxFill:CurrentFill;

        //FillableObject.localScale = new Vector3(1, CurrentFill, 1);
    }

    // Call this when hit by the rice stream
    public void OnRiceStreamHit()
    {
        if (gm == null) return;

        // Check for Jollof Rice recipe and step index 6
        if (gm.CurrentRecipe == GameManager.RecipeType.JollofRice && gm.currentStepIndex == 6)
        {
            var step = gm.currentRecipe[gm.currentStepIndex];

            // Only start timer if step is a timed step
            if (step.actionType == GameManager.ActionType.SecondsPassed && !isStepTimerActive)
            {
                stepTimerDuration = (float)step.actionCount;
                stepTimer = 0f;
                isStepTimerActive = true;
                Debug.Log($"Rice stream hit: starting timer for {stepTimerDuration} seconds.");
            }
            // If not a timed step, just complete the step and show rice pile
            else if (step.actionType == GameManager.ActionType.InstantAction)
            {
                gm.CompleteCurrentStep();
                if (ricePile != null) ricePile.SetActive(true);
            }
        }
    }
}
