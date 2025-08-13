using UnityEngine;
using static Stream;

public class FillableController : MonoBehaviour
{
    public Transform fillableObject; // The object that will be filled
    public float currentFill = 0.01f; // Current fill amount
    public float fillRate = 0.1f; // Rate at which the object fills, 10% per second
    public Renderer fillableRenderer; // Assign in inspector, the renderer of the fillable object

    [Space(10)]
    // For step 6 logic
    [Header("Jollof Oil Settings")]
    public string jollofOilStep = "oil_pot"; // The step index for Jollof Rice Step 4
    public float maxOilFill;

    [Header("Jollof Water Settings")]
    public string jollofWaterStep = "oil_pot"; // The step index for Jollof Rice Step 6
    public float maxWaterFill;

    [Header("Jollof Iru-Onion Settings")]
    public string jollofSpicesStep = "iru_onion_pot"; // The step index for Jollof Rice Step 5

    [Header("Jollof Rice Settings")]
    public GameObject ricePile;
    public string jollofRiceStep = "rice_pot"; // The step index for Jollof Rice Step 5

    [Header("Fufu Settings")]
    public string fufuWaterStep = "boil_water"; // The step index for Fufu Step 1
    public float maxFufuWaterFill;

    private Color originalColor;
    private bool isChangingToBlue = false;
    private bool hasTurnedBlue = false;
    private float blueChangeTimer = 0f;
    private float blueChangeDuration = 0f;

    private bool isStepTimerActive = false;
    private float stepTimer = 0f;
    private float stepTimerDuration = 0f;

    private MeshRenderer fillableMeshRenderer; // To enable and disable the mesh renderer of the fillable object

    GameManager gm;
    private void Start()
    {
        // Get the game manager instance
        gm = GameManager.Instance;
        fillableMeshRenderer = GetComponent<MeshRenderer>();

        if (fillableMeshRenderer == null)
        {
            Debug.LogError("There is no mesh renderer recognized on the object.");
        }
        else
        {
            fillableMeshRenderer.enabled = false; // Disable the mesh renderer initially if it's not
            originalColor = fillableRenderer.material.color; // Grab the original color of the fillable object
        }
    }

    void Update()
    {
        // Step 6: Lerp color to blue over the step's actionCount seconds
        if (isChangingToBlue && fillableRenderer != null && blueChangeDuration > 0f)
        {
            blueChangeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(blueChangeTimer / blueChangeDuration);
            Color targetBlue = new(0f, 0f, 1f, originalColor.a); // Blue with original alpha
            fillableRenderer.material.color = Color.Lerp(originalColor, targetBlue, t);

            if (t >= 1f)
            {
                isChangingToBlue = false;
                hasTurnedBlue = true; // Mark as finished
            }
        }

        if (isStepTimerActive)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepTimerDuration)
            {
                isStepTimerActive = false;
                stepTimer = 0;
                gm.CompleteCurrentStep();
            }
            Debug.Log($"Step timer: {stepTimer}/{stepTimerDuration} seconds");
        }
    }

    // This is so nasty of a way to go about it ... ugh, if statements control what actions happen depending on the recipe step and liquid
    public void Fill(float amount, StreamType streamType)
    {
        if (gm == null) {
            Debug.LogWarning("GM is null");
            EnsureGameManager();
        }

        var recipe = gm.selectedRecipe;
        var step = gm.currentRecipe[gm.currentStepIndex];

        if (recipe == GameManager.RecipeType.JollofRice)
        {
            //Debug.Log("Jollof Rice step: " + step + " with stream type: " + streamType);

            // Oil step: fill and progress
            if (step.stepID == jollofOilStep && streamType == StreamType.Oil)
            {
                fillableMeshRenderer.enabled = true; // Enable the mesh renderer for the fillable object
                currentFill += amount * fillRate * Time.deltaTime;
                currentFill = Mathf.Clamp(currentFill, 0, maxOilFill);
                fillableObject.localScale = new Vector3(1, currentFill, 1);

                if (currentFill >= maxOilFill)
                {
                    gm.CompleteCurrentStep();
                }
            }
            // Spices step: start timer logic for sauce
            else if (step.stepID == jollofSpicesStep && streamType == StreamType.Sauce)
            {
                Debug.Log("Progressing spices step: " + step + " with stream type: " + streamType);
                if (!isStepTimerActive)
                {
                    stepTimerDuration = (float)step.actionCount;
                    Debug.Log("Spices step timer duration is: " + stepTimerDuration);
                    stepTimer = 0f;
                    isStepTimerActive = true;
                }
            }
            // Water step: start timer logic for water
            else if (step.stepID == jollofWaterStep && streamType == StreamType.Water)
            {
                Debug.Log("Progressing water step: " + step + " with stream type: " + streamType);
                currentFill += amount * fillRate * Time.deltaTime;
                currentFill = Mathf.Clamp(currentFill, 0, maxWaterFill);
                fillableObject.localScale = new Vector3(1, currentFill, 1);

                // Only start color change if it hasn't already finished
                if (!isChangingToBlue && !hasTurnedBlue)
                {
                    blueChangeDuration = step.actionCount;
                    blueChangeTimer = 0f;
                    isChangingToBlue = true;
                }

                if (currentFill >= maxWaterFill)
                {
                    gm.CompleteCurrentStep();
                }
            }
            // Rice step: enable rice pile
            else if (step.stepID == jollofRiceStep)
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
            if (step.stepID == fufuWaterStep)
            {
                fillableMeshRenderer.enabled = true; // Enable the mesh renderer for the fillable object
                // Fufu logic; when filled, complete step
                currentFill += amount * fillRate * Time.deltaTime;
                currentFill = Mathf.Clamp(currentFill, 0, maxFufuWaterFill);
                fillableObject.localScale = new Vector3(1, currentFill, 1);

                if (currentFill >= maxFufuWaterFill && streamType == StreamType.Water)
                    gm.CompleteCurrentStep();

            } else if (step.stepID == "sprinkle_yams_1")
            {
                stepTimerDuration = (float)step.actionCount;
                Debug.Log("Watering number 1 duration is: " + stepTimerDuration);
                isStepTimerActive = true;
            } else if (step.stepID == "sprinkle_yams_2")
            {
                stepTimerDuration = (float)step.actionCount;
                Debug.Log("Watering number 2 duration is: " + stepTimerDuration);
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
        if (isChangingToBlue && GameManager.Instance != null && GameManager.Instance.currentStepIndex == 6)
        {
            Debug.Log("Step 6 complete: Pot is fully red!");
            GameManager.Instance.CompleteCurrentStep();
        }
    }

    private void EnsureGameManager()
    {
        if (gm == null)
        {
            gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogWarning("GameManager.Instance is still null!");
            }
        }
    }

}
