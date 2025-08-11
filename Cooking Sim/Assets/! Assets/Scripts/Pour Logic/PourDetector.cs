using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PourAudioClip
{
    public AudioSource audioSource;
    public float delayBeforePlay = 0f;
}

public class PourDetector : MonoBehaviour
{
    [Header("Pour Settings")]
    public float pouringThreshold = 0.75f;
    public Transform pourPoint;
    public Stream streamPrefab;

    private Stream currentStream;
    private bool isPouring = false;

    [Header("Fill Settings")]
    public float maxFill = 1f;
    public float fillLevel = 1f;
    public float drainRate = 0.1f;
    public float currentThreshold;

    [Header("Audio Sequence")]
    public List<PourAudioClip> pourAudios = new List<PourAudioClip>();

    void Update()
    {
        currentThreshold = Mathf.Lerp(-1f, pouringThreshold, fillLevel);
        bool pourCheck = CalculateDotProduct() < currentThreshold;

        if (isPouring)
        {
            fillLevel = Mathf.Clamp(fillLevel, 0f, maxFill);
        }

        if (isPouring != pourCheck)
        {
            isPouring = pourCheck;
            if (isPouring)
            {
                currentStream = Instantiate<Stream>(streamPrefab, pourPoint.position, Quaternion.identity, transform);
                currentStream.Begin();

                StopAllCoroutines();
                StartCoroutine(PlayAudioSequence());
            }
            else
            {
                if (currentStream != null)
                {
                    currentStream.End();
                    currentStream = null;
                }

                // Stop all playing audio sources
                foreach (var audioClip in pourAudios)
                {
                    if (audioClip.audioSource != null && audioClip.audioSource.isPlaying)
                        audioClip.audioSource.Stop();
                }
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
        isPouring = false;
        // Stop all playing audio sources
        foreach (var audioClip in pourAudios)
        {
            if (audioClip.audioSource != null && audioClip.audioSource.isPlaying)
                audioClip.audioSource.Stop();
        }

        // Disable this script to stop pouring
        this.enabled = false;
    }

    private float CalculateDotProduct()
    {
        Vector3 upDirection = Vector3.up;
        float dotProduct = Vector3.Dot(transform.up, upDirection);
        return dotProduct;
    }

    private void Refill()
    {
        fillLevel = maxFill;
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(150, 10, 100, 20), "Refill"))
        {
            Refill();
        }
    }

    private IEnumerator PlayAudioSequence()
    {
        foreach (var audioClip in pourAudios)
        {
            if (audioClip.audioSource != null)
            {
                yield return new WaitForSeconds(audioClip.delayBeforePlay);
                audioClip.audioSource.Play();
            }
        }
    }
}
