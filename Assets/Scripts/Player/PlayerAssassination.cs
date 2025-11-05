using UnityEngine;

public class PlayerAssassination : MonoBehaviour
{
    [Header("Assassination Settings")]
    public float assassinationRange = 1.5f; 
    public float maxRange = 15f;            
    public LayerMask guardMask;

    public GuardRhythmPatrol FindGuardInAssassinationRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, assassinationRange, guardMask);
        if (hits.Length > 0)
        {
            return hits[0].GetComponent<GuardRhythmPatrol>();
        }
        return null;
    }

    public void ExecuteAssassinationStrike(GuardRhythmPatrol target)
    {
        if (target != null)
            target.Die(); 
    }

    public void ExecuteRangedAssassination(GuardRhythmPatrol target)
    {
        if (target != null)
            target.Die(); 
    }
}