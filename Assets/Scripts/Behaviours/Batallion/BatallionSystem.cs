using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Jobs;

public struct Unit
{
    public float3 position;
    public float3 velocity;
}

public class BatallionSystem : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private MeshVertexAnimations   animations;
    [SerializeField] private int                    defaultAnimation;
    [SerializeField] private Material               material;

    [Header("Behaviour")]
    [SerializeField] private int                    unitCount;
    [SerializeField] private int                    batches;
    [SerializeField] private Transform              target;
    [SerializeField] private float                  moveSpeed;


    private NativeArray<Unit>                       units;
    private NativeArray<Matrix4x4>                  unitMatrices;
    private ComputeBuffer                           unitMatrixBuffer;
    private VertexAnimationRenderer                 unitRenderer;
    private Bounds                                  bounds;

    private void OnEnable()
    {
        units               = new NativeArray<Unit>(unitCount, Allocator.Persistent);
        unitMatrices        = new NativeArray<Matrix4x4>(unitCount, Allocator.Persistent);
        unitMatrixBuffer    = new ComputeBuffer(unitCount, UnsafeUtility.SizeOf<Matrix4x4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        unitRenderer        = new VertexAnimationRenderer(animations, material, defaultAnimation);
        unitRenderer.Enable();

        bounds              = new Bounds(Vector3.zero, Vector3.one * 10000);

        CopyMatricesToBuffer();

        InitializeUnits();
    }

    private void OnDisable()
    {
        units.Dispose();
        unitMatrices.Dispose();
        unitMatrixBuffer.Dispose();
        unitRenderer.Disable();
    }

    private void CopyMatricesToBuffer()
    {
        var bufferNative = unitMatrixBuffer.BeginWrite<Matrix4x4>(0, unitCount);
        bufferNative.CopyFrom(unitMatrices);
        unitMatrixBuffer.EndWrite<Matrix4x4>(unitCount);
    }

    private void InitializeUnits()
    {
        Unit unit = new Unit();

        int w = (int)Mathf.Sqrt(unitCount);

        for(int i = 0; i < unitCount; ++i)
        {
            float x = i % w;
            float z = i / w;
            unit.position = new float3(x, 0, z);
            units[i] = unit;
        }
    }

    private void Update()
    {
        var seekJob = new BatallionSeekJob()
        {
            units           = units,
            matrices        = unitMatrices,
            target          = target.transform.position,
            deltaTime       = Time.deltaTime,
            moveSpeed       = moveSpeed,
            arrivalDistance = 10,
        };

        var transformJob = new BatallionUpdateTransformJob()
        {
            units       = units,
            matrices    = unitMatrices,
            up          = new float3(0, 1, 0),
        };

        Profiler.BeginSample("Unit Jobs");

        var seekHandle      = seekJob.Schedule(unitCount, unitCount / batches);
        var transformHandle = transformJob.Schedule(unitCount, unitCount / batches, seekHandle);

        JobHandle.CompleteAll(ref seekHandle, ref transformHandle);

        Profiler.EndSample();

        CopyMatricesToBuffer();

        Profiler.BeginSample("Unit Render");

        unitRenderer.Draw(unitCount, unitMatrixBuffer, bounds);

        Profiler.EndSample();
    }
}
