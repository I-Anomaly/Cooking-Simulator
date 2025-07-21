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
    [Header("Step 5 Progression - Pour water into the pot")]
    int currentStep = 5; // Current step index in the recipe

    // For step 6 logic
    [Header("Step 6 Settings")]
    public Renderer fillableRenderer; // Assign in inspector, the renderer of the fillable object
    public float colorChangeDuration = 2f; // Time in seconds to fully change to red

    private bool isChangingColor = false;
    private float colorChangeTimer = 0f;
    private bool isFullyRed = false;

    // Start is called before the first frame update
    void Start()
    {
        maxFill = 0.1f; // Assuming the fillable object is scaled in the Y direction

    }

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

    public void Fill(float amount, StreamType streamType)
    {
        currentFill += amount * fillRate * Time.deltaTime;
        currentFill = Mathf.Clamp(currentFill, 0, maxFill);
        fillableObject.localScale = new Vector3(1, currentFill, 1);

        if (GameManager.Instance != null)
        {
            // Step 5: Only progress with water stream
            if (currentFill >= maxFill && GameManager.Instance.currentStepIndex == 5 && streamType == StreamType.Water)
            {
                // Debug.Log("Filled with water, progressing step 5!");
                GameManager.Instance.CompleteCurrentStep();
            }
            // Step 6: Only progress color change with sauce stream
            else if (GameManager.Instance.currentStepIndex == 6 && streamType == StreamType.Sauce)
            {
                // Debug.Log("Fillig with sauce, changing color to red!");
                if (!isChangingColor && !isFullyRed)
                {
                    isChangingColor = true;
                    colorChangeTimer = 0f;
                }
            }
        }
    }

    // Check if both conditions are met to complete the step
    private void CheckStepCompletion()
    {
        if (isFullyRed && GameManager.Instance != null && GameManager.Instance.currentStepIndex == 6)
        {
            Debug.Log("Step 6 complete: Pot is fully red!");
            GameManager.Instance.CompleteCurrentStep();
        }
    }

}
