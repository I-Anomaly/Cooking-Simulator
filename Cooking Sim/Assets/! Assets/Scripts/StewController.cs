using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StewController : MonoBehaviour
{
    List<Vector3> velocityBuffer = new List<Vector3>();
    public float agitationScale = 10;
    public float agitationFactor = 100;


    // Update is called once per frame
    void Update()
    {
        agitationFactor = Mathf.Lerp(agitationFactor,0, 0.05f);
        gameObject.transform.Rotate(Vector3.up, agitationFactor * agitationScale);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.name.Contains("Spoon"))
        {
            // Limit our buffer to 10 values
            if (velocityBuffer.Count > 10)
            {
                velocityBuffer.RemoveAt(0);
            }

            velocityBuffer.Add(collision.relativeVelocity);

            for (int i = 0; i < velocityBuffer.Count; i++)
            {
                agitationFactor += Vector3.Magnitude(velocityBuffer[i]);
            }
            agitationFactor /= velocityBuffer.Count;

        }
    }
}
