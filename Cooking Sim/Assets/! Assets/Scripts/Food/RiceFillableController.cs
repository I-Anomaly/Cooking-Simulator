using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiceFillableController : MonoBehaviour
{
    public Transform FillableObject;
    public float CurrentFill = 0.01f;
    public float FillRate = 0.1f;
    float MaxFill;

    // Start is called before the first frame update
    void Start()
    {
        MaxFill = 1;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fill(float amount)
    {

        CurrentFill += amount * FillRate * Time.deltaTime;
        CurrentFill = CurrentFill>MaxFill?MaxFill:CurrentFill;

        FillableObject.localScale = new Vector3(1, CurrentFill, 1);
    }

    // Call this when hit by the rice stream
    public void OnRiceStreamHit()
    {
        Debug.Log("Rice stream hit the fillable object, starting to fill.");
        if (GameManager.Instance != null && GameManager.Instance.currentStepIndex == 7)
        {
            GameManager.Instance.StartTimedAction();
        }
    }
}
