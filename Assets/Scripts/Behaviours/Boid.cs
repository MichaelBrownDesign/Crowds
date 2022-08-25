using UnityEngine;
using Unity.Mathematics;

public struct Boid
{
    public float3 velocity;
    public float3 wanderForce;
    public float3 target;
    public float3 position;
}