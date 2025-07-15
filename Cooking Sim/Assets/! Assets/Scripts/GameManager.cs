using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Define a RecipeStep class for each step in the recipe
    [System.Serializable]
    public class RecipeStep
    {
        public string description; // Description of the step, e.g., "Slice tomatoes", "Add water to pot"
        public string utensil; // The utensil required for this step, e.g., "Knife", "Mortar", "Pestle", "Pot"
        public int actionCount; // Number of times an action is performed for this step
        public string actionType; // What type of action this is, e.g., "Instant action", "Number of actions", "Seconds passed"
        // You can add more fields, e.g., required ingredient, etc.
    }

    // Enum for recipe selection
    public enum RecipeType { JollofRice, Fufu }
    public RecipeType selectedRecipe = RecipeType.Fufu;

    // Define recipes
    public List<RecipeStep> jollofRiceRecipe = new List<RecipeStep>
    {
        new() { description = "Slice tomatoes and place in mortar", utensil = "Mortar", actionCount = 3, actionType = "Number of actions" },
        new() { description = "Slice peppers and place in mortar", utensil = "Mortar", actionCount = 2, actionType = "Number of actions" },
        new() { description = "Slice onions and place in mortar", utensil = "Mortar", actionCount = 2, actionType = "Number of actions" },
        new() { description = "Ground everything into a paste", utensil = "Pestle", actionCount = 5, actionType = "Number of actions" },
        new() { description = "Add water to pot and bring to a boil", utensil = "Pot", actionCount = 1, actionType = "Instant action" },
        new() { description = "Add paste, spices, and meat to the stew", utensil = "Pot", actionCount = 3, actionType = "Instant action" },
        new() { description = "Add rice to the pot; it will absorb the stew into it", utensil = "Pot", actionCount = 3, actionType = "Seconds passed" }, // raycast hit to pot to add rice
        new() { description = "Stir the rice until all the liquid is absorbed", utensil = "Pot", actionCount = 5, actionType = "Seconds passed" }
    };

    public List<RecipeStep> fufuRecipe = new List<RecipeStep>
    {
        new() { description = "Peel the yams", utensil = "Knife", actionCount = 4, actionType = "Number of actions" },
        new() { description = "Add water and bring to a boil", utensil = "Pot", actionCount = 1, actionType = "Instant action" },
        new() { description = "Place yams in water", utensil = "Pot", actionCount = 1, actionType = "Instant action" },
        new() { description = "Boil yams until they are soft", utensil = "Pot", actionCount = 5, actionType = "Seconds passed" },
        new() { description = "Drain water", utensil = "Pot", actionCount = 5, actionType = "Seconds passed" },
        new() { description = "Move yams to mortar", utensil = "Pestle", actionCount = 1, actionType = "Instant action" },
        new() { description = "Pound yams with pestle", utensil = "Pestle", actionCount = 3, actionType = "Number of actions" },
        new() { description = "Sprinkle water", utensil = "Water", actionCount = 1, actionType = "Instant action" },
        new() { description = "Pound yams with pestle", utensil = "Pestle", actionCount = 3, actionType = "Number of actions" },
        new() { description = "Sprinkle water", utensil = "Water", actionCount = 1, actionType = "Instant action" },
        new() { description = "Pound yams with pestle", utensil = "Pestle", actionCount = 3, actionType = "Number of actions" },
        new() { description = "Roll into a ball", utensil = "Hand", actionCount = 3, actionType = "Seconds passed" }
    };

    public List<RecipeStep> currentRecipe = new List<RecipeStep>();
    public TextMeshPro stepText; // Reference to a TextMeshPro world component to display the current step
    public int currentStepIndex = 0;
    public int actionCount = 0;

    // For steps that require time to pass, e.g., "Seconds passed"
    private bool isTiming = false; // For steps that require time to pass
    private float elapsedTime = 0f; // Timer for "Seconds passed" steps

    static GameManager instance;
    public static GameManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Fader.FadeIn();

        // Choose recipe based on selectedRecipe
        switch (selectedRecipe)
        {
            case RecipeType.JollofRice:
                currentRecipe = new List<RecipeStep>(jollofRiceRecipe);
                break;
            case RecipeType.Fufu:
                currentRecipe = new List<RecipeStep>(fufuRecipe);
                break;
        }

        currentStepIndex = 0;
        actionCount = 0;
        ShowCurrentStep();
    }

    private void Update()
    {
        // Only run timer for "Seconds passed" steps
        if (currentRecipe.Count > 0 && currentStepIndex < currentRecipe.Count)
        {
            var step = currentRecipe[currentStepIndex];
            if (step.actionType == "Seconds passed" && isTiming)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= step.actionCount) // actionCount used as seconds
                {
                    isTiming = false;
                    elapsedTime = 0f;
                    CompleteCurrentStep();
                    Debug.Log("Timed step complete. Moving onto next step.");
                }
            }
        }
    }

    // Call this when a task is completed
    public void CompleteCurrentStep()
    {
        actionCount = 0; // Reset action count for the next step
        // If not at the last step, increment the index (recipe step) and show the next step
        currentStepIndex++;
        Debug.Log("Complete the current step, reset the action count (" + actionCount + "), increment the step index, and show the next step.");
        if (currentStepIndex < currentRecipe.Count)
        {
            ShowCurrentStep();
        }
        else
        {
            Debug.Log("Recipe complete!");
            // Handle recipe completion
        }
    }

    // Use this to handle doing an action 'x' amount of times
    public void IncrementAction()
    {
        Debug.Log("Incrementing action count. The current action count is " + actionCount + " and after this it should be " + (actionCount + 1));
        actionCount++;
        if (actionCount >= currentRecipe[currentStepIndex].actionCount)
        {
            Debug.Log("Step completed: " + currentRecipe[currentStepIndex].description + ". Moving onto next step.");
            CompleteCurrentStep(); // Automatically complete the step if action count is met
        }
        else
        {
            Debug.Log("Action performed: " + actionCount + "/" + currentRecipe[currentStepIndex].actionCount);
        }
    }

    #region Timed Steps
    // Call this in OnTriggerEnter for the timed step
    public void StartTimedAction()
    {
        isTiming = true;
    }

    // Call this in OnTriggerExit for the timed step
    public void StopTimedAction()
    {
        isTiming = false;
    }
    #endregion

    // Display the current step in the TextMeshPro component and in the console
    void ShowCurrentStep()
    {
        Debug.Log("Current Step: " + currentRecipe[currentStepIndex].description + " and it must be done " + currentRecipe[currentStepIndex].actionCount + " times.");
        if (stepText != null)
        {
            stepText.text = currentRecipe[currentStepIndex].description + " (" + currentRecipe[currentStepIndex].utensil + ")";
        }
        else
        {
            Debug.LogWarning("No TextMeshPro component assigned to display the step text.");
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 20), "Complete Step"))
        {
            CompleteCurrentStep();
        }

        if (GUI.Button(new Rect(10, 30, 100, 20), "Start Timer"))
        {
            StartTimedAction();
        }

        if (GUI.Button(new Rect(10, 50, 100, 20), "Stop Timer"))
        {
            StopTimedAction();
        }

        if (GUI.Button(new Rect(10, 70, 100, 20), "Add Action"))
        {
            IncrementAction();
        }
    }
}
