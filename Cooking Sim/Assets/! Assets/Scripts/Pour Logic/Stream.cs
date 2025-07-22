using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stream : MonoBehaviour
{
    LineRenderer lineRenderer;

    ParticleSystem splashParticle; // Optional: If you want to add particle effects to the stream

    Vector3 targetPosition = Vector3.zero; // The position where the stream is directed

    bool beginPour, endPour; // Flags to control pouring state

    public enum StreamType { Water, Sauce, Oil }
    public StreamType streamType; // Set this in the inspector for each stream object

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        splashParticle = GetComponentInChildren<ParticleSystem>();
    }

    private void Start()
    {
        // 0 and 1 are the indices of the positions in the LineRenderer, it'll stretch the line between these two points
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position); // Initialize the second position to the same as the first
    }

    private void Update()
    {
        if (beginPour)
        {
            targetPosition = FindEndPoint(); // Find the end point where the stream should go

            lineRenderer.SetPosition(0, transform.position);
            AnimateToPosition(1, targetPosition); // Animate the second position towards the target position
        }
        else if (endPour)
        {
            if (!hasReachedPosition(0, targetPosition))
            {
                AnimateToPosition(0, targetPosition); // Animate the first position towards the target position
                AnimateToPosition(1, targetPosition); // Animate the second position towards the target position
            }
            else
            {
                Destroy(gameObject); // Destroy the stream when pouring ends }
            }
        }

        splashParticle.gameObject.transform.position = targetPosition; // Position the splash particle at the pour point
        splashParticle.transform.rotation = Quaternion.identity; // Reset rotation to avoid any unwanted rotation effects
        bool isHitting = hasReachedPosition(1, targetPosition); // Check if the stream is hitting the target position
        splashParticle.gameObject.SetActive(isHitting); // Activate the particle system only if the stream is hitting the target position
    }

    public void Begin()
    {
        beginPour = true;
    }

    public void End()
    {
        beginPour = false;
        endPour = true;
    }

    private Vector3 FindEndPoint()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down); // Cast a ray downwards from the stream's position
        if (Physics.Raycast(ray, out hit, 10f))
        {
            // Debug.Log($"Hit: {hit.collider.gameObject.name} at {hit.point}"); // Log the hit information
            FillableController fillable = hit.collider.gameObject.GetComponent<FillableController>(); // Check if the hit object has a FillableController component, though you could use tags instead later
            if (fillable != null)
            {
                fillable.Fill(1f, streamType); // Pass the stream type
            }
        }
        ; // Adjust the distance as needed

        Vector3 endpoint = hit.collider ? hit.point : ray.GetPoint(2f); // If the ray hits an object, use the hit point; otherwise, use a point 2 units down the ray

        return endpoint; // Return the point where the ray hits an object
    }

    private void AnimateToPosition(int index, Vector3 targetPosition)
    {
        Vector3 currentPosition = lineRenderer.GetPosition(index);
        Vector3 newPoint = Vector3.MoveTowards(currentPosition, targetPosition, Time.deltaTime * 1.75f); // Move towards the target position at a speed of 5 units per second
        lineRenderer.SetPosition(index, newPoint); // Update the position in the LineRenderer
    }

    private bool hasReachedPosition(int index, Vector3 targetPosition)
    {
        Vector3 currentPosition = lineRenderer.GetPosition(index);
        return Vector3.Distance(currentPosition, targetPosition) < 0.01f; // Check if the current position is close enough to the target position
    }
}
