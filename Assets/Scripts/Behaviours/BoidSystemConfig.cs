using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boids/Boid System Config")]
public class BoidSystemConfig : ScriptableObject
{
    [SerializeField] private int                    boidCount;
    [SerializeField] private float                  spawnAreaRadius;
    [SerializeField] private WanderBehaviourConfig  wander;

    public int                      BoidCount           =>  boidCount;
    public float                    SpawnAreaRadius     =>  spawnAreaRadius;
    public WanderBehaviourConfig    Wander              =>  wander;
}
