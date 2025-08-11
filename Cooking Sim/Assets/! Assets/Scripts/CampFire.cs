using UnityEngine;

public class CampFire : MonoBehaviour
{
    GameManager gm;

    int count;

    public GameObject fireGameObject; // GameObject to activate when the action count is reached

    public bool isFireOn = false; // Track if the fire is already on
    bool canLightOnFire = false;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get the game manager instance
        gm = GameManager.Instance;

    }

    // Increment the action count when the player interacts with the campfire, the -1 is just because the additional 'action' is the fire lighting
    public void IncrementProgress()
    {
        count++;
        if (count >= (gm.currentRecipe[gm.currentStepIndex].actionCount - 1) && isFireOn == false)
        {
            canLightOnFire = true;
        }
        gm.IncrementAction();
    }

    public void DecrementProgress()
    {
        if (count > 0)
        {
            count--;
            gm.ReduceAction();
            canLightOnFire = false; // Reset the ability to light fire if count goes below required
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Flame"))
        {
            Debug.Log("Flame has been brought over");
            LightFire();
        }
        else
        {
            Debug.Log("Collision with non-flame object: " + other.gameObject.name);
        }
    }

    public void LightFire()
    {
        if (canLightOnFire == false)
        {
            Debug.Log("Cannot light fire yet, not enough actions completed.");
            return;
        }

        if (isFireOn)
        {
            Debug.Log("Fire is already on, no need to light again.");
            return;
        }

        Debug.Log("Enable fire effect");
        if (fireGameObject == null)
        {
            Debug.LogError("fireGameObject is not assigned!");
            return;
        }
        isFireOn = true;
        fireGameObject.SetActive(true); // Ensure the object is enabled
        IncrementProgress();
    }
}
