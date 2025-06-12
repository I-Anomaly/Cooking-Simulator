using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillableController : MonoBehaviour
{
    public Transform fillableObject; // The object that will be filled
    public float currentFill = 0.01f; // Current fill amount
    public float fillRate = 0.1f; // Rate at which the object fills, 10% per second

    public float maxFill;

    // Start is called before the first frame update
    void Start()
    {
        maxFill = 0.1f; // Assuming the fillable object is scaled in the Y direction

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fill(float amount)
    {
        currentFill += amount * fillRate * Time.deltaTime; // Increase the fill amount based on the fill rate and time
        currentFill = currentFill > maxFill ? maxFill : currentFill; // Clamp the fill amount to the maximum fill level
        // Alternatively ... you can use Mathf.Clamp
        // currentFill = Mathf.Clamp(currentFill, 0, maxFill); // Ensure the fill amount does not exceed the maximum

        //// Update the scale of the fillable object based on the current fill amount
        //Vector3 newScale = fillableObject.localScale;
        //newScale.y = currentFill;
        //fillableObject.localScale = newScale;

        fillableObject.localScale = new Vector3(1, currentFill, 1); // Update the scale of the fillable object based on the current fill amount
    }
}
