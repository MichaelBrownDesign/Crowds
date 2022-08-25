using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct ParticleJob : IJobParallelFor
{
    public float3 target;
    public float deltaTime;
    public float moveSpeed;
    public float gravityConstant;
    public float maxSpeed;
    public float speedColorClamp;
    public float targetMass;
    public float particleSize;

    public NativeArray<Particle> particles;
    public NativeArray<Matrix4x4> matrices;
    public NativeArray<float> colors;

    public void Execute(int index)
    {
        float3 up = new float3(0, 1, 0);

        var particle = particles[index];

        float3 delta = target - particle.position;
        float3 direction = math.normalize(delta);
        float  distance = math.length(delta);

        float factor = gravityConstant * ((particle.mass * targetMass) / (distance * distance * distance)) / particle.mass;

        particle.velocity += direction * factor * deltaTime;
        particle.position += particle.velocity * deltaTime;

        particles[index] = particle;

        colors[index] = math.min(1, math.length(particle.velocity) / speedColorClamp);

        var rotation = quaternion.LookRotation(direction, up);
        matrices[index] = Matrix4x4.TRS(particle.position, rotation, Vector3.one * particleSize);

        return;

       // float3 up = new float3(0, 1, 0);

       // var particle = particles[index];

       // float3 delta = target - particle.position;
       // float3 direction = math.normalize(delta);
       // float distance = math.length(delta);
       // float gravityTerm = (particle.mass * targetMass) / (distance * distance);
       // float gravity = gravityTerm * gravityConstant;

       // particle.velocity += direction * moveSpeed * deltaTime;
       // if(math.lengthsq(particle.velocity) > (maxSpeed * maxSpeed))
       // {
       //     particle.velocity = math.normalize(particle.velocity) * maxSpeed;
       // }
       // particle.position += particle.velocity;

       // particles[index] = particle;
       // colors[index] = math.min(1, math.length(particle.velocity) / speedColorClamp);

       // var rotation = quaternion.LookRotation(direction, up);
       //// float scale = math.min(0.05f * particle.mass, 1);
       // matrices[index] = Matrix4x4.TRS(particle.position, rotation, Vector3.one * particleSize);

    }
}
