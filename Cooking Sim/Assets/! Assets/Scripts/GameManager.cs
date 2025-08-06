using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{
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

    public enum ActionType
    {
        InstantAction,
        NumberOfActions,
        SecondsPassed,
        Auto
    }

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
        Fader.FadeIn();
        currentRecipe.Clear();

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
        recipeComplete = false;

        campFire = FindObjectOfType<CampFire>();
        if (campFire && campFire.isFireOn)
        {
            CompleteCurrentStep();
        }

        ShowCurrentStep();
    }

    private void Update()
    {
        if (recipeComplete) return;

        if (currentStepIndex >= currentRecipe.Count) return;

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

    public void CompleteCurrentStep()
    {
        actionCount = 0;
        currentStepIndex++;

        if (currentStepIndex < currentRecipe.Count)
        {
            // Activate the mashed texture, otherwise disable it
            if (selectedRecipe == RecipeType.JollofRice && currentStepIndex == mashedTextureIndex && mashedTexture.activeInHierarchy != true)
            {
                mashedTexture.SetActive(true);
                mashedPourDetector.enabled = true;

                XRGrabInteractable grab = mortar.GetComponent<XRGrabInteractable>();
                if (grab != null) grab.enabled = true;

                Rigidbody rb = mortar.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false;
            } else
            {
                if (mashedTexture.activeInHierarchy == true)
                {
                    Debug.Log("The mashed texture is active");
                    if (currentStepIndex == (mashedTextureIndex+2))
                    {
                        Debug.Log("The current step index is " + currentStepIndex + " and the mashed texture should be disabled here.");
                        mashedTexture.SetActive(false);
                        mashedPourDetector.enabled = false;
                    }
                }
                    
            }

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

    public void StartTimedAction()
    {
        isTiming = true;
    }

    public void StopTimedAction()
    {
        isTiming = false;
    }

    IEnumerator DelayAndPlayAudio(RecipeStep step)
    {
        if (delayPanel) delayPanel.SetActive(true);

        if (step.delayBeforeAudio > 0)
            yield return new WaitForSeconds(step.delayBeforeAudio);

        if (delayPanel) delayPanel.SetActive(false);

        if (audioSource != null)
        {
            if (step.voiceClip != null)
            {
                audioSource.PlayOneShot(step.voiceClip);
            }
            else if (additionalVoiceClips != null && additionalVoiceClips.Length > 0)
            {
                AudioClip fallback = additionalVoiceClips[Random.Range(0, additionalVoiceClips.Length)];
                audioSource.PlayOneShot(fallback);
                Debug.Log("Playing fallback voice clip: " + fallback.name);
            }
        }
    }

    void ShowCurrentStep()
    {
        var step = currentRecipe[currentStepIndex];
        StopAllCoroutines();
        StartCoroutine(DelayAndPlayAudio(step));

        UpdateHighlightables();

        if (stepText != null)
        {
            stepText.text = step.description + " (" + step.utensil + ")";
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 80, 100, 20), "Complete Step"))
        {
            CompleteCurrentStep();
        }
    }
}
