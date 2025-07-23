using UnityEngine;
using static Stream;

public class FillableController : MonoBehaviour
{
    public Transform fillableObject; // The object that will be filled
    public float currentFill = 0.01f; // Current fill amount
    public float fillRate = 0.1f; // Rate at which the object fills, 10% per second

    public float maxFill;

    [Space(10)]
    // For step 6 logic
    [Header("Jollof Oil Settings")]
    public int jollofOilStepIndex = 4; // The step index for Jollof Rice Step 4
    public Renderer fillableRenderer; // Assign in inspector, the renderer of the fillable object

    [Header("Jollof Water Settings")]
    public float colorChangeDuration = 2f; // Time in seconds to fully change to blue
    public int jollofWaterStepIndex = 7; // The step index for Jollof Rice Step 6

    [Header("Jollof Spices Settings")]
    public int jollofSpicesStepIndex = 5; // The step index for Jollof Rice Step 5

    [Header("Jollof Rice Settings")]
    public GameObject ricePile;
    public int jollofRiceStepIndex = 6; // The step index for Jollof Rice Step 5

    [Header("Fufu Settings")]
    public int fufuWaterStepIndex = 1; // The step index for Fufu Step 1

    private bool isChangingColor = false;
    private float colorChangeTimer = 0f;
    private bool isFullyRed = false;

    private bool isStepTimerActive = false;
    private float stepTimer = 0f;
    private float stepTimerDuration = 0f;

    GameManager gm;
    private void Start()
    {
        // Get the game manager instance
        gm = GameManager.Instance;
    }

