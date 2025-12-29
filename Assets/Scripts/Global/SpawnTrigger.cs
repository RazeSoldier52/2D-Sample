using UnityEngine;
using UnityEngine.Events;

public class SpawnTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent onPlayerEnter;
    public void OnTriggerEnter2D(Collider2D other)
    {
        onPlayerEnter?.Invoke();
        Debug.Log("Player has hit the spawn trigger!");
        gameObject.SetActive(false);
    }
}
