using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AgentLogicSettings
{
    //What the agent can interact with
    public List<AgentInteractableType> interactableType;
    public AgentData agentData;
}
