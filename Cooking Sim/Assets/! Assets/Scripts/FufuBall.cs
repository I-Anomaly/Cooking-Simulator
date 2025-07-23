using UnityEngine;

public class FufuBall : MonoBehaviour
{
    GameManager gm;
    int actionCount = 0;
    int hitCount = 0;

    void Start()
    {
        gm = GameManager.Instance;
        if (gm != null && gm.currentRecipe != null)
        {
            actionCount = gm.currentRecipe[gm.currentStepIndex].actionCount;
            Debug.Log("The action count for this step is: " + actionCount);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("XRController"))
        {
            hitCount++;
            Debug.Log("Fufu ball hit count: " + hitCount);
            if (hitCount >= actionCount && gm != null)
            {
                gm.CompleteCurrentStep();
                hitCount = 0; // Reset if you want to allow for repeated steps
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("XRController"))
        {
            hitCount++;
            Debug.Log("Fufu ball hit count: " + hitCount);
            if (hitCount >= actionCount && gm != null)
            {
                gm.CompleteCurrentStep();
                hitCount = 0; // Reset if you want to allow for repeated steps
            }
        }
    }

    public void FufuBalled()
    {
        hitCount++;
        Debug.Log("Fufu ball hit count: " + hitCount);
        if (hitCount >= actionCount && gm != null)
        {
            gm.CompleteCurrentStep();
            hitCount = 0; // Reset if you want to allow for repeated steps
        }
    }
}
