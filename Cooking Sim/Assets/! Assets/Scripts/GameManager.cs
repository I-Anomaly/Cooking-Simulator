using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Each step in a recipe and the variables associated with it.
    /// </summary>
    [System.Serializable]
    public class RecipeStep
    {
        public string description;
        public string utensil;
        [Tooltip("The 'time' or 'action count' required for this step, e.g., 3 slices, 5 seconds, etc.")]
        public int actionCount;
        public ActionType actionType;
        public AudioClip voiceClip;
        public int delayBeforeAudio;
        public string stepID;
        public GameObject[] highlightables;
    }

    /// <summary>
    /// Action types for recipe steps.
    /// </summary>
    public enum ActionType
    {
        InstantAction,
        NumberOfActions,
        SecondsPassed,
        Auto
    }

    // Recipe types
    public enum RecipeType { JollofRice, Fufu }
    public RecipeType CurrentRecipe { get; set; }
    public RecipeType selectedRecipe = RecipeType.Fufu;

    public List<RecipeStep> jollofRiceRecipe = new List<RecipeStep>
    {
        new() { description = "Slice tomatoes and place in mortar", utensil = "Mortar", actionCount = 3, actionType = ActionType.NumberOfActions },
        new() { description = "Slice peppers and place in mortar", utensil = "Mortar", actionCount = 2, actionType = ActionType.NumberOfActions },
        new() { description = "Slice onions and place in mortar", utensil = "Mortar", actionCount = 2, actionType = ActionType.NumberOfActions },
        new() { description = "Ground everything into a paste", utensil = "Pestle", actionCount = 5, actionType = ActionType.NumberOfActions },
        new() { description = "Add water to pot and bring to a boil", utensil = "Pot", actionCount = 1, actionType = ActionType.InstantAction },
        new() { description = "Add paste, spices, and meat to the stew", utensil = "Pot", actionCount = 3, actionType = ActionType.InstantAction },
        new() { description = "Add rice to the pot; it will absorb the stew into it", utensil = "Pot", actionCount = 3, actionType = ActionType.SecondsPassed },
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
    public TextMeshProUGUI stepText;
    public int currentStepIndex = 0;
    public int actionCount = 0;

    private bool isTiming = false;
    private float elapsedTime = 0f;

    private AudioSource audioSource;

    [Header("Additional Voice Clips")]
    public AudioClip[] additionalVoiceClips;  // Optional voice fallback
    public AudioSource audioSourceVoices;

    [Space(10)]
    public bool recipeComplete = false;
    CampFire campFire;

    [Header("UI")]
    public GameObject delayPanel;

    [Space(10)]
    [Header("Jollof Rice Recipe Objects")]
    public GameObject[] jollofVeggies;
    public GameObject mashedTexture;
    public int mashedTextureIndex = 4;
    public PourDetector mashedPourDetector;
    public GameObject mortar;

    [Space(10)]
    [Header("Fufu Items")]

    [Space(10)]
    [Header("Recipe Complete!")]
    public GameObject particleEffect;
    public AudioClip celebration;

    [Space(10)]
    [Header("Highlighting")]
    public string highlightLayerName = "Outlined Objects";
    public string defaultLayerName = "Default";

    private GameObject[] previousHighlightables = null;

    static GameManager instance;
    public static GameManager Instance => instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Fader.FadeIn(); // Fade in from black at the start of the scene
        currentRecipe.Clear(); // Ensure the current recipe is clear before populating it

        // Set the current recipe based on the selected recipe
        switch (selectedRecipe)
        {
            case RecipeType.JollofRice:
                currentRecipe = new List<RecipeStep>(jollofRiceRecipe);
                break;
            case RecipeType.Fufu:
                currentRecipe = new List<RecipeStep>(fufuRecipe);
                break;
        }

        // Get the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();

        // Reset variables
        currentStepIndex = 0;
        actionCount = 0;
        recipeComplete = false;

        // Find the CampFire in the scene, if already on, complete the step
        campFire = FindObjectOfType<CampFire>();
        if (campFire && campFire.isFireOn)
        {
            CompleteCurrentStep();
        }

        // Show the first step
        ShowCurrentStep();
    }

    private void Update()
    {
        // If the recipe is complete, do nothing
        if (recipeComplete) return;

        // If we've reached the end of the recipe, do nothing
        if (currentStepIndex >= currentRecipe.Count) return;

        // Handle timing for steps that require it
        var step = currentRecipe[currentStepIndex];

        if (step.actionType == ActionType.SecondsPassed && isTiming)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= step.actionCount)
            {
                isTiming = false;
                elapsedTime = 0f;
                CompleteCurrentStep();
            }
        }

        // Automatically complete steps that are set to Auto after the specified time
        if (step.actionType == ActionType.Auto)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= step.actionCount)
            {
                isTiming = false;
                elapsedTime = 0f;
                CompleteCurrentStep();
            }
        }
    }

    #region Step Completion
    /// <summary>
    /// 'Completes' the current step if audio is not playing; otherwise, waits for audio to finish before completing the step.
    /// </summary>
    public void CompleteCurrentStep()
    {
        Debug.Log("Attempting to complete step " + currentStepIndex);
        // If voice audio is playing, wait for it to finish before completing the step
        if (audioSourceVoices != null && audioSourceVoices.isPlaying)
        {
            Debug.Log("Voice audio is playing, waiting to complete step.");
            StartCoroutine(WaitForVoiceAndCompleteStep());
            return;
        }

        ActuallyCompleteCurrentStep();

    }

    /// <summary>
    /// Actually completes the current step, increments the step index, and shows the next step.
    /// This will also enable the mashed texture logic for the Jollof Rice recipe.
    /// </summary>
    private void ActuallyCompleteCurrentStep()
    {
        Debug.Log("ACTUALLY completing step " + currentStepIndex);
        actionCount = 0;
        currentStepIndex++;

        
        if (currentStepIndex < currentRecipe.Count)
        {
            #region Mashed Texture Logic
            // Activate the mashed texture, otherwise disable it
            if (selectedRecipe == RecipeType.JollofRice && currentStepIndex == mashedTextureIndex && mashedTexture.activeInHierarchy != true)
            {
                mashedTexture.SetActive(true);
                mashedPourDetector.enabled = true;

                XRGrabInteractable grab = mortar.GetComponent<XRGrabInteractable>();
                if (grab != null) grab.enabled = true;

                Rigidbody rb = mortar.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false;
            }
            else
            {
                if (mashedTexture.activeInHierarchy == true)
                {
                    Debug.Log("The mashed texture is active");
                    if (currentStepIndex == (mashedTextureIndex + 2))
                    {
                        Debug.Log("The current step index is " + currentStepIndex + " and the mashed texture should be disabled here.");
                        mashedTexture.SetActive(false);
                        mashedPourDetector.enabled = false;
                    }
                }
            }
            #endregion

            ShowCurrentStep();
        }
        else
        {
            recipeComplete = true;
            if (stepText != null) stepText.text = "Recipe Complete!";
            if (particleEffect != null) Instantiate(particleEffect, stepText.transform.position, Quaternion.identity);
            if (audioSource != null && celebration != null) audioSource.PlayOneShot(celebration);
        }
        
    }

    /// <summary>
    /// Checks if the voice audio is playing, and waits for it to finish before completing the step.
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForVoiceAndCompleteStep()
    {
        while (audioSourceVoices != null && audioSourceVoices.isPlaying)
        {
            yield return null;
        }
        ActuallyCompleteCurrentStep();
    }
    #endregion

    #region Action Management
    public void IncrementAction()
    {
        actionCount++;
        if (actionCount >= currentRecipe[currentStepIndex].actionCount)
        {
            CompleteCurrentStep();
        }
    }

    public void ReduceAction()
    {
        if (actionCount > 0) actionCount--;
    }

    void UpdateHighlightables()
    {
        if (previousHighlightables != null)
        {
            SetObjectsLayer(previousHighlightables, defaultLayerName);
        }

        var step = currentRecipe[currentStepIndex];
        if (step.highlightables != null && step.highlightables.Length > 0)
        {
            SetObjectsLayer(step.highlightables, highlightLayerName);
            previousHighlightables = step.highlightables;
        }
        else
        {
            previousHighlightables = null;
        }
    }

    #region Layer Management to fix collision issues
    void SetObjectsLayer(GameObject[] objects, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        foreach (var obj in objects)
        {
            if (obj != null)
            {
                SetLayerRecursively(obj, layer);
            }
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    #endregion

    public void StartTimedAction()
    {
        isTiming = true;
    }

    public void StopTimedAction()
    {
        isTiming = false;
    }
    #endregion

    /// <summary>
    /// Delays for the specified time before playing the audio clip for the given step.
    /// </summary>
    /// <param name="step">Recipe step number</param>
    /// <returns></returns>
    IEnumerator DelayAndPlayAudio(RecipeStep step)
    {
        if (delayPanel) delayPanel.SetActive(true);

        Debug.Log("Delaying for " + step.delayBeforeAudio + " seconds before playing audio.");
        if (step.delayBeforeAudio > 0)
            yield return new WaitForSeconds(step.delayBeforeAudio);

        if (delayPanel) delayPanel.SetActive(false);

        // Use the assigned voice clip, or fallback to a random additional voice clip if none is assigned, and use the audio source located on the VoiceOver child in GameManager
        if (audioSourceVoices != null)
        {
            if (step.voiceClip != null)
            {
                Debug.Log("Playing voice clip: " + step.voiceClip.name);
                audioSourceVoices.PlayOneShot(step.voiceClip);
            }
            else if (additionalVoiceClips != null && additionalVoiceClips.Length > 0)
            {
                AudioClip fallback = additionalVoiceClips[Random.Range(0, additionalVoiceClips.Length)];
                audioSourceVoices.PlayOneShot(fallback);
                Debug.Log("Playing fallback voice clip: " + fallback.name);
            }
        }
    }

    /// <summary>
    /// Shows the current step in the UI and plays the associated audio clip.
    /// </summary>
    void ShowCurrentStep()
    {
        var step = currentRecipe[currentStepIndex];

        StopAllCoroutines();

        if(step.delayBeforeAudio > 0 && step.voiceClip != null)
            StartCoroutine(DelayAndPlayAudio(step));

        // Update highlightables
        UpdateHighlightables();

        // Update the step text UI
        if (stepText != null)
        {
            stepText.text = step.description + " (" + step.utensil + ")";
        }
    }

    // For testing purposes only: Complete the current step when the button is pressed
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 80, 100, 20), "Complete Step"))
        {
            CompleteCurrentStep();
        }
    }
}