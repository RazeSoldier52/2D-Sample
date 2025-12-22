using UnityEngine;

public class WorldBoundary : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.TryGetComponent<IBoundaryBehaviour>(out IBoundaryBehaviour handleBoundaryAction))
        {
            handleBoundaryAction.HandleBoundaryBehaviour();
        }
    }
}
