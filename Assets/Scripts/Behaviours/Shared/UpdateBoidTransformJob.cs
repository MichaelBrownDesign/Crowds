using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct UpdateBoidTransformJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<Boid> boids;
    public NativeArray<Matrix4x4> matrices;

    public void Execute(int i)
    {
        float3 up = new float3(0, 1, 0);

        var boid = boids[i];

        var position = new float3(boid.position.x, boid.position.y, boid.position.z);
        var velocity = new float3(boid.velocity.x, 0, boid.velocity.z);
        var rotation = quaternion.LookRotation(math.normalize(velocity), up);
        matrices[i] = Matrix4x4.TRS(position, rotation, Vector3.one);
    }
}
