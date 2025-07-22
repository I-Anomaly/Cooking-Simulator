using System.Collections;
using System.Collections.Generic;
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
    [Header("Jollof Step 6 Settings")]
    public Renderer fillableRenderer; // Assign in inspector, the renderer of the fillable object
    public float colorChangeDuration = 2f; // Time in seconds to fully change to red

    private bool isChangingColor = false;
    private float colorChangeTimer = 0f;
    private bool isFullyRed = false;

    // Update is called once per frame
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
    }

    // This is so nasty of a way to go about it ... it doesn't take into account the different recipes
    public void Fill(float amount, StreamType streamType)
    {
        currentFill += amount * fillRate * Time.deltaTime;
        currentFill = Mathf.Clamp(currentFill, 0, maxFill);
        fillableObject.localScale = new Vector3(1, currentFill, 1);

        if (GameManager.Instance != null)
        {
            var recipe = GameManager.Instance.CurrentRecipe;
            var step = GameManager.Instance.currentStepIndex;

            if (recipe == GameManager.RecipeType.JollofRice)
            {
                // Jollof logic
                if (currentFill >= maxFill && step == 4 && streamType == StreamType.Oil)
                    GameManager.Instance.CompleteCurrentStep();
                else if (step == 6 && streamType == StreamType.Sauce)
                {
                    if (!isChangingColor && !isFullyRed)
                    {
                        isChangingColor = true;
                        colorChangeTimer = 0f;
                    }
                }
            }
            else if (recipe == GameManager.RecipeType.Fufu)
            {
                // Fufu logic   
                if (currentFill >= maxFill)
                {
                    Debug.Log("Fufu step complete: Pot is filled!");
                }
                if (currentFill >= maxFill && streamType == StreamType.Oil)
                    GameManager.Instance.CompleteCurrentStep();
                // Add more Fufu-specific logic here
            }
        }
    }

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
