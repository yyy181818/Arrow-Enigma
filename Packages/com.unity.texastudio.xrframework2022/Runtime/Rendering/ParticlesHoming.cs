using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticlesHoming : MonoBehaviour
{
    public Transform target;
    public float force = 10.0f;
    public AnimationCurve forceOverLifetime;

    ParticleSystem ps;
    ParticleSystem.Particle[] particles;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        int maxParticles = ps.main.maxParticles;
        if (particles == null || particles.Length < maxParticles)
        {
            particles = new ParticleSystem.Particle[maxParticles];
        }

        ps.GetParticles(particles);

        Vector3 targetPosition = target.position;
        bool isLocalSpace = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
        if (isLocalSpace)
        {
            targetPosition = transform.InverseTransformPoint(target.position);
        }

        int particleCount = ps.particleCount;
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 targetDirection = Vector3.Normalize(targetPosition - particles[i].position);
            Vector3 seekForce = targetDirection * force * Time.deltaTime;
            float lifetime = (particles[i].startLifetime - particles[i].remainingLifetime) / particles[i].startLifetime; //粒子已使用的寿命百分比
            particles[i].velocity += seekForce * forceOverLifetime.Evaluate(lifetime);
        }

        ps.SetParticles(particles, particleCount);
    }
}