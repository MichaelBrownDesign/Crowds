using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct BatallionSeekJob : IJobParallelFor
{
    public float3                   target;
    public float                    deltaTime;
    public float                    moveSpeed;
    public float                    arrivalDistance;

    public NativeArray<Unit>        units;
    public NativeArray<Matrix4x4>   matrices;

    public void Execute(int index)
    {
        var unit = units[index];

        float3 delta = target - unit.position;
        float3 direction = math.normalize(delta);
        float distance = math.length(delta);

        float d = math.max(distance, arrivalDistance);
        float speed = math.lerp(0, moveSpeed, distance / d);

        unit.position += direction * speed * deltaTime;
        unit.velocity = direction * (float)math.max(speed, 0.01) * deltaTime;

        units[index] = unit;

        //float3 up = new float3(0, 1, 0);

        //var rotation = quaternion.LookRotation(direction, up);
        //matrices[index] = Matrix4x4.TRS(unit.position, rotation, Vector3.one);

    }
}

[BurstCompile]
public struct BatallionUpdateTransformJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<Unit> units;
    public NativeArray<Matrix4x4> matrices;

    public float3 up;

    public void Execute(int index)
    {
        var unit = units[index];

        var rotation = quaternion.LookRotation(math.normalize(unit.velocity), up);
        matrices[index] = Matrix4x4.TRS(unit.position, rotation, Vector3.one);
    }
}