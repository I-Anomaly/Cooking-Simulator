using UnityEngine;

public class CampFire : MonoBehaviour
{
    GameManager gm;
    int count;

    public GameObject fireGameObject; // GameObject to activate when the action count is reached
    public bool isFireOn = false; // Track if the fire is already on
    bool canLightOnFire = false;

    [Header("Audio Settings")]
    [Tooltip("Boom ignite sound (played once)")]
    public AudioSource igniteAudioSource;

    [Tooltip("Looping fire crackling sound (set to loop = true in Inspector)")]
    public AudioSource crackleAudioSource;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        gm = GameManager.Instance;
    }

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
        if (!canLightOnFire)
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
        fireGameObject.SetActive(true);

        // Play ignite sound once
        if (igniteAudioSource != null)
        {
            igniteAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("Ignite AudioSource not assigned!");
        }

        // Play crackle sound in loop after ignite
        if (crackleAudioSource != null)
        {
            crackleAudioSource.loop = true; // ensure looping
            crackleAudioSource.PlayDelayed(igniteAudioSource != null ? igniteAudioSource.clip.length : 0f);
        }
        else
        {
            Debug.LogWarning("Crackle AudioSource not assigned!");
        }

        IncrementProgress();
    }
}
