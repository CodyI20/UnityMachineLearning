using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This struct stores all genes / weights from an Agent.
/// It is used to pass this information along to other Agents, instead of using the MonoBehavior itself.
/// Also, it makes it easier to inspect since it is a Serializable struct.
/// </summary>
[Serializable]
public struct AgentData
{
    //What the agent can interact with
    public List<AgentInteractableType> interactableType;

    [Header("Genes")]
    [Tooltip("Steps for the area of sight.")]
    public int steps;

    [Range(0.0f, 360.0f), Tooltip("Divides the 360˚ view of the Agent into rayRadius steps.")]
    public int rayRadius;

    [Tooltip("Ray distance. For the front ray, the value of 1.5 * Sight is used.")]
    public float sight;

    public float movingSpeed;

    [Tooltip("All directions starts with a random value from X-Y (Math.Abs, Math.Min and Math.Max are applied).")]
    public Vector2 randomDirectionValue;

    [Space(10)]
    [Header("Weights")]
    //Interactable is Box
    public float boxWeight;
    public float distanceFactor;
    //Interactable is Boat
    public float boatWeight;
    public float boatDistanceFactor;
    //Interactable is Enemy
    public float enemyWeight;
    public float enemyDistanceFactor;
    //Interactable is Police
    public float policeWeight;
    public float policeDistanceFactor;

    public AgentData(int steps, int rayRadius, float sight, float movingSpeed, Vector2 randomDirectionValue,
        float boxWeight, float distanceFactor, float boatWeight, float boatDistanceFactor, float enemyWeight,
        float enemyDistanceFactor, float policeWeight, float policeDistanceFactor, List<AgentInteractableType> interactableTypes)
    {
        this.steps = steps;
        this.rayRadius = rayRadius;
        this.sight = sight;
        this.movingSpeed = movingSpeed;
        this.randomDirectionValue = randomDirectionValue;
        this.boxWeight = boxWeight;
        this.distanceFactor = distanceFactor;
        this.boatWeight = boatWeight;
        this.boatDistanceFactor = boatDistanceFactor;
        this.enemyWeight = enemyWeight;
        this.enemyDistanceFactor = enemyDistanceFactor;
        this.policeWeight = policeWeight;
        this.policeDistanceFactor = policeDistanceFactor;
        this.interactableType = interactableTypes;
    }
}
