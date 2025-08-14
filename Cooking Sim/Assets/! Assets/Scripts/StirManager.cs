using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StirManager : MonoBehaviour
{
    private bool isInWater = false;

    [Header("Audio Settings")]
    public AudioSource stirringAudio; // Assign the stirring AudioSource here

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Water"))
            return;

        if (GameManager.Instance.currentStepIndex != 8 || GameManager.Instance.recipeComplete == true)
            return;

        if (!isInWater)
        {
            Debug.Log("Stirring in water");
            isInWater = true;
            GameManager.Instance.StartTimedAction();

            // Start stirring sound
            if (stirringAudio != null && !stirringAudio.isPlaying)
                stirringAudio.Play();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (GameManager.Instance.recipeComplete == true)
        {
            isInWater = false;

            // Stop sound if recipe is done
            if (stirringAudio != null && stirringAudio.isPlaying)
                stirringAudio.Pause();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Water"))
            return;

        if (GameManager.Instance.currentStepIndex != 8 || GameManager.Instance.recipeComplete == true)
            return;

        Debug.Log("Stopped stirring in water");
        isInWater = false;
        GameManager.Instance.StopTimedAction();

        // Pause stirring sound
        if (stirringAudio != null && stirringAudio.isPlaying)
            stirringAudio.Pause();
    }
}
