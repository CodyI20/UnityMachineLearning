using System;
using UnityEngine;
/// <summary>
/// This struct helps to order the directions an Agent can take based on its utility.
/// Every Direction (a vector to where the Agent would move) has a utility value.
/// Higher utility values are expected to lead to better outcomes.
/// </summary>
struct AgentDirection : IComparable
{
    public Vector3 Direction { get; }
    public float utility;

    public AgentDirection(Vector3 direction, float utility)
    {
        Direction = direction;
        this.utility = utility;
    }

    /// <summary>
    /// Notices that this method is an "inverse" sorting. It makes the higher values on top of the Sort, instead of
    /// the smaller values. For the smaller values, the return line would be utility.CompareTo(otherAgent.utility).
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        AgentDirection otherAgent = (AgentDirection)obj;
        return otherAgent.utility.CompareTo(utility);
    }
}
