using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{
    // Define a RecipeStep class for each step in the recipe
    [System.Serializable]
    public class RecipeStep
    {
        public string description; // Description of the step, e.g., "Slice tomatoes", "Add water to pot"
        public string utensil; // The utensil required for this step, e.g., "Knife", "Mortar", "Pestle", "Pot", this could maybe be removed
        public int actionCount; // Number of times an action is performed for this step
        public ActionType actionType; // What type of action this is, e.g., "Instant action", "Number of actions", "Seconds passed"
        public AudioClip voiceClip; // Assign in Inspector
        public int delayBeforeAudio; // Delay before playing the audio clip, useful for timed steps
        // You can add more fields, e.g., required ingredient, etc.
    }
    public enum ActionType
    {
        InstantAction,
        NumberOfActions,
        SecondsPassed,
        Auto
    }

    // Enum for recipe selection
    public enum RecipeType { JollofRice, Fufu }
    public RecipeType CurrentRecipe { get; set; }
    public RecipeType selectedRecipe = RecipeType.Fufu;

    // Define recipes
    public List<RecipeStep> jollofRiceRecipe = new List<RecipeStep>
    {
        new() { description = "Slice tomatoes and place in mortar", utensil = "Mortar", actionCount = 3, actionType = ActionType.NumberOfActions },
        new() { description = "Slice peppers and place in mortar", utensil = "Mortar", actionCount = 2, actionType = ActionType.NumberOfActions },
        new() { description = "Slice onions and place in mortar", utensil = "Mortar", actionCount = 2, actionType = ActionType.NumberOfActions },
        new() { description = "Ground everything into a paste", utensil = "Pestle", actionCount = 5, actionType = ActionType.NumberOfActions },
        new() { description = "Add water to pot and bring to a boil", utensil = "Pot", actionCount = 1, actionType = ActionType.InstantAction },
        new() { description = "Add paste, spices, and meat to the stew", utensil = "Pot", actionCount = 3, actionType = ActionType.InstantAction },
        new() { description = "Add rice to the pot; it will absorb the stew into it", utensil = "Pot", actionCount = 3, actionType = ActionType.SecondsPassed }, // raycast hit to pot to add rice
        new() { description = "Stir the rice until all the liquid is absorbed", utensil = "Pot", actionCount = 5, actionType = ActionType.SecondsPassed }
    };

    public List<RecipeStep> fufuRecipe = new List<RecipeStep>
    {
        new() { description = "Peel the yams", utensil = "Knife", actionCount = 4, actionType = ActionType.NumberOfActions },
        new() { description = "Add water and bring to a boil", utensil = "Pot", actionCount = 1, actionType = ActionType.InstantAction },
        new() { description = "Place yams in water", utensil = "Pot", actionCount = 1, actionType = ActionType.InstantAction },
        new() { description = "Boil yams until they are soft", utensil = "Pot", actionCount = 5, actionType = ActionType.SecondsPassed },
        new() { description = "Drain water", utensil = "Pot", actionCount = 5, actionType = ActionType.SecondsPassed },
        new() { description = "Move yams to mortar", utensil = "Pestle", actionCount = 1, actionType = ActionType.InstantAction },
        new() { description = "Pound yams with pestle", utensil = "Pestle", actionCount = 3, actionType = ActionType.NumberOfActions },
        new() { description = "Sprinkle water", utensil = "Water", actionCount = 1, actionType = ActionType.InstantAction },
        new() { description = "Pound yams with pestle", utensil = "Pestle", actionCount = 3, actionType = ActionType.NumberOfActions },
        new() { description = "Sprinkle water", utensil = "Water", actionCount = 1, actionType = ActionType.InstantAction },
        new() { description = "Pound yams with pestle", utensil = "Pestle", actionCount = 3, actionType = ActionType.NumberOfActions },
        new() { description = "Roll into a ball", utensil = "Hand", actionCount = 3, actionType = ActionType.SecondsPassed }
    };

    public List<RecipeStep> currentRecipe = new List<RecipeStep>();
    public TextMeshProUGUI stepText; // Reference to a TextMeshPro world component to display the current step
    public int currentStepIndex = 0;
    public int actionCount = 0;

    // For steps that require time to pass, e.g., "Seconds passed"
    private bool isTiming = false; // For steps that require time to pass
    private float elapsedTime = 0f; // Timer for "Seconds passed" steps

    private AudioSource audioSource; // Assign in Inspector

    [Space(10)]
    public bool recipeComplete = false; // This is used to determine if the recipe is complete or not

    [Header("UI")]
    public GameObject delayPanel;

    [Space(10)]
    [Header("Jollof Rice Recipe Objects")]
    public GameObject[] jollofVeggies;
    public GameObject mashedTexture; // This is the mashed texture for the jollof rice, which will be enabled when the veggies are mashed
    public int mashedTextureIndex = 4; // The index of the step where the mashed texture should be enabled
    public GameObject mortar; // The mortar object for the jollof rice recipe

    [Space(10)]
    [Header("Recipe Complete!")]
    public GameObject particleEffect;
    public AudioClip celebration;

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

        // Always clear the current recipe list first
        currentRecipe.Clear();

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

        audioSource = GetComponent<AudioSource>();

        currentStepIndex = 0;
        actionCount = 0;
        ShowCurrentStep();

        recipeComplete = false; // Reset recipe complete status
    }

    private void Update()
    {
        if (recipeComplete)
            {
            // Debug.Log("Recipe is complete, no further steps can be processed.");
            return; // If the recipe is complete, do not process any further steps
        }

        var step = currentRecipe[currentStepIndex];
        if (currentStepIndex >= currentRecipe.Count)
        {
            Debug.LogWarning("Current step index is out of bounds of the recipe list.");
            return;
        }
        // Only run timer for "Seconds passed" steps
        if (currentRecipe.Count > 0 && currentStepIndex < currentRecipe.Count)
        {
            //var step = currentRecipe[currentStepIndex];
            if (step.actionType == ActionType.SecondsPassed && isTiming)
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

        if (step.actionType == ActionType.Auto)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= step.actionCount) // actionCount used as seconds
                {
                    isTiming = false;
                    elapsedTime = 0f;
                    CompleteCurrentStep();
                    Debug.Log("Automatically moving onto the next step after 5 seconds have passed");
                }
            }
    }

    // Call this when a task is completed
    public void CompleteCurrentStep()
    {
        actionCount = 0; // Reset action count for the next step
        // If not at the last step, increment the index (recipe step) and show the next step
        currentStepIndex++;
        // Debug.Log("Complete the current step, reset the action count (" + actionCount + "), increment the step index, and show the next step.");
        if (currentStepIndex < currentRecipe.Count)
        {
            // This is a VERY lazy way to disable the jollof veggies, but it works for now
            if (RecipeType.JollofRice == selectedRecipe && currentStepIndex == mashedTextureIndex)
            {
                mashedTexture.SetActive(true); // Enable the mashed texture

                // Set the mortar to be interactable at this point
                // Get the XRGrabInteractable component
                XRGrabInteractable grab = mortar.GetComponent<XRGrabInteractable>();
                if (grab != null)
                {
                    grab.enabled = true; // Enable the grab component
                }

                // Get the Rigidbody and disable kinematic
                Rigidbody rb = mortar.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }
            }

            ShowCurrentStep();
        }
        else
        {
            Debug.Log("Recipe complete!");
            recipeComplete = true; // Set recipe complete status to true
            // Handle recipe completion
            if (stepText != null)
            {
                stepText.text = "Recipe Complete!";
            }
            if (particleEffect != null)
            {
                Instantiate(particleEffect, stepText.transform.position, Quaternion.identity);
            }
            if (audioSource != null && celebration != null)
            {
                audioSource.PlayOneShot(celebration);
            }
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

    /// <summary>
    /// This is used for any situation where you need to reduce the action count, such as when an action is undone or a step is reset.
    /// * It will not allow the action count to go below zero.
    /// </summary>
    public void ReduceAction()
    {
        if (actionCount > 0)
        {
            actionCount--;
            Debug.Log("Action count reduced. Current action count: " + actionCount);
        }
        else
        {
            Debug.LogWarning("Action count is already at zero, cannot reduce further.");
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

    IEnumerator DelayAndPlayAudio(RecipeStep step)
    {
        if (delayPanel) delayPanel.SetActive(true);
        if (step.delayBeforeAudio > 0)
            yield return new WaitForSeconds(step.delayBeforeAudio);
        if (delayPanel) delayPanel.SetActive(false);
        Debug.Log("Playing audio for step: " + step.description + " after a delay of " + step.delayBeforeAudio + " seconds.");
        if (audioSource && step.voiceClip)
            audioSource.PlayOneShot(step.voiceClip);
    }

    // Display the current step in the TextMeshPro component and in the console
    void ShowCurrentStep()
    {
        // Play audio for the current step if available
        var step = currentRecipe[currentStepIndex];
        if (audioSource != null && step.voiceClip != null)
        {
            StopAllCoroutines();
            StartCoroutine(DelayAndPlayAudio(step));
        }

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
        //if (GUI.Button(new Rect(10, 10, 100, 20), "Complete Step"))
        //{
        //    CompleteCurrentStep();
        //}

        //if (GUI.Button(new Rect(10, 30, 100, 20), "Start Timer"))
        //{
        //    StartTimedAction();
        //}

        //if (GUI.Button(new Rect(10, 50, 100, 20), "Stop Timer"))
        //{
        //    StopTimedAction();
        //}

        //if (GUI.Button(new Rect(10, 70, 100, 20), "Add Action"))
        //{
        //    IncrementAction();
        //}
    }
}
