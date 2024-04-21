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
                var parent = parents[parents.Length-1];
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

        parents = new T[parentSize];
        for (var i = 0; i < parentSize; i++)
        {
            parents[i] = activeAgents[i];
        }

        lastWinner = activeAgents[0];
        lastWinner.name += "Gen-" + GenerationManager.Instance.GenerationCount;
        lastWinnerData = lastWinner.GetData();
        PrefabUtility.SaveAsPrefabAsset(lastWinner.gameObject, GenerationManager.Instance.SavePrefabsAt + lastWinner.name + ".prefab");

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
