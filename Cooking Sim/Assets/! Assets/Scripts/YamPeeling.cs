using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YamPeeling : MonoBehaviour
{
    [Header("Peeling Textures")]
    [Tooltip("Assign 4 textures representing each peeling stage.")]
    public Texture[] peelingTextures = new Texture[4];

    [Header("Peel Step")]
    [Tooltip("Set this to the required recipe step for peeling.")]
    public int requiredRecipeStep = 1;

    [Header("Boil Step")]
    [Tooltip("Set this to the required recipe step for boiling.")]
    public int requiredBoilStep = 3;

    [Header("Mortar and Pestle Step")]
    [Tooltip("Set this to the required recipe step for using mortar and pestle.")]
    public int requiredMortarAndPestleStep = 5;

    [Header("References")]
    [Tooltip("Assign the Renderer component of the yam.")]
    public Renderer yamRenderer;

    [Header("Peeling Sounds")]
    [Tooltip("Assign one or more yam peeling sounds here.")]
    public AudioClip[] peelingSounds;
    private AudioSource audioSource;

    private int peelCount = 0;

    // Reference to GameManager (assumes a singleton pattern)
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;

        if (yamRenderer == null)
            yamRenderer = GetComponent<Renderer>();

        // Setup AudioSource if not already attached
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        UpdateTexture();
    }

    // Call this method from the Knife script when the yam is hit
    public void OnCollisionEnter(Collision collision)
    {
        // Ensure the game manager exists and the current step matches the required recipe step
        if (gameManager == null || gameManager.currentStepIndex != requiredRecipeStep)
            return;

        if (!collision.gameObject.CompareTag("Knife"))
            return;

        // Play a random peeling sound
        PlayPeelingSound();

        if (peelCount < peelingTextures.Length)
        {
            peelCount++;
            UpdateTexture();

            if (peelCount == peelingTextures.Length)
            {
                // Progress the recipe
                gameManager.IncrementAction();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance == null)
            return;

        if (other.CompareTag("Water") && GameManager.Instance.currentStepIndex == requiredBoilStep)
        {
            Debug.Log("Yam is in water, incrementing action.");
            gameManager.IncrementAction();
        }
        else if (other.CompareTag("Mortar") && GameManager.Instance.currentStepIndex == requiredMortarAndPestleStep)
        {
            Debug.Log("Yam is in mortar and pestle, incrementing action.");
            gameManager.IncrementAction();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (GameManager.Instance == null)
            return;

        if (other.CompareTag("Water") && GameManager.Instance.currentStepIndex == requiredBoilStep)
        {
            Debug.Log("Yam is out of water, reducing action.");
            gameManager.ReduceAction();
        }
        else if (other.CompareTag("Mortar") && GameManager.Instance.currentStepIndex == requiredMortarAndPestleStep)
        {
            Debug.Log("Yam is out of mortar and pestle, reducing action.");
            gameManager.ReduceAction();
        }
    }

    // Update the yam's texture based on the current peel count by incrementing through the 'list' of textures
    private void UpdateTexture()
    {
        if (yamRenderer != null && peelCount > 0 && peelCount <= peelingTextures.Length)
        {
            yamRenderer.material.mainTexture = peelingTextures[peelCount - 1];
        }
    }

    private void PlayPeelingSound()
    {
        if (peelingSounds.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, peelingSounds.Length);
            audioSource.PlayOneShot(peelingSounds[randomIndex]);
        }
    }
}
