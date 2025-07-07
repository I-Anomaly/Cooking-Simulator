using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HandPresencePhysics : MonoBehaviour
{
    public Transform target;
    Rigidbody rb;
    public Renderer nonPhysicalHand;
    public float showNonPhysicalHandDistance = 0.05f; // Distance at which the non-physical hand is shown

    Collider[] handColliders;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        handColliders = GetComponentsInChildren<Collider>();
    }

    public void EnableHandCollider()
    {
        foreach (Collider collider in handColliders)
        {
            collider.enabled = true;
        }
    }

    public void EnableHandColliderDelay(float delay)
    {
        // Enable all colliders after a specified delay
        Invoke("EnableHandCollider", delay);
    }

    public void DisableHandCollider()
    {
        // Disable all colliders when the hand is disabled
        foreach (Collider collider in handColliders)
        {
            collider.enabled = false;
        }
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, target.position);

        // Show or hide the non-physical hand based on distance to the target
        if (distance > showNonPhysicalHandDistance)
        {
            nonPhysicalHand.enabled = true;
        }
        else
        {
            nonPhysicalHand.enabled = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Position the hand at the target's position
        rb.velocity = (target.position - transform.position)/Time.fixedDeltaTime;

        // Rotation
        Quaternion rotationDifference = target.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        Vector3 rotationDifferenceInDegree = angleInDegrees * rotationAxis;

        rb.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);
    }
}
