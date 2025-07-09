using System.Collections;
using UnityEngine;

public class RiceStream : MonoBehaviour
{
    public ParticleSystem splashParticle; // Assign this in Inspector (can be same as main stream)

    private ParticleSystem streamParticles;

    private bool beginPour, endPour;

    public Transform followSource; // Assign this when spawning from PourDetector

   

    private void Awake()
    {
        streamParticles = GetComponent<ParticleSystem>();

        if (splashParticle != null)
            splashParticle.Stop();
    }

    void Update()
    {
        if (beginPour)
        {
            if (!streamParticles.isPlaying)
                streamParticles.Play();

            Vector3 hitPoint = FindEndPoint();

            if (splashParticle != null)
            {
                splashParticle.transform.position = hitPoint;
                splashParticle.gameObject.SetActive(true);
            }
        }
        else if (endPour)
        {
            if (streamParticles.isPlaying)
                streamParticles.Stop();

            if (splashParticle != null)
                splashParticle.Stop();

            Destroy(gameObject, 1.5f); // Delay destroy for fade-out
        }
    }

    public void Begin()
    {
        beginPour = true;
        endPour = false;
    }

    public void End()
    {
        beginPour = false;
        endPour = true;
    }

    private Vector3 FindEndPoint()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);

        Vector3 fallback = ray.GetPoint(2.0f);

        if (Physics.Raycast(ray, out hit, 2.0f))
        {
            // Interact with Fillable container
            RiceFillableController fillable = hit.collider.GetComponent<RiceFillableController>();
            if (fillable != null)
                fillable.Fill(1);

            return hit.point;
        }

        return fallback;
    }
    void LateUpdate()
    {
        // Prevent rotation drift
        transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.down); // aligns local Y with world down

        if (followSource != null)
            transform.position = followSource.position;
    }
}
