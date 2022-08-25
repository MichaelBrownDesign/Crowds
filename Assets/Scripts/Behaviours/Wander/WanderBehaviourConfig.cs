using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boids/Behaviours/Wander")]
public class WanderBehaviourConfig : ScriptableObject
{
    [SerializeField] private float circleRadius     = 1;
    [SerializeField] private float turnChance       = 0.05f;
    [SerializeField] private float maxRadius        = 5;
    [SerializeField] private float mass             = 15;
    [SerializeField] private float maxSpeed         = 3;
    [SerializeField] private float maxForce         = 15;

    public float CircleRadius   => circleRadius;
    public float TurnChance     => turnChance;
    public float MaxRadius      => maxRadius;
    public float Mass           => mass;
    public float MaxSpeed       => maxSpeed;
    public float MaxForce       => maxForce;

}
