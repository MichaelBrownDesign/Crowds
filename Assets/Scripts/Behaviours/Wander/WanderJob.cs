using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct WanderJob : IJobParallelFor
{
    public float circleRadius;
    public float turnChance;
    public float maxRadius;
    public float mass;
    public float maxSpeed;
    public float maxForce;

    public float deltaTime;
    public Random random;

    public NativeArray<Boid> boids;

    public void Execute(int i)
    {
        var boid = boids[i];

        if (math.lengthsq(boid.position) > maxRadius)
        {
            var directionToCenter = math.normalize(boid.target - boid.position);
            boid.wanderForce = directionToCenter;
        }
        else
        {
            float angle = random.NextFloat(-1, 1) * circleRadius * deltaTime;
            boid.wanderForce = math.rotate(quaternion.AxisAngle(new float3(0, 1, 0), angle), boid.wanderForce);
        }
        var steeringForce = boid.wanderForce - boid.velocity;

        if (math.lengthsq(steeringForce) > maxForce)
        {
            steeringForce = math.normalize(steeringForce) * maxSpeed;
        }

        boid.velocity += steeringForce;
        boid.velocity.y = 0;
        float velocitySqLen = math.lengthsq(boid.velocity);
        if (velocitySqLen > maxSpeed)
        {
            boid.velocity = math.normalize(boid.velocity) * maxSpeed;
        }
        else if(velocitySqLen <= 0)
        {
            boid.velocity = new float3(0, 0, 1);
        }

        boid.position += boid.velocity * deltaTime;
        boid.position.y = 0;
        boids[i] = boid;


        //float3 velocityNorm = math.normalize(boid.velocity);

        //// Wander Force
        //if (math.lengthsq(boid.position) > maxRadius)
        //{
        //    var directionToCenter = math.normalize(boid.target - boid.position);
        //    boid.wanderForce = velocityNorm + directionToCenter;
        //}
        //else if (random.NextFloat() < turnChance)
        //{
        //    // Random Wander force
        //    var randomPoint = random.NextFloat2Direction();

        //    var displacement = new float3(randomPoint.x, 0 , randomPoint.y) * circleRadius;

        //    displacement = math.mul(quaternion.LookRotation(boid.velocity, new float3(0, 1, 0)), displacement);
        //    displacement.y = 0;
        //    var circleCenter = velocityNorm + displacement;
        //    boid.wanderForce = circleCenter;
        //}

        //boid.wanderForce.y = 0;
        //boid.wanderForce = math.normalize(boid.wanderForce) * maxSpeed;

        //var steeringForce = boid.wanderForce - boid.velocity;

        //if (math.lengthsq(steeringForce) > maxForce)
        //{
        //    steeringForce = math.normalize(steeringForce) * maxSpeed;
        //}

        //steeringForce /= mass;

        //boid.velocity += steeringForce;
        //if (math.lengthsq(boid.velocity) > maxSpeed)
        //{
        //    boid.velocity = math.normalize(boid.velocity) * maxSpeed;
        //}

        //boid.position += boid.velocity * deltaTime;

        //boids[i] = boid;
    }
}
