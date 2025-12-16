using UnityEngine;

public class Respawn : MonoBehaviour
{
    Transform respawnPoint;
    void Start()
    {
        respawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").transform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            other.attachedRigidbody.linearVelocity = Vector3.zero;
            other.attachedRigidbody.transform.position = respawnPoint.position;
        }
    }
}