    void Update()
    {
        // Step 6: Change color to red over time
        if (GameManager.Instance != null && GameManager.Instance.currentStepIndex == 6 && isChangingColor && !isFullyRed)
        {
            // Debug.Log("Changing color to red...");
            colorChangeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(colorChangeTimer / colorChangeDuration);

            // Lerp from original color to red
            if (fillableRenderer != null)
            {
                Color startColor = fillableRenderer.material.color;
                Color targetColor = Color.red;
                fillableRenderer.material.color = Color.Lerp(startColor, targetColor, t);
            }

            if (t >= 1f)
            {
                isFullyRed = true;
                isChangingColor = false;
                CheckStepCompletion();
            }
        }

        if (isStepTimerActive)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepTimerDuration)
            {
                isStepTimerActive = false;
                gm.CompleteCurrentStep();
            }
            Debug.Log($"Step timer: {stepTimer}/{stepTimerDuration} seconds");
        }
    }

    // This is so nasty of a way to go about it ... ugh, if statements control what actions happen depending on the recipe step and liquid
    public void Fill(float amount, StreamType streamType)
    {
        if (gm == null) return;

        var recipe = gm.selectedRecipe;
        var step = gm.currentStepIndex;
        var count = gm.currentRecipe[gm.currentStepIndex];

        if (recipe == GameManager.RecipeType.JollofRice)
        {
            Debug.Log("Jollof Rice step: " + step + " with stream type: " + streamType);

            // Oil step: fill and progress
            if (step == jollofOilStepIndex && streamType == StreamType.Oil)
            {
                currentFill += amount * fillRate * Time.deltaTime;
                currentFill = Mathf.Clamp(currentFill, 0, maxFill);
                fillableObject.localScale = new Vector3(1, currentFill, 1);

                if (currentFill >= maxFill)
                {
                    gm.CompleteCurrentStep();
                }
            }
            // Spices step: start timer logic for sauce
            else if (step == jollofSpicesStepIndex && streamType == StreamType.Sauce)
            {
                Debug.Log("Progressing spices step: " + step + " with stream type: " + streamType);
                if (!isStepTimerActive)
                {
                    stepTimerDuration = (float)count.actionCount;
                    Debug.Log("Spices step timer duration is: " + stepTimerDuration);
                    stepTimer = 0f;
                    isStepTimerActive = true;
                }
            }
            // Water step: start timer logic for water
            else if (step == jollofWaterStepIndex && streamType == StreamType.Water)
            {
                Debug.Log("Progressing water step: " + step + " with stream type: " + streamType);
                if (!isStepTimerActive)
                {
                    stepTimerDuration = (float)count.actionCount;
                    Debug.Log("Water step timer duration is: " + stepTimerDuration);
                    stepTimer = 0f;
                    isStepTimerActive = true;
                }
            }
            // Rice step: enable rice pile
            else if (step == jollofRiceStepIndex)
            {
                if (ricePile != null && !ricePile.activeSelf)
                {
                    ricePile.SetActive(true);
                    gm.CompleteCurrentStep();
                }
            } else
            {
                Debug.Log("This is not a Jollof Rice step, so no action taken.");
            }
        }
        else if (recipe == GameManager.RecipeType.Fufu)
        {
            Debug.Log("Fufu water step: " + step + " with stream type: " + streamType);
            if (step == fufuWaterStepIndex)
            {
                // Fufu logic; when filled, complete step
                currentFill += amount * fillRate * Time.deltaTime;
                currentFill = Mathf.Clamp(currentFill, 0, maxFill);
                fillableObject.localScale = new Vector3(1, currentFill, 1);

                if (currentFill >= maxFill && streamType == StreamType.Water)
                    gm.CompleteCurrentStep();

            } else if (step == 7)
            {
                stepTimerDuration = (float)count.actionCount;
                Debug.Log("Watering number 1 duration is: " + stepTimerDuration);
                stepTimer = 0f;
                isStepTimerActive = true;
            } else if (step == 9)
            {
                stepTimerDuration = (float)count.actionCount;
                Debug.Log("Watering number 2 duration is: " + stepTimerDuration);
                stepTimer = 0f;
                isStepTimerActive = true;
            }
            else
            {
                Debug.Log("This is not the Fufu water step, so no action taken.");
            }
        }
        else
        {
            Debug.LogWarning("Unknown recipe type or step logic not implemented for: " + recipe);
        }
    }

    //public void Fill(float amount, StreamType streamType)
    //{
    //    currentFill += amount * fillRate * Time.deltaTime;
    //    currentFill = Mathf.Clamp(currentFill, 0, maxFill);
    //    fillableObject.localScale = new Vector3(1, currentFill, 1);

    //    if (GameManager.Instance != null)
    //    {
    //        var recipe = GameManager.Instance.CurrentRecipe;
    //        var step = GameManager.Instance.currentStepIndex;

    //        if (recipe == GameManager.RecipeType.JollofRice)
    //        {
    //            // Jollof logic
    //            if (currentFill >= maxFill && step == 4 && streamType == StreamType.Oil)
    //                GameManager.Instance.CompleteCurrentStep();
    //            else if (step == 6 && streamType == StreamType.Sauce)
    //            {
    //                if (!isChangingColor && !isFullyRed)
    //                {
    //                    isChangingColor = true;
    //                    colorChangeTimer = 0f;
    //                }
    //            }
    //        }
    //        else if (recipe == GameManager.RecipeType.Fufu)
    //        {
    //            // Fufu logic   
    //            if (currentFill >= maxFill)
    //            {
    //                Debug.Log("Fufu step complete: Pot is filled!");
    //            }
    //            if (currentFill >= maxFill && streamType == StreamType.Oil)
    //                GameManager.Instance.CompleteCurrentStep();
    //            // Add more Fufu-specific logic here
    //        }
    //    }
    //}

    // Check if both conditions are met to complete the step. This is only for Jollof Rice Step 6 so it doesn't need to
    // have any reference to the Fufu recipe.
    private void CheckStepCompletion()
    {
        if (isFullyRed && GameManager.Instance != null && GameManager.Instance.currentStepIndex == 6)
        {
            Debug.Log("Step 6 complete: Pot is fully red!");
            GameManager.Instance.CompleteCurrentStep();
        }
    }

}
