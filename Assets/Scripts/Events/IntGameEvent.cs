using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewIntGameEvent", menuName = "Events/IntGameEvent")]
public class IntGameEvent : ScriptableObject
{
    // UnityEvent field is serialized in the asset so designers can hook listeners in the inspector.
    public UnityEvent<int> OnEvent = new UnityEvent<int>();

    public void Raise(int value)
    {
        if (OnEvent != null)
            OnEvent.Invoke(value);
    }
}
