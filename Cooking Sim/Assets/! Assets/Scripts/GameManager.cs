using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Each step in a recipe and the variables associated with it.
    /// </summary>
    [System.Serializable]
    public class RecipeStep
    {
        [TextArea] public string description;
        public string utensil;
        public int actionCount;
        public ActionType actionType;
        public AudioClip voiceClip;
        public int delayBeforeAudio;
        public string stepID;
        public GameObject[] highlightables;

        // Drag Step1/Step2/... scene objects here
        public GameObject stepImageObject;
    }

    /// <summary>
    /// Action types for recipe steps.
    /// </summary>
    public enum ActionType { InstantAction, NumberOfActions, SecondsPassed, Auto }
    public enum RecipeType { JollofRice, Fufu }

    // Recipe types
    public RecipeType CurrentRecipe { get; set; }
    public RecipeType selectedRecipe = RecipeType.Fufu;

    public List<RecipeStep> jollofRiceRecipe = new();
    public List<RecipeStep> fufuRecipe = new();
    public List<RecipeStep> currentRecipe = new();

    [Header("UI - Instructions")]
    public TextMeshProUGUI stepText;
    public TextMeshProUGUI stepWristText;

    [Header("Progress UI")]
    public Slider progressBar;
    public TextMeshProUGUI stepCounterText;

    [Header("Fancy Transitions")]
    public bool fadeStepImages = true;
    [Range(0.05f, 1.5f)] public float imageFadeDuration = 0.25f;

    public bool animateProgressBar = true;
    [Range(0.05f, 1.5f)] public float progressFillDuration = 0.25f;

    // ===== Completion visuals (option A: two separate images) =====
    [Header("Completion UI (Two-Image Option)")]
    public GameObject jollofCompleteImage;   // UI Image GO (inactive by default)
    public GameObject fufuCompleteImage;     // UI Image GO (inactive by default)
    public bool autoHideCompletionImage = false;
    [Range(0.5f, 10f)] public float completionImageDuration = 3f;

    [Header("Completion Pop Settings")]
    [Range(0.5f, 1.5f)] public float completionStartScale = 0.85f;
    [Range(1.01f, 1.6f)] public float completionPopScale = 1.12f;
    [Range(0.05f, 0.8f)] public float completionPopInDuration = 0.25f;
    [Range(0.05f, 0.8f)] public float completionSettleDuration = 0.15f;

    // ===== Completion visuals (option B: single panel with sprite) =====
    [Header("Completion UI (Single-Panel Option)")]
    public GameObject recipeCompletePanel;   // Image on this GO will be swapped
    public Sprite jollofImage;
    public Sprite fufuImage;

    private Image recipeCompleteImage;       // cached Image component from recipeCompletePanel

    public int currentStepIndex = 0;
    public int actionCount = 0;

    private bool isTiming = false;
    private float elapsedTime = 0f;
    private AudioSource audioSource;

    [Header("Additional Voice Clips")]
    public AudioClip[] additionalVoiceClips;
    public AudioSource audioSourceVoices;

    [Space(10)]
    public bool recipeComplete = false;
    CampFire campFire;

    [Header("UI Panels")]
    public GameObject delayPanel;

    [Header("Jollof Rice Recipe Objects")]
    public GameObject[] jollofVeggies;
    public GameObject mashedTexture;
    public PourDetector mashedPourDetector;
    public int mashedTextureIndex = 4;
    public GameObject mortar;
    [Space(5)]
    public PourDetector oilPourDetector;
    public PourDetector waterPourDetector;
    public RicePourDetector ricePourDetector;

    [Space(10)]
    [Header("Fufu Items")]
    public GameObject boilingEffect;

    [Header("Recipe Complete!")]
    public GameObject particleEffect;
    public AudioClip jollofCelebration;
    public AudioClip fufuCelebration;

    [Header("Highlighting")]
    public string highlightLayerName = "Outlined Objects";
    public string defaultLayerName = "Default";

    private GameObject[] previousHighlightables = null;
    private GameObject _activeStepImageGO = null;
    private Coroutine _barRoutine;
    private Coroutine _imageSwapRoutine;

    static GameManager instance;
    public static GameManager Instance => instance;

    private Color originalStepCounterColor;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Fader.FadeIn(); // Fade in from black at the start of the scene
        currentRecipe.Clear(); // Ensure the current recipe is clear before populating it

        switch (selectedRecipe)
        {
            case RecipeType.JollofRice: currentRecipe = new List<RecipeStep>(jollofRiceRecipe); break;
            case RecipeType.Fufu: currentRecipe = new List<RecipeStep>(fufuRecipe); break;
        }

        if (progressBar != null)
        {
            progressBar.minValue = 0;
            progressBar.maxValue = currentRecipe.Count;
            progressBar.value = 0;
            progressBar.wholeNumbers = false; // we animate between steps
        }

        // Get the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();

        // Reset variables
        currentStepIndex = 0;
        actionCount = 0;
        recipeComplete = false;

        if (stepCounterText != null)
            originalStepCounterColor = stepCounterText.color;

        // Hide step & completion images at start
        HideAllStepImages();
        HideCompletionImagesAtStart();

        // Setup single-panel completion image (if you use option B)
        if (recipeCompletePanel != null)
        {
            recipeCompleteImage = recipeCompletePanel.GetComponent<Image>();
            recipeCompletePanel.SetActive(false);
        }

        // Find the CampFire in the scene, if already on, complete the step
        campFire = FindObjectOfType<CampFire>();
        if (campFire && campFire.isFireOn)
            CompleteCurrentStep();

        ShowCurrentStep();
    }

    private void Update()
    {
        // If the recipe is complete, do nothing
        if (recipeComplete || currentStepIndex >= currentRecipe.Count) return;

        // If we've reached the end of the recipe, do nothing
        if (currentStepIndex >= currentRecipe.Count) return;

        // Handle timing for steps that require it
        var step = currentRecipe[currentStepIndex];

        // Automatically complete steps that are set to Auto after the specified time
        if (step.actionType == ActionType.SecondsPassed && isTiming)
        {
            Debug.Log("Timing step " + currentStepIndex + " for " + step.actionCount + " seconds.");
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= step.actionCount)
            {
                isTiming = false;
                elapsedTime = 0f;
                CompleteCurrentStep();
            }
        }

        if (step.actionType == ActionType.Auto) {
            Debug.Log("Timing step " + currentStepIndex + " for " + step.actionCount + " seconds.");
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
            // LOGIC FOR MASHED TEXTURE IN JOLLOF RICE RECIPE, also gross in layout but it works for now
            if (selectedRecipe == RecipeType.JollofRice && currentStepIndex == mashedTextureIndex && mashedTexture.activeInHierarchy != true)
            {
                Rigidbody rb = mortar.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false;
                XRGrabInteractable grab = mortar.GetComponent<XRGrabInteractable>();
                grab.enabled = true;
                mashedTexture.SetActive(true);
            }
            else if (selectedRecipe == RecipeType.JollofRice)
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

            if (selectedRecipe == RecipeType.Fufu && currentStepIndex == 3 && boilingEffect != null)
            {
                Debug.Log("The boiling effect should be enabled now.");
                boilingEffect.SetActive(true);
            }

            // LOGIC FOR DISABLING THE POURING DETECTORS IN JOLLOF RICE RECIPE, this is so gross but it works for now
            if (selectedRecipe == RecipeType.JollofRice)
            {
                if (currentStepIndex == 5 && oilPourDetector.enabled == true)
                {
                    Debug.Log("The current step index is " + currentStepIndex + " and the oil should be disabled here.");
                    oilPourDetector.StopPouring();
                }
                if (currentStepIndex == 7 && ricePourDetector.enabled == true)
                {
                    Debug.Log("The current step index is " + currentStepIndex + " and the rice should be disabled here.");
                    ricePourDetector.StopPouring();
                }
                if (currentStepIndex == 8 && waterPourDetector.enabled == true)
                {
                    Debug.Log("The current step index is " + currentStepIndex + " and the water should be disabled here.");
                    waterPourDetector.StopPouring();
                }
            }

            ShowCurrentStep();
            return;
        }

        // === RECIPE COMPLETED BEYOND HERE ===
        recipeComplete = true;

        if (stepText) stepText.text = "Recipe Complete!";
        if (stepWristText) stepWristText.text = "Recipe Complete!";

        if (stepCounterText)
        {
            stepCounterText.text = $"Step {currentRecipe.Count}/{currentRecipe.Count}";
            stepCounterText.color = Color.green;
        }

        SmoothFill(progressBar != null ? progressBar.maxValue : 0f);

        if (_activeStepImageGO) SetGOVisible(_activeStepImageGO, false, imageFadeDuration);

        if (particleEffect)
            Instantiate(particleEffect, stepText ? stepText.transform.position : transform.position, Quaternion.identity);

        StopAllCoroutines();

        if (selectedRecipe == RecipeType.JollofRice && jollofCelebration)
        {
            Debug.Log("Playing Jollof Rice celebration audio");
            if (audioSourceVoices) audioSourceVoices.PlayOneShot(jollofCelebration);
        }
        else if (selectedRecipe == RecipeType.Fufu && fufuCelebration)
        {
            Debug.Log("Playing Fufu celebration audio");
            if (audioSourceVoices) audioSourceVoices.PlayOneShot(fufuCelebration);
        }

        // Option A: show a specific completion image GO with pop
        GameObject completionGO = (selectedRecipe == RecipeType.JollofRice) ? jollofCompleteImage : fufuCompleteImage;
        if (completionGO) StartCoroutine(ShowCompletionWithPop(completionGO));

        // Option B: show single panel with sprite and scale to 3x
        if (recipeCompleteImage && recipeCompletePanel)
        {
            recipeCompleteImage.sprite = (selectedRecipe == RecipeType.JollofRice) ? jollofImage : fufuImage;
            recipeCompletePanel.SetActive(true);
            StartCoroutine(ScaleUpRecipeImage()); // scales to 3x
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
        Debug.Log("Incrementing action count for step " + currentStepIndex);
        actionCount++;
        if (actionCount >= currentRecipe[currentStepIndex].actionCount)
            CompleteCurrentStep();
    }

    public void ReduceAction()
    {
        if (actionCount > 0) actionCount--;
    }
    #endregion

    /// <summary>
    /// Delays for the specified time before playing the audio clip for the given step.
    /// </summary>
    /// <param name="step">Recipe step number</param>
    /// <returns></returns>
    IEnumerator DelayAndPlayAudio(RecipeStep step)
    {
        if (delayPanel)
            delayPanel.SetActive(true);
        if (step.delayBeforeAudio > 0)
            yield return new WaitForSeconds(step.delayBeforeAudio);
        if (delayPanel) delayPanel.SetActive(false);

        if (audioSourceVoices != null)
        {
            if (step.voiceClip)
            {
                // Assign it a clip directly and play
                if (audioSourceVoices.clip != null)
                {
                    audioSourceVoices.clip = null; // clear existing clip
                }
                audioSourceVoices.clip = step.voiceClip;
                audioSourceVoices.Play();
            }
            else if (additionalVoiceClips != null && additionalVoiceClips.Length > 0)
                audioSourceVoices.PlayOneShot(additionalVoiceClips[Random.Range(0, additionalVoiceClips.Length)]);
        }
    }

    #region UI Management
    /// <summary>
    /// Shows the current step in the UI and plays the associated audio clip.
    /// </summary>
    void ShowCurrentStep()
    {
        var step = currentRecipe[currentStepIndex];

        StopAllCoroutines();

        if (step.voiceClip != null)
            StartCoroutine(DelayAndPlayAudio(step));

        // Update all highlightables to the current step
        UpdateHighlightables();

        if (stepText)
            stepText.text = $"{step.description}";

        if (stepText)
            stepWristText.text = $"{step.description}";

        if (stepCounterText)
        {
            stepCounterText.text = $"Step {currentStepIndex + 1}/{currentRecipe.Count}";
            stepCounterText.color = originalStepCounterColor;
        }

        // Smoothly fill to the current (pre-completion) step index
        SmoothFill(currentStepIndex);

        // Swap step image GameObjects (with fade)
        SwapStepImage(step.stepImageObject);
    }

    // --------- Fancy bits ---------

    void SmoothFill(float targetValue)
    {
        if (!progressBar) return;

        if (!animateProgressBar)
        {
            progressBar.value = targetValue;
            return;
        }

        if (_barRoutine != null) StopCoroutine(_barRoutine);
        _barRoutine = StartCoroutine(SmoothFillRoutine(targetValue, progressFillDuration));
    }

    IEnumerator SmoothFillRoutine(float target, float duration)
    {
        float t = 0f;
        float start = progressBar.value;
        if (Mathf.Approximately(start, target)) yield break;

        while (t < duration)
        {
            t += Time.deltaTime;
            progressBar.value = Mathf.Lerp(start, target, Mathf.Clamp01(t / duration));
            yield return null;
        }
        progressBar.value = target;
    }

    void SwapStepImage(GameObject nextGO)
    {
        if (!fadeStepImages)
        {
            if (_activeStepImageGO) _activeStepImageGO.SetActive(false);
            if (nextGO) nextGO.SetActive(true);
            _activeStepImageGO = nextGO;
            return;
        }

        if (_imageSwapRoutine != null) StopCoroutine(_imageSwapRoutine);
        _imageSwapRoutine = StartCoroutine(SwapStepImageRoutine(nextGO));
    }

    IEnumerator SwapStepImageRoutine(GameObject nextGO)
    {
        // fade out current
        if (_activeStepImageGO)
        {
            yield return FadeGO(_activeStepImageGO, 0f, imageFadeDuration * 0.5f);
            _activeStepImageGO.SetActive(false);
        }

        // fade in next
        _activeStepImageGO = nextGO;
        if (_activeStepImageGO)
        {
            var cg = GetOrAddCanvasGroup(_activeStepImageGO);
            cg.alpha = 0f;
            _activeStepImageGO.SetActive(true);
            yield return FadeGO(_activeStepImageGO, 1f, imageFadeDuration * 0.5f);
        }
    }

    IEnumerator FadeGO(GameObject go, float targetAlpha, float duration)
    {
        if (!go) yield break;
        var cg = GetOrAddCanvasGroup(go);
        float start = cg.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(t / duration));
            yield return null;
        }
        cg.alpha = targetAlpha;
    }

    void SetGOVisible(GameObject go, bool visible, float fadeTime = 0f)
    {
        if (!go) return;
        if (fadeTime <= 0f)
        {
            go.SetActive(visible);
            var cg = go.GetComponent<CanvasGroup>();
            if (cg) cg.alpha = visible ? 1f : 0f;
            return;
        }
        StartCoroutine(visible
            ? FadeInAndEnable(go, fadeTime)
            : FadeGO(go, 0f, fadeTime));
    }

    IEnumerator FadeInAndEnable(GameObject go, float duration)
    {
        var cg = GetOrAddCanvasGroup(go);
        cg.alpha = 0f;
        go.SetActive(true);
        yield return FadeGO(go, 1f, duration);
    }

    CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    // --------- Completion helpers ---------

    private IEnumerator ScaleUpRecipeImage()
    {
        if (!recipeCompletePanel) yield break;

        float duration = 0.5f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * 3f;

        float time = 0;
        recipeCompletePanel.transform.localScale = startScale;

        while (time < duration)
        {
            recipeCompletePanel.transform.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        recipeCompletePanel.transform.localScale = endScale;
    }

    IEnumerator ShowCompletionWithPop(GameObject imageGO)
    {
        if (!imageGO) yield break;

        var cg = GetOrAddCanvasGroup(imageGO);
        var rt = imageGO.GetComponent<RectTransform>();
        if (!rt) rt = imageGO.AddComponent<RectTransform>();

        Vector3 baseScale = Vector3.one;

        imageGO.SetActive(true);
        cg.alpha = 0f;
        rt.localScale = baseScale * completionStartScale;

        // Fade in + pop to completionPopScale
        float t = 0f;
        while (t < completionPopInDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / completionPopInDuration);
            cg.alpha = a;
            rt.localScale = Vector3.Lerp(baseScale * completionStartScale, baseScale * completionPopScale, a);
            yield return null;
        }
        cg.alpha = 1f;
        rt.localScale = baseScale * completionPopScale;

        // Settle to 3x final size
        t = 0f;
        Vector3 targetScale = baseScale * 3f;
        while (t < completionSettleDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / completionSettleDuration);
            rt.localScale = Vector3.Lerp(baseScale * completionPopScale, targetScale, a);
            yield return null;
        }
        rt.localScale = targetScale;

        // Optional auto-hide
        if (autoHideCompletionImage)
        {
            yield return new WaitForSeconds(completionImageDuration);

            float d = imageFadeDuration;
            float startA = cg.alpha;
            t = 0f;
            while (t < d)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(startA, 0f, Mathf.Clamp01(t / d));
                yield return null;
            }
            cg.alpha = 0f;
            imageGO.SetActive(false);
        }
    }

    void HideCompletionImagesAtStart()
    {
        if (jollofCompleteImage)
        {
            var cg = GetOrAddCanvasGroup(jollofCompleteImage);
            cg.alpha = 0f;
            jollofCompleteImage.SetActive(false);
        }
        if (fufuCompleteImage)
        {
            var cg = GetOrAddCanvasGroup(fufuCompleteImage);
            cg.alpha = 0f;
            fufuCompleteImage.SetActive(false);
        }
        if (recipeCompletePanel)
        {
            var cg = GetOrAddCanvasGroup(recipeCompletePanel);
            cg.alpha = 1f; // we animate via scale; keep fully visible when enabled
            recipeCompletePanel.SetActive(false);
        }
    }

    void HideAllStepImages()
    {
        foreach (var s in jollofRiceRecipe)
            if (s != null && s.stepImageObject)
            {
                var cg = GetOrAddCanvasGroup(s.stepImageObject);
                cg.alpha = 0f;
                s.stepImageObject.SetActive(false);
            }

        foreach (var s in fufuRecipe)
            if (s != null && s.stepImageObject)
            {
                var cg = GetOrAddCanvasGroup(s.stepImageObject);
                cg.alpha = 0f;
                s.stepImageObject.SetActive(false);
            }
    }
    #endregion

    // --------- Existing helpers ---------

    void UpdateHighlightables()
    {
        if (previousHighlightables != null)
            SetObjectsLayer(previousHighlightables, defaultLayerName);

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
            if (obj) SetLayerRecursively(obj, layer);
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
    #endregion

    public void StartTimedAction() => isTiming = true;
    public void StopTimedAction() => isTiming = false;

    // For testing purposes only: Complete the current step when the button is pressed
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 80, 100, 20), "Complete Step"))
            CompleteCurrentStep();
    }


}
