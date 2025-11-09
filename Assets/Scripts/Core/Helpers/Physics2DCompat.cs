using UnityEngine;

/// <summary>
/// [Deprecated] Physics2D compatibility layer has been moved to GameServices.
/// This class now forwards calls to the centralized implementation in GameServices
/// to maintain compatibility with existing code.
/// </summary>
public static class Physics2DCompat
{
    /// <summary>
    /// Performs a Physics2D.OverlapCircle with a reusable results buffer and layer filtering.
    /// [Deprecated] Use GameServices.OverlapCircleCompat instead.
    /// </summary>
    public static int OverlapCircle(Vector2 point, float radius, LayerMask mask, Collider2D[] results)
    {
        return GameServices.OverlapCircleCompat(point, radius, mask, results);
    }

    /// <summary>
    /// Performs a 2D raycast using the centralized physics helpers.
    /// [Deprecated] Use GameServices.RaycastCompat instead.
    /// </summary>
    public static bool Raycast(Vector2 origin, Vector2 direction, out RaycastHit2D hit, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers)
    {
        return GameServices.RaycastCompat(origin, direction, out hit, distance, layerMask);
    }

    /// <summary>
    /// Checks if there is a clear line of sight between two points.
    /// [Deprecated] Use GameServices.HasLineOfSight instead.
    /// </summary>
    public static bool HasLineOfSight(Vector2 from, Vector2 to, LayerMask obstacleMask)
    {
        return GameServices.HasLineOfSight(from, to, obstacleMask);
    }
}
