using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RicePourDetector : MonoBehaviour
{
    public float PourThreshold = 0.75f;
    public Transform PourOrigin;
    public RiceStream StreamPrefab;

    public float fillLevel = 1.0f;
    public float drainRate = 0.1f;
    public float currentThreshold;

    private bool isPouring;
    private RiceStream currentStream;

    [Header("Audio Settings")]
    public AudioSource pourAudioSource; // Assign your pour sound AudioSource here

    void Update()
    {
        currentThreshold = Mathf.Lerp(-1f, PourThreshold, fillLevel);
        bool pourcheck = CalculateDotProduct() < currentThreshold;

        if (isPouring)
        {
            fillLevel -= drainRate * Time.deltaTime;
        }

        if (isPouring != pourcheck)
        {
            isPouring = pourcheck;

            if (isPouring)
            {
                // Start rice stream
                currentStream = Instantiate(StreamPrefab, PourOrigin.position, Quaternion.identity);
                currentStream.followSource = PourOrigin;
                currentStream.Begin();

                // Start pouring sound
                if (pourAudioSource != null && !pourAudioSource.isPlaying)
                    pourAudioSource.Play();
            }
            else
            {
                // End rice stream
                if (currentStream != null)
                {
                    currentStream.End();
                    currentStream = null;
                }

                // Stop pouring sound
                if (pourAudioSource != null && pourAudioSource.isPlaying)
                    pourAudioSource.Stop();
            }
        }
    }

    public void StopPouring()
    {
        if (currentStream != null)
        {
            currentStream.End();
            currentStream = null;
        }

        if (pourAudioSource != null && pourAudioSource.isPlaying)
            pourAudioSource.Stop();

        isPouring = false;
        this.enabled = false;
    }

    private float CalculateDotProduct()
    {
        return Vector3.Dot(transform.up, Vector3.up);
    }
}