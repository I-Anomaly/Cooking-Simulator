using UnityEngine;

public class FufuBall : MonoBehaviour
{
    GameManager gm;
    int actionCount = 0;
    int hitCount = 0;

    [Header("Ball Mesh Scaling")]
    public Transform ballMesh; // Assign your child mesh in the inspector
    public Vector3[] ballScales; // Set desired scales for each hit in the inspector
    public GameObject finishedFufuBallPrefab;
    public GameObject particleEffect;

    void Start()
    {
        gm = GameManager.Instance;
        if (gm != null && gm.currentRecipe != null)
        {
            actionCount = gm.currentRecipe[gm.currentStepIndex].actionCount;
            Debug.Log("The action count for this step is: " + actionCount);
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

    public void FufuBalled()
    {
        if (gm == null)
        {
            EnsureGameManager();
            actionCount = gm.currentRecipe[gm.currentStepIndex].actionCount;
            if (gm == null) return; // Exit if GameManager is still null
        }

        hitCount++;
        Debug.Log("Fufu ball hit count: " + hitCount);

        // Change scale if within range
        if (ballScales != null && hitCount - 1 < ballScales.Length)
        {
            ballMesh.localScale = ballScales[hitCount - 1];
        }
        if (hitCount >= actionCount && gm != null)
        {
            gm.CompleteCurrentStep();
            EnableAllFufuBalls();
            hitCount = 0;
        }
    }

    public void EnableAllFufuBalls()
    {
        if (finishedFufuBallPrefab == null) return;

        Vector3 basePosition = transform.position;
        float offset = 0.2f; // Adjust as needed to spread out the balls

        for (int i = 0; i < 3; i++)
        {
            Vector3 spawnPos = basePosition + new Vector3(i * offset, 0, 0);
            Instantiate(finishedFufuBallPrefab, spawnPos, Quaternion.identity);
        }

        if (particleEffect != null)
        {
            Instantiate(particleEffect, basePosition, Quaternion.identity);
        }

        Destroy(gameObject); // Destroy the original FufuBall object
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 200, 100, 20), "Fufu Squeeze"))
        {
            FufuBalled();
        }
    }
}
