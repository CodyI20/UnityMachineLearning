using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PoliceLogic : AgentLogic
{
    #region Static Variables
    
    private static float _piratePoints = 10.0f;
    private static float _boatPoints = 3.5f;

    #endregion

    private void OnCollisionEnter(Collision other)
    {
        switch (other.gameObject.tag)
        {
            case "Pirate":
                points += _piratePoints;
                Destroy(other.gameObject);
                break;
            case "Boat":
                points += _boatPoints;
                Destroy(other.gameObject);
                break;
            default:
                return;
        }
    }
}
