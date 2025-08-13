using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YamLogic : MonoBehaviour
{
    [Header("Mashed Textures")]
    [Tooltip("Assign 3 textures representing each mashed stage.")]
    public Texture[] mashTextures = new Texture[3];

    MeshRenderer meshRenderer;

    GameManager gm;

    public int firstMashStep = 6;
    public int secondMashStep = 8;
    public int lastMashStep = 10;

    int mashCount = 0;

    public GameObject[] yamPieces; // Array to hold yam pieces for the final step
    public GameObject fufuBall;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        gm = GameManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pestle")) {             // Check if the game manager exists and the current step matches the required recipe step
            EnsureGameManager();
            if (gm == null || 
                (gm.currentStepIndex != firstMashStep && 
                 gm.currentStepIndex != secondMashStep && 
                 gm.currentStepIndex != lastMashStep))
            {
                return;
            }
            Debug.Log("Yam has been mashed with the pestle.");
            mashCount++;
            var count = gm.currentRecipe[gm.currentStepIndex];

            if (mashCount >= count.actionCount)
            {
                Debug.Log("Yam is fully mashed, incrementing action.");
                UpdateTexture();
                gm.CompleteCurrentStep();
                if (gm.currentStepIndex == lastMashStep + 1)
                {
                    fufuBall.SetActive(true); // Show the fufu ball after the last step
                    this.gameObject.SetActive(false); // Hide the yam after the last step
                }
            }
        }
    }

    // Update the yam's texture based on the current peel count by incrementing through the 'list' of textures
    private void UpdateTexture()
    {
        if (meshRenderer == null) return;

        // Change texture and enable renderer based on the completed step
        if (gm.currentStepIndex == firstMashStep && mashTextures.Length > 0)
        {
            meshRenderer.enabled = true;
            meshRenderer.material.mainTexture = mashTextures[0];

            // Destroy yam pieces
            if (yamPieces != null)
            {
                foreach (var piece in yamPieces)
                {
                    if (piece != null)
                    {
                        Destroy(piece);
                    }
                }
            }
        }
        else if (gm.currentStepIndex == secondMashStep && mashTextures.Length > 1)
        {
            meshRenderer.material.mainTexture = mashTextures[1];
        }
        else if (gm.currentStepIndex == lastMashStep && mashTextures.Length > 2)
        {
            meshRenderer.material.mainTexture = mashTextures[2];
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
