using System;
using UnityEngine;

public class RoadTrigger : MonoBehaviour
{
    // Event declaration: Fires when the car passes the trigger.
    public static event Action OnCarPassedTrigger;

    private void OnTriggerEnter(Collider OtherCollider)
    {
        // Assuming your car has the tag "Player"
        if (OtherCollider.CompareTag("Player"))
        {
            OnCarPassedTrigger?.Invoke();
        }
    }
}