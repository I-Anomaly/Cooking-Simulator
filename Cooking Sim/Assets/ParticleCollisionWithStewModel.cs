using UnityEngine;

public class ParticleCollisionWithStewModel : MonoBehaviour
{
    private ParticleSystem particleSystem;

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    void OnParticleCollision(GameObject other)
    {
        // Check if the collided object is named "StewModel"
        if (other.name == "StewModel")
        {
            // Get all particles
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.particleCount];
            int numParticles = particleSystem.GetParticles(particles);

            // Set lifetime to 0 for all particles (effectively deletes them)
            for (int i = 0; i < numParticles; i++)
            {
                particles[i].remainingLifetime = 0;
            }

            // Apply changes back to the particle system
            particleSystem.SetParticles(particles, numParticles);
        }
    }
}