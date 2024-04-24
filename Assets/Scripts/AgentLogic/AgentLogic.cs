using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Main script for the Agent behaviour.
/// It is responsible for caring its genes, deciding its actions and controlling debug properties.
/// The agent moves by using its rigidBody velocity. The velocity is set to its speed times the movementDirection.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class AgentLogic : MonoBehaviour, IComparable
{
    private Vector3 _movingDirection;
    private Rigidbody _rigidbody;

    [SerializeField] protected float points;

    private bool _isAwake;

    [Space(2)]
    [SerializeField] protected AgentData settings;

    [Space(10)] [Header("Debug & Help")] 
    [SerializeField] private Color visionColor;

    [SerializeField] private Color foundColor;
    [SerializeField] private Color directionColor;

    [SerializeField, Tooltip("Shows visualization rays.")]
    private bool debug;

    #region Static Variables

    private static readonly float _minimalSteps = 5.0f;
    private static readonly float _minimalRayRadius = 10.0f;
    private static readonly float _minimalSight = 2.5f;
    private static readonly float _minimalMovingSpeed = 4.0f;
    private static readonly float _speedInfluenceInSight = 0.1250f;
    private static readonly float _sightInfluenceInSpeed = 0.0125f;
    private static readonly float _maxUtilityChoiceChance = 0.85f;

    #endregion

    private void Awake()
    {
        Initiate();
    }

    /// <summary>
    /// Initiate the values for this Agent, setting its points to 0 and recalculating its sight parameters.
    /// Make sure to override this method in order to add agentType to the settings.
    /// </summary>
    private void Initiate()
    {
        points = 0;
        settings.steps = 360 / settings.rayRadius;
        _rigidbody = GetComponent<Rigidbody>();

        //Force alpha to be 1.0f.
        visionColor.a = 1.0f;
        foundColor.a = 1.0f;
        directionColor.a = 1.0f;
    }

    /// <summary>
    /// Copies the genes / weights from the parent.
    /// </summary>
    /// <param name="parent"></param>
    public void Birth(AgentData parent)
    {
        settings = parent;
    }

    /// <summary>
    /// Has a mutationChance ([0%, 100%]) of causing a mutationFactor [-mutationFactor, +mutationFactor] to each gene / weight.
    /// The chance of mutation is calculated per gene / weight.
    /// </summary>
    /// <param name="mutationFactor">How much a gene / weight can change (-mutationFactor, +mutationFactor)</param>
    /// <param name="mutationChance">Chance of a mutation happening per gene / weight.</param>
    public void Mutate(float mutationFactor, float mutationChance)
    {
        if (Random.Range(0.0f, 100.0f) <= mutationChance)
        {
            settings.steps += (int)Random.Range(-mutationFactor, +mutationFactor);
            settings.steps = (int)Mathf.Max(settings.steps, _minimalSteps);
        }

        if (Random.Range(0.0f, 100.0f) <= mutationChance)
        {
            settings.rayRadius += (int)Random.Range(-mutationFactor, +mutationFactor);
            settings.rayRadius = (int)Mathf.Max(settings.rayRadius, _minimalRayRadius);
        }

        if (Random.Range(0.0f, 100.0f) <= mutationChance)
        {
            var sightIncrease = Random.Range(-mutationFactor, +mutationFactor);
            settings.sight += sightIncrease;
            settings.sight = Mathf.Max(settings.sight, _minimalSight);
            if (sightIncrease > 0.0f)
            {
                settings.movingSpeed -= sightIncrease * _sightInfluenceInSpeed;
                settings.movingSpeed = Mathf.Max(settings.movingSpeed, _minimalMovingSpeed);
            }
        }

        if (Random.Range(0.0f, 100.0f) <= mutationChance)
        {
            var movingSpeedIncrease = Random.Range(-mutationFactor, +mutationFactor);
            settings.movingSpeed += movingSpeedIncrease;
            settings.movingSpeed = Mathf.Max(settings.movingSpeed, _minimalMovingSpeed);
            if (movingSpeedIncrease > 0.0f)
            {
                settings.sight -= movingSpeedIncrease * _speedInfluenceInSight;
                settings.sight = Mathf.Max(settings.sight, _minimalSight);
            }
        }

        CheckMutation(mutationFactor, mutationChance, settings.randomDirectionValue.x, settings.randomDirectionValue.y,
            settings.boatWeight, settings.distanceFactor, settings.boatWeight, settings.boatDistanceFactor, settings.enemyWeight,
            settings.enemyDistanceFactor, settings.policeWeight, settings.policeDistanceFactor);
    }

    /// <summary>
    /// Check if a mutation could happen to the genes / weights and apply it.
    /// The first two <paramref name="values"/> are the mutationFactor and mutationChance, respectively. <paramref name="values"/>[0] = mutationFactor, <paramref name="values"/>[1] = mutationChance.
    /// </summary>
    /// <param name="values"></param>
    private void CheckMutation(params float[] values)
    {
        float mutationFactor = values[0];
        float mutationChance = values[1];
        for(int i= 2; i < values.Length; i++)
        {
            if(Random.Range(0.0f,100.0f) <= mutationChance) { values[i] += Random.Range(-mutationFactor, +mutationFactor); }
        }
    }

    private void Update()
    {
        if (_isAwake)
        {
            Act();
        }
    }

    /// <summary>
    /// Calculate the best direction to move using the Agent properties.
    /// The agent shoots a ray in a area on front of itself and calculates the utility of each one of them based on what
    /// it did intersect or using a random value (uses a Random from [randomDirectionValue.x, randomDirectionValue.y]).
    /// 
    /// </summary>
    private void Act()
    {
        var selfTransform = transform;
        var forward = selfTransform.forward;
        //Ignores the y component to avoid flying/sinking Agents.
        forward.y = 0.0f;
        forward.Normalize();
        var selfPosition = selfTransform.position;

        //Initiate the rayDirection on the opposite side of the spectrum.
        var rayDirection = Quaternion.Euler(0, -1.0f * settings.steps * (settings.rayRadius / 2.0f), 0) * forward;

        //List of AgentDirection (direction + utility) for all the directions.
        var directions = new List<AgentDirection>();
        for (var i = 0; i <= settings.rayRadius; i++)
        {
            //Add the new calculatedAgentDirection looking at the rayDirection.
            directions.Add(CalculateAgentDirection(selfPosition, rayDirection));

            //Rotate the rayDirection by _steps every iteration through the entire rayRadius.
            rayDirection = Quaternion.Euler(0, settings.steps, 0) * rayDirection;
        }

        //Adds an extra direction for the front view with a extra range.
        directions.Add(CalculateAgentDirection(selfPosition, forward, 1.5f));

        directions.Sort();
        //There is a (100 - _maxUtilityChoiceChance) chance of using the second best option instead of the highest one. Should help into ambiguous situation.
        var highestAgentDirection = directions[Random.Range(0.0f, 1.0f) <= _maxUtilityChoiceChance ? 0 : 1];

        //Rotate towards to direction. The factor of 0.1 helps to create a "rotation" animation instead of automatically rotates towards the target. 
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(highestAgentDirection.Direction), 0.1f);

        //Sets the velocity using the chosen direction
        _rigidbody.velocity = highestAgentDirection.Direction * settings.movingSpeed;

        if (debug)
        {
            Debug.DrawRay(selfPosition, highestAgentDirection.Direction * (settings.sight * 1.5f), directionColor);
        }
    }

    private AgentDirection CalculateAgentDirection(Vector3 selfPosition, Vector3 rayDirection, float sightFactor = 1.0f)
    {
        if (debug)
        {
            Debug.DrawRay(selfPosition, rayDirection * settings.sight, visionColor);
        }

        //Calculate a random utility to initiate the AgentDirection.
        var utility = Random.Range(Mathf.Min(settings.randomDirectionValue.x, settings.randomDirectionValue.y),
            Mathf.Max(settings.randomDirectionValue.x, settings.randomDirectionValue.y));

        //Create an AgentDirection struct with a random utility value [utility]. Ignores y component.
        var direction = new AgentDirection(new Vector3(rayDirection.x, 0.0f, rayDirection.z), utility);

        //Raycast into the rayDirection to check if something can be seen in that direction.
        //The sightFactor is a variable that increases / decreases the size of the ray.
        //For now, the sightFactor is only used to control the long sight in front of the agent.
        if (Physics.Raycast(selfPosition, rayDirection, out RaycastHit raycastHit, settings.sight * sightFactor))
        {
            if (debug)
            {
                Debug.DrawLine(selfPosition, raycastHit.point, foundColor);
            }

            //Calculate the normalized distance from the agent to the intersected object.
            //Closer objects will have distancedNormalized close to 0, and further objects will have it close to 1.
            var distanceNormalized = (raycastHit.distance / (settings.sight * sightFactor));

            //Inverts the distanceNormalized. Closer objects will tend to 1, while further objects will tend to 0.
            //Thus, closer objects will have a higher value.
            var distanceIndex = 1.0f - distanceNormalized;

            //Calculate the utility of the found object according to its type.
            utility = raycastHit.collider.gameObject.tag switch
            {
                //All formulas are the same. Only the weights change.
                "Box" => distanceIndex * settings.distanceFactor + settings.boxWeight,
                "Boat" => distanceIndex * settings.boatDistanceFactor + settings.boatWeight,
                "Enemy" => distanceIndex * settings.enemyDistanceFactor + settings.enemyWeight,
                "Police" => distanceIndex * settings.policeDistanceFactor + settings.policeWeight,
                _ => utility
            };
        }

        direction.utility = utility;
        return direction;
    }

    /// <summary>
    /// Activates the agent update method.
    /// Does nothing if the agent is already awake.
    /// </summary>
    public void AwakeUp()
    {
        _isAwake = true;
    }

    /// <summary>
    /// Stops the agent update method and sets its velocity to zero.
    /// Does nothing if the agent is already sleeping.
    /// </summary>
    public void Sleep()
    {
        _isAwake = false;
        _rigidbody.velocity = Vector3.zero;
    }

    public float GetPoints()
    {
        return points;
    }

    /// <summary>
    /// Compares the points of two agents. When used on Sort function will make the highest points to be on top.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        var otherAgent = obj as AgentLogic;
        if (otherAgent != null)
        {
            return otherAgent.GetPoints().CompareTo(GetPoints());
        }

        throw new ArgumentException("Object is not an AgentLogic");
    }

    /// <summary>
    /// Returns the AgentData of this Agent.
    /// </summary>
    /// <returns></returns>
    public AgentData GetData()
    {
        return settings;
    }
}