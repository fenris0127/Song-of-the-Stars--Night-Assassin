using UnityEngine;

/// <summary>
/// Simple input publisher that raises movement commands via a ScriptableObject event (Vector2GameEvent).
/// Attach to a singleton InputManager or the player and assign a `Vector2GameEvent` asset to `movementCommandEvent`.
/// This decouples input from PlayerController and allows multiple listeners to react to the same commands.
/// </summary>
public class PlayerInputPublisher : MonoBehaviour
{
    public Vector2GameEvent movementCommandEvent;

    void Update()
    {
        Vector2 dir = Vector2.zero;

        if (Input.GetKeyDown(KeyCode.W)) dir = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.S)) dir = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.A)) dir = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.D)) dir = Vector2.right;

        if (dir != Vector2.zero && movementCommandEvent != null)
            movementCommandEvent.Raise(dir);
    }
}
