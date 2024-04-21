using UnityEngine;
using Random = UnityEngine.Random;
public class GenerationManager : Singleton<GenerationManager>
{
    [Header("Generators")]
    [SerializeField]
    private GenerateObjectsInArea[] boxGenerators;
    [SerializeField] private GenerationComponent<BoatLogic> boatGeneration;
    [SerializeField] private GenerationComponent<PirateLogic> pirateGeneration;
    [SerializeField] private GenerationComponent<PoliceLogic> policeGeneration;

    [Space(10)]
    [Header("Parenting and Mutation")]
    [SerializeField]
    private float mutationFactor;

    [SerializeField] private float mutationChance;

    [Space(10)]
    [Header("Simulation Controls")]
    [SerializeField, Tooltip("Time per simulation (in seconds).")]
    private float simulationTimer;

    [SerializeField, Tooltip("Current time spent on this simulation.")]
    private float simulationCount;

    [SerializeField, Tooltip("Automatically starts the simulation on Play.")]
    private bool runOnStart;

    [SerializeField, Tooltip("Initial count for the simulation. Used for the Prefabs naming.")]
    private int generationCount;

    [SerializeField, Tooltip("Seed for the Random Generator.")]
    private int randomGenSeed = 6;

    [Space(10)]
    [Header("Prefab Saving")]
    [SerializeField]
    private string savePrefabsAt;

    private bool _runningSimulation;

    //Public getters
    public float MutationChance => mutationChance;
    public float MutationFactor => mutationFactor;
    public int GenerationCount => generationCount;
    public string SavePrefabsAt => savePrefabsAt;

    protected override void Awake()
    {
        base.Awake();
        Random.InitState(randomGenSeed);
    }

    private void Start()
    {
        if (runOnStart)
        {
            StartSimulation();
        }
    }

    private void Update()
    {
        if (!_runningSimulation) return;
        //Creates a new generation.
        if (simulationCount >= simulationTimer)
        {
            ++generationCount;
            MakeNewGeneration();
            simulationCount = -Time.deltaTime;
        }

        simulationCount += Time.deltaTime;
    }


    /// <summary>
    /// Generates the boxes on all box areas.
    /// </summary>
    public void GenerateBoxes()
    {
        foreach (var generateObjectsInArea in boxGenerators)
        {
            generateObjectsInArea.RegenerateObjects();
        }
    }

    public void GenerateObjects()
    {
        boatGeneration.GenerateObjects();
        pirateGeneration.GenerateObjects();
        policeGeneration.GenerateObjects();
    }

    /// <summary>
    /// Creates a new generation by using GenerateBoxes and GenerateBoats/Pirates/Polices.
    /// Previous generations will be removed and the best parents will be selected and used to create the new generation.
    /// The best parents (top 1) of the generation will be stored as a Prefab in the [savePrefabsAt] folder. Their name
    /// will use the [generationCount] as an identifier.
    /// </summary>
    private void MakeNewGeneration()
    {
        Random.InitState(randomGenSeed);

        GenerateBoxes();
        boatGeneration.NewGen();
        pirateGeneration.NewGen();
        policeGeneration.NewGen();

        // Winners:
        Debug.Log("Last winner boat had: " + boatGeneration.lastWinner.GetPoints() + " points! Last winner pirate had: " +
                  pirateGeneration.lastWinner.GetPoints() + " points! Last winner police had: " +
                  policeGeneration.lastWinner.GetPoints() + " points!");

        //GenerateObjects(_boatParents, _pirateParents, _policeParents);
    }

    /// <summary>
    /// Starts a new simulation. It does not call MakeNewGeneration. It calls both GenerateBoxes and GenerateObjects and
    /// then sets the _runningSimulation flag to true.
    /// </summary>
    public void StartSimulation()
    {
        Random.InitState(randomGenSeed);

        GenerateBoxes();
        GenerateObjects();
        _runningSimulation = true;
    }

    /// <summary>
    /// Continues the simulation. It calls MakeNewGeneration to use the previous state of the simulation and continue it.
    /// It sets the _runningSimulation flag to true.
    /// </summary>
    public void ContinueSimulation()
    {
        MakeNewGeneration();
        _runningSimulation = true;
    }

    /// <summary>
    /// Stops the count for the simulation. It also removes null (Destroyed) boats from the _activeBoats list and sets
    /// all boats, pirates, and polices to Sleep.
    /// </summary>
    public void StopSimulation()
    {
        _runningSimulation = false;
        boatGeneration.CleanUpAndSleep();
        pirateGeneration.CleanUpAndSleep();
        policeGeneration.CleanUpAndSleep();
    }

    //private void CleanupItems(params GenerationComponent<AgentLogic>[] components)
    //{
    //    foreach (var component in components)
    //    {
    //        component.CleanUpNullItems();
    //    }
    //}
}
