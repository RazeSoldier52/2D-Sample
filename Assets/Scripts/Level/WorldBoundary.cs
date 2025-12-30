using UnityEngine;

public class WorldBoundary : MonoBehaviour
{
    public static System.Action OnPlayerFellOutOfBounds;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.TryGetComponent<IBoundaryBehaviour>(out IBoundaryBehaviour handleBoundaryAction))
        {
            handleBoundaryAction.HandleBoundaryBehaviour();
        }
        if (other.gameObject.CompareTag("Player"))
        {
            OnPlayerFellOutOfBounds?.Invoke();
        }
    }
}
