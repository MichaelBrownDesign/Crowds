using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

public class BoidSystem : MonoBehaviour
{
    [SerializeField] private int innerLoopBatchCount = 64;
    [SerializeField] private BoidSystemConfig config;

    [Header("Animation")]
    [SerializeField] private MeshVertexAnimations   meshAnimations;
    [SerializeField] private Material               material;
    [SerializeField] private int                    defaultAnimation;


    private VertexAnimationRenderer boidRenderer;

    private NativeArray<Boid>   boids;

    private Unity.Mathematics.Random random;
    private NativeArray<Matrix4x4> boidMatricesNative;

    private ComputeBuffer boidBuffer;
    private ComputeBuffer positionsBuffer;
    MaterialPropertyBlock matProperties;

    private int batchCount;

    private void Awake()
    {
        random          = new Unity.Mathematics.Random(0x10101010);

        boidRenderer    = new VertexAnimationRenderer(meshAnimations, material, defaultAnimation);
        boidRenderer.Enable();

        batchCount      = config.BoidCount / 1023 + 1;

        boids           = new NativeArray<Boid>(config.BoidCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);

        boidMatricesNative  = new NativeArray<Matrix4x4>(config.BoidCount, Allocator.Persistent);
        boidBuffer          = new ComputeBuffer(config.BoidCount, UnsafeUtility.SizeOf<Matrix4x4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        positionsBuffer     = new ComputeBuffer(config.BoidCount, sizeof(float) * 3, ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        matProperties       = new MaterialPropertyBlock();
        matProperties.SetBuffer("_InstanceBuffer", boidBuffer);

        var native = boidBuffer.BeginWrite<Matrix4x4>(0, boidMatricesNative.Length);
        native.CopyFrom(boidMatricesNative);
        boidBuffer.EndWrite<Matrix4x4>(boidMatricesNative.Length);

        Boid boid = new Boid();

        for (int i = 0; i < config.BoidCount; ++i)
        {
            boid.velocity       = UnityEngine.Random.onUnitSphere;
            SetRandomWanderForce(ref boid);
            boid.position       = random.NextFloat3Direction() * config.Wander.MaxRadius;
            boids[i]            = boid;
        }
    }

    private void OnDestroy()
    {
        boids.Dispose();
        boidMatricesNative.Dispose();
        boidBuffer.Release();
        positionsBuffer.Release();
        boidRenderer.Disable();
    }

    private Bounds boidBounds = new Bounds(Vector3.zero, new Vector3(10000, 10000, 10000));

    private void Update()
    {
        Profiler.BeginSample("Boids");
        var boidJob = new WanderJob();
        boidJob.circleRadius = config.Wander.CircleRadius;
        boidJob.turnChance = config.Wander.TurnChance;
        boidJob.maxRadius = config.Wander.MaxRadius * config.Wander.MaxRadius;
        boidJob.mass = config.Wander.Mass;
        boidJob.maxSpeed = config.Wander.MaxSpeed * config.Wander.MaxSpeed;
        boidJob.maxForce = config.Wander.MaxForce * config.Wander.MaxForce;
        boidJob.boids = boids;
        boidJob.deltaTime = Time.deltaTime;
        boidJob.random = random;


        UpdateBoidTransformJob transJob = new()
        {
            boids       = boids,
            matrices    = boidMatricesNative,
        };

        var boidHandle = boidJob.Schedule(boids.Length, innerLoopBatchCount);

        var transHandle = transJob.Schedule(config.BoidCount, config.BoidCount / 20, boidHandle);

        JobHandle.CompleteAll(ref boidHandle, ref transHandle);

        var nativeBuffer = boidBuffer.BeginWrite<Matrix4x4>(0, boidBuffer.count);
        boidMatricesNative.CopyTo(nativeBuffer);
        boidBuffer.EndWrite<Matrix4x4>(boidBuffer.count);

        Profiler.EndSample();

        Profiler.BeginSample("Render boids");

        boidRenderer.Draw(config.BoidCount, boidBuffer, boidBounds);

        Profiler.EndSample();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetRandomWanderForce(ref Boid boid)
    {
        var circleCenter = Unity.Mathematics.math.normalize(boid.velocity);
        var randomPoint = UnityEngine.Random.insideUnitCircle;

        var displacement = new Unity.Mathematics.float3(randomPoint.x, 0, randomPoint.y) * config.Wander.CircleRadius;
        displacement = math.mul(quaternion.LookRotation(boid.velocity, new float3(0, 1, 0)), displacement);

        boid.wanderForce = circleCenter + displacement;
    }

    private void OnDrawGizmos()
    {
        for(int i = 0; i < boidMatricesNative.Length; ++i)
        {
            Gizmos.matrix = boidMatricesNative[i];
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }
    }
}
