using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PirateLogic : AgentLogic
{
    #region Static Variables

    private static readonly float _boxPoints = 0.1f;
    private static readonly float _boatPoints = 5.0f;
    private static readonly float _policePoints = -120.0f;

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.tag.Equals("Box")) return;
        points += _boxPoints;
        Destroy(other.gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag.Equals("Boat"))
        {
            points += _boatPoints;
            Destroy(other.gameObject);
        }

        else if (other.gameObject.tag.Equals("Police"))
        {
            points += _policePoints;
        }
    }
}