using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewVector2GameEvent", menuName = "Events/Vector2GameEvent")]
public class Vector2GameEvent : ScriptableObject
{
    public UnityEvent<Vector2> OnEvent = new UnityEvent<Vector2>();

    public void Raise(Vector2 value)
    {
        if (OnEvent != null)
            OnEvent.Invoke(value);
    }
}
