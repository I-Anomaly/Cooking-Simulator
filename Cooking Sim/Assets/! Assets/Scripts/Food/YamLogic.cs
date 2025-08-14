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
        EnsureGameManager();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pestle"))
        {
            if (gm.currentStepIndex != firstMashStep &&
                 gm.currentStepIndex != secondMashStep &&
                 gm.currentStepIndex != lastMashStep)
            {
                return;
            }

            Debug.Log("Yam has been mashed with the pestle.");
            mashCount++;
            var count = gm.currentRecipe[gm.currentStepIndex];

            if (mashCount >= count.actionCount)
            {
                Debug.Log("Yam is fully mashed, incrementing action. " + mashCount + "/" + count.actionCount);
                UpdateTexture();
                gm.CompleteCurrentStep();
                mashCount = 0; // Reset mash count for the next step
            }
        }
    }

    // Update the yam's texture based on the current peel count by incrementing through the 'list' of textures
    private void UpdateTexture()
    {
        Debug.Log("Updating yam texture for step: " + gm.currentStepIndex + ". The first mash step is " + firstMashStep);
        if (meshRenderer == null) return;

        // Change texture and enable renderer based on the completed step
        if (gm.currentStepIndex == firstMashStep && mashTextures.Length > 0)
        {
            Debug.Log("Setting first mash texture and destroying yams.");
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
