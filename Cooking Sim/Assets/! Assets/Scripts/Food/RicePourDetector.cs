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

    // Update is called once per frame
    void Update()
    {
        currentThreshold = Mathf.Lerp(-1f, PourThreshold, fillLevel);
        bool pourcheck = CalculateDotProduct() < currentThreshold;
        if (pourcheck)
            // Debug.Log("Pour Water!");

            if (isPouring)
            {

                fillLevel -= drainRate * Time.deltaTime;
            }
        if (isPouring != pourcheck)
        {
            isPouring = pourcheck;

            if (isPouring)
            {
                currentStream = Instantiate(StreamPrefab, PourOrigin.position, Quaternion.identity);
                currentStream.followSource = PourOrigin;
                currentStream.Begin();
            }
            else
            {
                currentStream.End();
                currentStream = null;
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

        // Disable this script to stop pouring
        this.enabled = false;
    }

    private float CalculateDotProduct()
    {
        return Vector3.Dot(transform.up, Vector3.up);
    }
}