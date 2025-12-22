using UnityEngine;

public class WorldBoundary : MonoBehaviour
{
    public static System.Action OnPlayerFallsOutOfBounds;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            OnPlayerFallsOutOfBounds?.Invoke();
        }
        if(other.TryGetComponent<IBoundaryBehaviour>(out IBoundaryBehaviour handleBoundaryAction))
        {
            handleBoundaryAction.HandleBoundaryBehaviour();
        }
    }
}
