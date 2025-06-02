using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Define a RecipeStep class for each step in the recipe
    [System.Serializable]
    public class RecipeStep
    {
        public string description;
        public string utensil;
        public int actionCount; // Number of times an action is performed for this step
        // You can add more fields, e.g., required ingredient, action, etc.
    }

    // Enum for recipe selection
    public enum RecipeType { HerbSoup, OnionStew, FruitSalad }
    public RecipeType selectedRecipe = RecipeType.HerbSoup;

    // Define recipes
    private List<RecipeStep> herbSoupRecipe = new List<RecipeStep>
    {
        new RecipeStep { description = "Grind herbs", utensil = "Mortar", actionCount = 3 },
        new RecipeStep { description = "Add water", utensil = "Pot", actionCount = 1 },
        new RecipeStep { description = "Stir", utensil = "Spoon", actionCount = 2 }
    };

    private List<RecipeStep> onionStewRecipe = new List<RecipeStep>
    {
        new RecipeStep { description = "Chop onions", utensil = "Knife", actionCount = 2 },
        new RecipeStep { description = "Fry onions", utensil = "Pan", actionCount = 1 },
        new RecipeStep { description = "Simmer", utensil = "Pot", actionCount = 1 }
    };

    private List<RecipeStep> fruitSaladRecipe = new List<RecipeStep>
    {
        new RecipeStep { description = "Slice apples", utensil = "Knife", actionCount = 2 },
        new RecipeStep { description = "Add berries", utensil = "Bowl", actionCount = 1 },
        new RecipeStep { description = "Mix", utensil = "Spoon", actionCount = 1 }
    };

    public List<RecipeStep> currentRecipe = new List<RecipeStep>();
    private int currentStepIndex = 0;
    private int actionCount = 0;

    void Start()
    {
        Fader.FadeIn();

        // Choose recipe based on selectedRecipe
        switch (selectedRecipe)
        {
            case RecipeType.HerbSoup:
                currentRecipe = new List<RecipeStep>(herbSoupRecipe);
                break;
            case RecipeType.OnionStew:
                currentRecipe = new List<RecipeStep>(onionStewRecipe);
                break;
            case RecipeType.FruitSalad:
                currentRecipe = new List<RecipeStep>(fruitSaladRecipe);
                break;
        }

        currentStepIndex = 0;
        actionCount = 0;
        ShowCurrentStep();
    }

    // Call this when a task is completed
    public void CompleteCurrentStep()
    {
        // If not at the last step, increment the index and show the next step
        currentStepIndex++;
        actionCount = 0; // Reset action count for the next step
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
        actionCount++;
        if (actionCount >= currentRecipe[currentStepIndex].actionCount)
        {
            CompleteCurrentStep(); // Automatically complete the step if action count is met
            Debug.Log("Step completed: " + currentRecipe[currentStepIndex].description + ". Moving onto next step.");
        }
        else
        {
            Debug.Log("Action performed: " + actionCount + "/" + currentRecipe[currentStepIndex].actionCount);
        }
    }

    void ShowCurrentStep()
    {
        Debug.Log("Current Step: " + currentRecipe[currentStepIndex].description);
        // Update UI here if needed
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 100, 100, 20), "Complete Step"))
        {
            CompleteCurrentStep();
        }
    }
}
