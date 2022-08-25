using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Jobs;

public struct Particle
{
    public float3   position;
    public float3   velocity;
    public float    mass;
}

public class BlackHoleSystem : MonoBehaviour
{
    [Header("Rendering")]
    [SerializeField] private Mesh       mesh;
    [SerializeField] private Material   material;

    [Header("Behaviour")]
    [SerializeField] private int        particleCount;
    [SerializeField] private int        batches;
    [SerializeField] private Transform  target;
    [SerializeField] private float      moveSpeed;
    [SerializeField] private float      gravityConstant;
    [SerializeField] private float      maxSpeed;
    [SerializeField] private float      speedColorClamp;
    [SerializeField] private float      blackHoleMass;

    [Header("Particles")]
    [SerializeField] private float      particleSize;
    [SerializeField] private float      spawnRadiusMin;
    [SerializeField] private float      spawnRadiusMax;
    [SerializeField] private float      massMin;
    [SerializeField] private float      massMax;
    [SerializeField] private float      spawnVelocityMin;
    [SerializeField] private float      spawnVelocityMax;


    private NativeArray<Particle>       particles;
    private NativeArray<Matrix4x4>      particleMatrices;
    private NativeArray<float>          particleColors;
    private ComputeBuffer               particleMatrixBuffer;
    private ComputeBuffer               particleColorBuffer;
    private Bounds                      bounds;

    private MaterialPropertyBlock       materialProperties;

    private void OnEnable()
    {
        particles               = new NativeArray<Particle>(particleCount, Allocator.Persistent);
        particleMatrices        = new NativeArray<Matrix4x4>(particleCount, Allocator.Persistent);
        particleColors          = new NativeArray<float>(particleCount, Allocator.Persistent);
        particleMatrixBuffer    = new ComputeBuffer(particleCount, UnsafeUtility.SizeOf<Matrix4x4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        particleColorBuffer     = new ComputeBuffer(particleCount, sizeof(float), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        bounds                  = new Bounds(Vector3.zero, Vector3.one * 10000);

        CopyArrayToBuffer(particleMatrices, particleMatrixBuffer);
        CopyArrayToBuffer(particleColors, particleColorBuffer);

        InitializeUnits();

        materialProperties = new MaterialPropertyBlock();
        materialProperties.SetBuffer("_InstanceBuffer", particleMatrixBuffer);
        materialProperties.SetBuffer("_InstanceColor", particleColorBuffer);
    }

    private void OnDisable()
    {
        particles.Dispose();
        particleMatrices.Dispose();
        particleMatrixBuffer.Dispose();
        particleColors.Dispose();
        particleColorBuffer.Dispose();
    }

    private void CopyArrayToBuffer<T>(NativeArray<T> array, ComputeBuffer buffer) where T : struct
    {
        var bufferNative = buffer.BeginWrite<T>(0, particleCount);
        bufferNative.CopyFrom(array);
        buffer.EndWrite<T>(particleCount);
    }

    private void InitializeUnits()
    {
        Particle particle = new Particle();

        float3 up = new float3(0, 1, 0);

        for(int i = 0; i < particleCount; ++i)
        {
            float dist          = UnityEngine.Random.Range(spawnRadiusMin, spawnRadiusMax);
            float x             = Mathf.Cos(i) * dist;
            float z             = Mathf.Sin(i) * dist;
            float3 pos          = new float3(x, 0, z);
            particle.position   = pos;

            float t = (dist - spawnRadiusMin) / (spawnRadiusMax - spawnRadiusMin);
            t = 1 - t;
            float v = Mathf.Lerp(spawnVelocityMin, spawnVelocityMax, t);
            float3 velocity     =  math.cross(math.normalize(pos), up) * v;
            
            particle.velocity   = velocity;
            particle.mass       = UnityEngine.Random.Range(massMin, massMax);
            particles[i]        = particle;
        }
    }

    private void Update()
    {
        var particleJob = new ParticleJob()
        {
            particles       = particles,
            matrices        = particleMatrices,
            colors          = particleColors,
            target          = target.transform.position,
            deltaTime       = Time.deltaTime,
            moveSpeed       = moveSpeed,
            gravityConstant = gravityConstant,
            maxSpeed        = maxSpeed,
            speedColorClamp = speedColorClamp,
            targetMass      = blackHoleMass,
            particleSize    = particleSize,
        };


        Profiler.BeginSample("Unit Jobs");

        var seekHandle      = particleJob.Schedule(particleCount, particleCount / batches);
        seekHandle.Complete();

        Profiler.EndSample();

        CopyArrayToBuffer(particleMatrices, particleMatrixBuffer);
        CopyArrayToBuffer(particleColors,   particleColorBuffer);

        Profiler.BeginSample("Particle Render");

        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, particleCount, materialProperties);

        Profiler.EndSample();
    }
}
