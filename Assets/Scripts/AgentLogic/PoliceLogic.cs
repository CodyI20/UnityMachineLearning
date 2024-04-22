using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class PoliceLogic : AgentLogic
{
    #region Static Variables
    
    private static readonly float _piratePoints = 10.0f;
    private static readonly float _boatPoints = 3.5f;

    #endregion

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Enemy"))
        {
            points += _piratePoints;
            Destroy(other.gameObject);
        }
        else if (other.collider.CompareTag("Boat"))
        {
            points += _boatPoints;
            Destroy(other.gameObject);
        }
    }
}
