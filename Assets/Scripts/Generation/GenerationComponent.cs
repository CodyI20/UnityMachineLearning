using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class GenerationComponent<T> where T : AgentLogic
{
    [Space(2)]
    public GenerateObjectsInArea generator = null;
    [Space(2)]
    public int parentSize = 0;
    [Space(2)]
    public T[] parents = null;
    [Space(2)]
    public List<T> activeAgents = new List<T>();
    [Space(2)]
    public AgentData lastWinnerData = new AgentData();
    [Space(2)]
    public string savePrefabsAt;
    public T lastWinner { get; private set; }


    /// <summary>
    /// Generates the list of <typeparamref name="T"/> using the parents list. The parent list can be null and, if so, it will be ignored.
    /// Newly created agents will go under mutation (MutationChances and MutationFactor will be applied).
    /// Newly create agents will be Awaken (calling AwakeUp()).
    /// </summary>
    /// <param name="parents"></param>
    public void GenerateObjects(T[] parents = null)
    {
        activeAgents = new List<T>();
        var objects = generator.RegenerateObjects();
        foreach (var agent in objects.Select(obj => obj.GetComponent<T>()).Where(agent => agent != null))
        {
            activeAgents.Add(agent);
            if (parents != null)
            {
                //Changed the generation of the new agents to be based on the last parent. This should create fast, but not very diverse evolution.
                var parent = parents[parents.Length-1];
                //Enable this line to create a random parent for the new agents. This should create a more diverse evolution.
                //var parent = parents[Random.Range(0, parents.Length)];
                agent.Birth(parent.GetData());
            }

            agent.Mutate(GenerationManager.Instance.MutationFactor, GenerationManager.Instance.MutationChance);
            agent.AwakeUp();
        }
    }

    public void NewGen()
    {
        CleanUpNullItems();
        activeAgents.Sort();
        if (activeAgents.Count == 0)
        {
            GenerateObjects();
        }
        parentSize += 1;
        if (parentSize > activeAgents.Count) { parentSize = activeAgents.Count; }
        parents = new T[parentSize];
        for (var i = 0; i < parentSize; i++)
        {
            parents[i] = activeAgents[i];
        }

        lastWinner = activeAgents[0];
        lastWinner.name += "Gen-" + GenerationManager.Instance.GenerationCount;
        lastWinnerData = lastWinner.GetData();
        PrefabUtility.SaveAsPrefabAsset(lastWinner.gameObject, savePrefabsAt + lastWinner.name + ".prefab");

        GenerateObjects(parents);
    }
    public void CleanUpNullItems()
    {
        activeAgents.RemoveAll(item => item == null);
    }
    public void Sleep()
    {
        activeAgents.ForEach(agent => agent.Sleep());
    }
    public void CleanUpAndSleep()
    {
        CleanUpNullItems();
        Sleep();
    }
}
